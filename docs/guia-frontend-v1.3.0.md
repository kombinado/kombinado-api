# Guia de integração do app — API v1.3.0

Este guia lista o que mudou na API (branch `develop` do `kombinado-api`) e o que precisa ser ajustado no `kombinado-expo`.

> **Prazo:** a v1.3.0 vai para o Azure **até 10/10, antes da demonstração**. A partir daí, o cadastro de motorista só funciona com o ajuste da [seção 1](#1--obrigatório-enviar-vehicletotalseats-no-cadastro-de-motorista).

## Resumo

| # | Mudança | Prioridade | Arquivos no app |
|---|---|---|---|
| 1 | Cadastro de motorista exige `vehicleTotalSeats` | 🔴 Obrigatório | `src/app/(auth)/signup.tsx` |
| 2 | Signup valida nome, WhatsApp, curso e placa | 🟡 Recomendado | `src/app/(auth)/signup.tsx` |
| 3 | Criar carona valida origem/destino e vagas (1 a 8) | 🟡 Recomendado | `src/components/feature/CreateRideModal.tsx` |
| 4 | Todo erro vem no envelope `ApiResponse` em português | ✅ Já funciona | `services/api.ts` |
| 5 | Refresh com token inválido retorna 401 (antes 500) | ✅ Já funciona | `services/api.ts` |
| 6 | Perfil mostra vagas do veículo + novo endpoint de edição | 🟢 Opcional | `src/hooks/useProfile.ts`, `src/app/(private)/profile.tsx` |

---

## Como testar com a versão da `develop`

No repositório da API:

```bash
git checkout develop && git pull
docker compose up -d --build
```

No `.env` do app:

```env
# Emulador Android
EXPO_PUBLIC_API_URL=http://10.0.2.2:8080
# Celular físico (mesma rede Wi-Fi)
# EXPO_PUBLIC_API_URL=http://<IP-da-máquina>:8080
```

O Swagger fica em `http://localhost:8080/swagger`.

---

## 1. 🔴 Obrigatório: enviar `vehicleTotalSeats` no cadastro de motorista

O motorista agora informa quantas **vagas para passageiros** o veículo tem, **de 1 a 8**, sem contar o motorista. Sem esse campo, o `POST /api/Auth/signup` de motorista retorna:

```json
{
  "success": false,
  "message": "O número de vagas do veículo deve ser entre 1 e 8.",
  "data": null,
  "statusCode": 400
}
```

O cadastro de passageiro não muda: para passageiros, a API ignora os campos do veículo.

### O que mudar em `signup.tsx`

1. Criar o estado junto dos outros campos do motorista:

   ```tsx
   const [vehicleTotalSeats, setVehicleTotalSeats] = useState("");
   ```

2. Adicionar um input numérico abaixo de "Placa", no mesmo estilo dos outros:

   ```tsx
   <TextInput
     placeholder="Vagas para passageiros (1 a 8)"
     value={vehicleTotalSeats}
     onChangeText={(text) => setVehicleTotalSeats(text.replace(/\D/g, ""))}
     keyboardType="number-pad"
     maxLength={1}
     className={inputClassName}
     placeholderTextColor="#A1A1AA"
     editable={!isSubmitting}
   />
   ```

3. Incluir o campo na validação local do motorista:

   ```tsx
   const seats = Number(vehicleTotalSeats);
   if (isDriver && (!vehicleModel || !vehicleColor || !vehiclePlate || seats < 1 || seats > 8)) {
     const msg = "Por favor, preencha todas as informações do veículo (vagas de 1 a 8)!";
     // ...mesmo tratamento que já existe
   }
   ```

4. Enviar o campo no payload como **número**:

   ```tsx
   ...(isDriver
     ? {
         vehicleModel,
         vehicleColor,
         vehiclePlate,
         vehicleTotalSeats: Number(vehicleTotalSeats),
       }
     : {}),
   ```

---

## 2. 🟡 Signup agora valida os dados

A API passou a validar e normalizar todos os campos do cadastro, com as mesmas regras da edição de perfil:

| Campo | Regra na API | Normalização feita pela API |
|---|---|---|
| `name` | Obrigatório, até 100 caracteres | Remove espaços nas pontas |
| `whatsApp` | DDD + número, **sem +55**, 10 ou 11 dígitos | Remove a máscara (`(34) 99999-8888` → `34999998888`) |
| `course` | Obrigatório, até 100 caracteres | Remove espaços nas pontas |
| `vehicleModel` | Motorista: obrigatório, até 50 caracteres | Remove espaços nas pontas |
| `vehicleColor` | Motorista: obrigatório, até 30 caracteres | Remove espaços nas pontas |
| `vehiclePlate` | Motorista: formato antigo `ABC1234` ou Mercosul `ABC1D23` | Aceita hífen e minúsculas; salva `ABC1D23` |
| `vehicleTotalSeats` | Motorista: de 1 a 8 | — |

Se algo estiver errado, a API devolve 400 e uma mensagem pronta para exibir, por exemplo:
- `"Informe um WhatsApp válido com DDD (10 ou 11 dígitos, sem o +55)."`
- `"Placa inválida. Use o formato ABC1234 ou ABC1D23."`

O app já mostra o `message` no `Alert`, então nada quebra. Sugestões para melhorar a experiência:
- trocar o placeholder da placa para `"Placa (ABC1234 ou ABC1D23)"`;
- usar `autoCapitalize="characters"` e `maxLength={8}` no input da placa;
- o WhatsApp já é enviado sem máscara, então não precisa mudar nada.

---

## 3. 🟡 Criar carona: novas validações

`POST /api/Rides` agora retorna 400 nestes casos, nesta ordem:

| Situação | Mensagem |
|---|---|
| Origem ou destino vazio | `Informe a origem e o destino da carona.` |
| Origem ou destino com mais de 200 caracteres | `A origem e o destino devem ter no máximo 200 caracteres.` |
| Origem igual ao destino (ignora maiúsculas) | `A origem e o destino devem ser diferentes.` |
| Horário no passado | `O horário de partida deve ser uma data futura.` |
| Vagas fora de 1 a 8 | `A carona deve oferecer entre 1 e 8 vagas.` |
| Vagas acima das vagas do veículo | `A carona não pode oferecer mais vagas do que o seu veículo possui (4).` |

Em `CreateRideModal.tsx`, o campo "Vagas" usa `maxLength={1}` e por isso aceita `0` e `9`. A API recusa os dois com mensagem, mas dá para avisar antes de enviar:

```tsx
const spotsNumber = parseInt(spots, 10);
const canSubmit =
  Boolean(origin && destination && time) &&
  spotsNumber >= 1 && spotsNumber <= 8 &&
  !isSubmitting;
```

Se quiser limitar o campo às vagas do veículo, use o `vehicleTotalSeats` do `GET /api/auth/me` (ver seção 6).

---

## 4. ✅ Erros sempre no envelope `ApiResponse`

Antes, um JSON malformado ou um campo com tipo errado devolviam o `ProblemDetails` padrão do .NET, em inglês. Agora **todo erro** vem neste formato:

```json
{
  "success": false,
  "message": "Dados da requisição inválidos. Verifique se o corpo é um JSON válido e se os campos têm o tipo correto. Campo: totalSeats.",
  "data": null,
  "statusCode": 400
}
```

O `services/api.ts` já lê o `data.message` primeiro, então **não precisa mudar nada**. O fallback para `data.errors` e `data.title` pode ficar, mas não é mais usado.

Erros 500 continuam com `"Ocorreu um erro interno no servidor. Tente novamente mais tarde."`. A diferença é que agora ficam registrados no log da API: se acontecer na demonstração, avise o horário e a rota para o backend investigar.

---

## 5. ✅ Refresh com token inválido retorna 401

`POST /api/Auth/refresh` com token malformado agora retorna **401** com `"Sessão expirada. Por favor, faça login novamente."`. Antes retornava 500.

O `services/api.ts` já desloga o usuário quando o refresh falha (`refreshRes.ok === false`), então **não precisa mudar nada**.

---

## 6. 🟢 Opcional: vagas no perfil e edição de perfil

### `GET /api/auth/me` agora retorna `vehicleTotalSeats`

```json
{
  "id": "3c536c47-...",
  "name": "Maria Silva",
  "email": "maria.silva@estudante.iftm.edu.br",
  "course": "Engenharia de Software",
  "whatsApp": "34999998888",
  "isDriver": true,
  "vehicleModel": "Onix",
  "vehicleColor": "Prata",
  "vehiclePlate": "ABC1D23",
  "vehicleTotalSeats": 4
}
```

Para usar esse campo:
- adicionar `vehicleTotalSeats: number | null` em `AppProfile` (`src/hooks/useProfile.ts`);
- exibir o valor no card do veículo em `profile.tsx`.

O campo vem `null` para passageiros e para motoristas cadastrados antes dele existir.

### Novo endpoint: `PUT /api/users/profile`

Edita nome, WhatsApp, curso e, para motoristas, os dados do veículo. Requer o token (`Authorization: Bearer <token>`).

```json
{
  "name": "Maria Silva",
  "whatsApp": "34999998888",
  "course": "Engenharia de Software",
  "vehicleModel": "Onix",
  "vehicleColor": "Prata",
  "vehiclePlate": "ABC1D23",
  "vehicleTotalSeats": 4
}
```

- A atualização é **completa**: envie todos os campos, não só os alterados.
- Valem as mesmas regras e mensagens da [seção 2](#2--signup-agora-valida-os-dados).
- E-mail e o tipo de conta (motorista/passageiro) **não** podem ser alterados.
- Para passageiros, a API ignora os campos do veículo.
- Responde 200 com o perfil atualizado em `data`, com `message` igual a `"Perfil atualizado com sucesso."`.
- Depois de salvar, recarregue o perfil com `GET /api/auth/me`. O nome dentro do token só é atualizado no próximo login.

---

## Checklist de teste

- [ ] Cadastro de motorista com vagas de 1 a 8 → sucesso.
- [ ] Cadastro de motorista sem vagas → mensagem de erro exibida (validação local ou da API).
- [ ] Cadastro com placa `abc-1d23` → sucesso; o perfil mostra `ABC1D23`.
- [ ] Cadastro com placa inválida → mensagem `"Placa inválida..."` exibida.
- [ ] Cadastro de passageiro → continua funcionando como antes.
- [ ] Criar carona com 0 vagas, ou com mais vagas que o veículo → mensagem de erro exibida.
- [ ] Criar carona com origem igual ao destino → mensagem de erro exibida.
- [ ] Criar carona válida → aparece na lista do motorista.

## Referências

- **README da API**, seções *Authentication Domain*, *Rides Domain*, *Users Domain* e *Validation and Server Errors* (há versão em português no final do arquivo).
- **Collection do Postman** "Kombinado API", pastas Auth, Rides, Requests e Users. Cada request tem a descrição completa com exemplos e erros.
