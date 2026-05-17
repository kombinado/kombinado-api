# Kombinado API 🚗
Release v0.1.0

<details>
<summary>🇧🇷 <b>Leia em Português</b></summary>

## Sobre o Projeto
A API do Kombinado é o serviço de backend para um aplicativo de caronas acadêmicas. Ela fornece a infraestrutura principal para que estudantes e funcionários compartilhem caronas, gerenciando usuários, motoristas e a disponibilidade das viagens de forma eficiente.

## Tecnologias Utilizadas
- **.NET 8 (ASP.NET Core)**
- **Entity Framework Core (EF Core)**
- **PostgreSQL**
- **JWT (JSON Web Tokens)**

## Pré-requisitos
Para rodar este projeto localmente, você precisa ter o seguinte instalado na sua máquina:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker e Docker Compose](https://www.docker.com/products/docker-desktop) (para rodar o banco de dados PostgreSQL)

## Configuração de Ambiente (.env)
Antes de rodar a aplicação, você precisa configurar as variáveis de ambiente.
Crie um arquivo `.env` na raiz do projeto (você pode usar o arquivo `.env.example` como base) e preencha com a sua configuração local. Aqui está um exemplo:

```env
# 1. Postgres Configuration
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=kombinado_db

# 2. Pg Admin Configuration
PGADMIN_DEFAULT_EMAIL=admin@kombinado.com
PGADMIN_DEFAULT_PASSWORD=admin

# 3. Connection String for the API
CONNECTION_STRING=Host=localhost;Database=kombinado_db;Username=postgres;Password=postgres;

# 4. JWT Configuration
JWT_SECRET=sua_chave_secreta_jwt_aqui_deve_ser_longa_o_suficiente
JWT_EXPIRE_MINUTES=60
JWT_REFRESH_EXPIRE_DAYS=7
JWT_ISSUER=Kombinado
JWT_AUDIENCE=KombinadoApp
```

## Como Rodar o Projeto

Siga este passo a passo exato para clonar e rodar a API na sua máquina em menos de 5 minutos:

1. **Suba o banco de dados PostgreSQL** usando o Docker Compose na raiz do projeto:
   ```bash
   docker-compose up -d
   ```

2. **Navegue até a pasta da API**:
   ```bash
   cd Kombinado.Api
   ```

3. **Aplique as migrações do EF Core** para criar as tabelas no banco de dados:
   ```bash
   dotnet ef database update
   ```

4. **Rode a aplicação**:
   ```bash
   dotnet run
   ```

A API estará rodando localmente! Verifique a saída do terminal para descobrir em qual porta ela subiu (geralmente `http://localhost:5000` ou `https://localhost:5001`).

## Documentação de Endpoints (Swagger)

Quando a aplicação estiver rodando, você pode acessar a interface do Swagger para testar os endpoints acessando a rota:  
👉 `http://localhost:<porta>/swagger`

### Principais Endpoints

#### 🔐 Domínio de Autenticação
*Estas rotas não precisam de autenticação.*
- `POST /api/Auth/signup` - Cadastra um novo usuário.
- `POST /api/Auth/login` - Autentica um usuário e retorna um token JWT.
- `POST /api/Auth/refresh` - Atualiza um JWT expirado usando um refresh token.

#### 🚗 Domínio de Caronas/Motorista
*⚠️ **Todas as rotas abaixo precisam do cabeçalho `Authorization: Bearer <token>`.***
- `POST /api/Rides` - Cria uma nova carona (Apenas motorista).
- `GET /api/Rides` - Lista todas as caronas disponíveis (Qualquer usuário autenticado).
- `GET /api/Rides/me/driving` - Lista todas as caronas onde o usuário logado é o motorista (Apenas motorista).
- `PATCH /api/Rides/{rideId}/cancel` - Cancela uma carona específica (Apenas motorista).

<hr>
</details>

<br>

## About the Project
Kombinado API is the backend service for an academic carpooling application. It provides the core infrastructure for students and staff to share rides, efficiently managing users, drivers, and ride availability.

## Technologies Used
- **.NET 8 (ASP.NET Core)**
- **Entity Framework Core (EF Core)**
- **PostgreSQL**
- **JWT (JSON Web Tokens)**

## Prerequisites
To run this project locally, you need to have the following installed on your machine:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker and Docker Compose](https://www.docker.com/products/docker-desktop) (for running the PostgreSQL database)

## Environment Setup (.env)
Before running the application, you need to configure the environment variables. 
Create a `.env` file in the root of the project (you can copy the provided `.env.example`) and fill it with your local configuration. Here is an example:

```env
# 1. Postgres Configuration
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=kombinado_db

# 2. Pg Admin Configuration
PGADMIN_DEFAULT_EMAIL=admin@kombinado.com
PGADMIN_DEFAULT_PASSWORD=admin

# 3. Connection String for the API
CONNECTION_STRING=Host=localhost;Database=kombinado_db;Username=postgres;Password=postgres;

# 4. JWT Configuration
JWT_SECRET=your_super_secret_jwt_key_here_must_be_long_enough
JWT_EXPIRE_MINUTES=60
JWT_REFRESH_EXPIRE_DAYS=7
JWT_ISSUER=Kombinado
JWT_AUDIENCE=KombinadoApp
```

## How to Run the Project

Follow these exact steps to get the API running on your machine in less than 5 minutes:

1. **Start the PostgreSQL database** using Docker Compose from the project root:
   ```bash
   docker-compose up -d
   ```

2. **Navigate to the API folder**:
   ```bash
   cd Kombinado.Api
   ```

3. **Apply EF Core Migrations** to create the database schema:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**:
   ```bash
   dotnet run
   ```

The API should now be running locally! Check your console output for the exact URL (usually `http://localhost:5000` or `https://localhost:5001`).

## Endpoint Documentation (Swagger)

When the application is running, you can access the Swagger UI to interact with the endpoints by navigating to:  
👉 `http://localhost:<port>/swagger`

### Main Endpoints Overview

#### 🔐 Authentication Domain
*These routes do not require authentication.*
- `POST /api/Auth/signup` - Register a new user.
- `POST /api/Auth/login` - Authenticate a user and receive a JWT.
- `POST /api/Auth/refresh` - Refresh an expired JWT using a refresh token.

#### 🚗 Rides & Driver Domain
*⚠️ **All routes below require the `Authorization: Bearer <token>` header.***
- `POST /api/Rides` - Create a new ride (Driver only).
- `GET /api/Rides` - List all available rides (Any authenticated user).
- `GET /api/Rides/me/driving` - List all rides where the current user is the driver (Driver only).
- `PATCH /api/Rides/{rideId}/cancel` - Cancel a specific ride (Driver only).