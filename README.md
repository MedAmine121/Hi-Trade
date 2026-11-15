# Hi-Trade

A comprehensive stock trading portfolio management application built with .NET 8 and Angular, enabling users to manage investment portfolios, buy/sell assets, and track their investments in real-time.

## Project Overview

Hi-Trade is a full-stack web application that provides a complete trading platform where users can:
- Create and manage multiple investment portfolios
- Buy and sell stocks and other tradable assets
- Monitor real-time asset prices
- Manage account funds and make deposits
- Track portfolio performance

## Tech Stack

### Backend
- **.NET 8** - Modern ASP.NET Core framework
- **Entity Framework Core 8** - ORM for database access
- **SQL Server 2022** - Primary relational data store (Docker container)
- **Redis 7** - Distributed caching, session revocation & performance optimization (Docker container)
- **MailHog** - Local SMTP testing & web-based email viewer (Docker container)
- **JWT Authentication** - Secure token-based authentication with Redis revocation
- **Serilog** - Structured logging (Console & Rolling File)
- **Swagger/OpenAPI** - Interactive API documentation
- **FluentValidation** - Robust request model validation
- **Stripe** - Payment processing & wallet funding
- **Finnhub** - Real-time market quotes and automated price synchronization

### Frontend
- **Angular** - Progressive web application framework

## Project Architecture

The project follows a layered architecture pattern:

```
Hi-Trade/                    # Main API project
??? Hi-Trade.Models/         # Data models and DTOs
??? Hi-Trade.DAL/            # Data Access Layer (EF Core contexts)
??? Hi-Trade.BLL/            # Business Logic Layer
??? Hi-Trade.Services/       # Service layer with business operations
??? Hi-Trade.Common/         # Shared utilities and helpers
??? Hi-Trade.Tests/          # Unit and integration tests
??? Hi-Trade.Angular/        # Angular frontend application
```

### Layer Descriptions

- **DAL (Data Access Layer)**: Handles all database operations using Entity Framework Core with SQL Server
- **BLL (Business Logic Layer)**: Contains core business rules and logic
- **Services**: Implements service interfaces for user management, portfolio management, asset management, etc.
- **Models**: Contains request/response DTOs, validators, and common types
- **Common**: Shared utilities, configuration options, and helpers

## Key Features

### 1. User Management
- **Sign Up**: Create new user accounts
- **Login/Logout**: Secure authentication with JWT tokens
- **User Profile**: View and manage user information
- **Fund Management**: Add funds to trading account via Stripe

**Endpoints:**
- `POST /user/signup` - Register new user
- `POST /user/login` - Authenticate user
- `POST /user/logout` - Logout user
- `GET /user/fetch` - Get current user details
- `POST /user/checkout` - Add funds to account

### 2. Portfolio Management
- **Create Portfolio**: Set up new investment portfolios
- **View Portfolios**: List all user portfolios
- **Buy Assets**: Purchase stocks/assets for a portfolio
- **Track Holdings**: Monitor current portfolio holdings

**Endpoints:**
- `GET /portfolio/get` - Retrieve user portfolios
- `POST /portfolio/create` - Create new portfolio
- `POST /portfolio/buyasset` - Buy asset in portfolio
- `POST /portfolio/sellasset` - Sell asset from portfolio

### 3. Asset Management
- **Create Assets**: Admin can add new tradable assets (Stocks, etc.)
- **View Assets**: Get list of available enabled assets
- **Price Updates**: Real-time price updates via Finnhub API
- **Background Service**: Automated price synchronization

**Endpoints:**
- `POST /asset/create` - Create new asset (Admin only)
- `GET /asset/get` - List all available assets

## Getting Started

### Prerequisites

- **Docker & Docker Compose** (for running SQL Server, Redis, and MailHog containers)
- **.NET 8 SDK** or later
- **Node.js 18+** (for the Angular frontend)
- **Stripe Account** (for payment processing / funding)
- **Finnhub API Key** (for real-time stock price synchronization)

---

### 1. Start Docker Infrastructure (SQL Server, Redis & MailHog)

Hi-Trade includes a `docker-compose.yml` that provisions all local development services:
- **SQL Server 2022**: Relational database engine on port `1433`.
- **Redis 7 (Alpine)**: In-memory cache & JWT token revocation store on port `6379`.
- **MailHog**: Local mock SMTP server on port `1025` with a Web UI inbox on port `8025`.
- **Automated Seeder (`db-seed`)**: Initializes schema migrations and seeds the current asset catalog once SQL Server is healthy.

Start all containers in detached mode:

```bash
docker compose up -d
```

Verify the status of all containers:

```bash
docker compose ps
```

#### Service Endpoints & Credentials

| Service | Port | Host & Credentials / URL | Description |
| :--- | :--- | :--- | :--- |
| **SQL Server 2022** | `1433` | Host: `localhost,1433`<br>User: `sa`<br>Password: `HiTrade@2026Password!`<br>Database: `HiTrade` | Relational database storage |
| **Redis 7** | `6379` | Host: `localhost:6379` *(no password required in dev)* | Token revocation & response caching |
| **MailHog SMTP** | `1025` | Host: `localhost:1025` | Local mock SMTP server |
| **MailHog Web UI** | `8025` | [http://localhost:8025](http://localhost:8025) | Webmail viewer to inspect outgoing emails |

---

### 2. Database Migrations & Initial Asset Seeding

On first startup with `docker compose up -d`, the `db-seed` container automatically applies all Entity Framework Core migrations and populates the `[Assets]` table with the current platform assets.

#### Running or Re-Running the Seed Script Manually

If you need to seed or re-seed the database manually at any point, several convenient methods are available:

- **PowerShell (Windows)**:
  ```powershell
  ./scripts/seed-assets.ps1
  ```

- **Bash (Linux / macOS)**:
  ```bash
  chmod +x ./scripts/seed-assets.sh
  ./scripts/seed-assets.sh
  ```

- **Docker Compose Service**:
  ```bash
  docker compose run --rm db-seed
  ```

- **Direct SQL Execution via `docker exec`**:
  ```bash
  docker exec -i hitrade-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "HiTrade@2026Password!" -C -i /scripts/seed-assets.sql
  ```

- **Apply EF Core Migrations via .NET CLI**:
  ```bash
  dotnet ef database update --project Hi-Trade.DAL --startup-project Hi-Trade
  ```

#### Current Platform Assets Seeded:

The seed script idempotently populates the following major stock assets:
- `AAPL` — Apple Inc.
- `MSFT` — Microsoft Corporation
- `GOOGL` — Alphabet Inc.
- `AMZN` — Amazon.com Inc.
- `TSLA` — Tesla Inc.
- `NVDA` — NVIDIA Corporation
- `META` — Meta Platforms Inc.
- `NFLX` — Netflix Inc.
- `BRK.A` — Berkshire Hathaway Inc.
- `NIO` — NIO Inc.

*(When the backend runs, `PriceBackgroundService` automatically fetches live quote updates for all enabled assets from Finnhub).*

---

### 3. Application Configuration

In `Hi-Trade/appsettings.Development.json`, settings are pre-configured to connect seamlessly to the local Docker services:

```json
{
  "ConnectionStrings": {
    "HiTradeConnection": "Server=localhost,1433;Database=HiTrade;User Id=sa;Password=HiTrade@2026Password!;TrustServerCertificate=True;Encrypt=False;"
  },
  "Redis": {
    "EndpointUrl": "localhost",
    "Port": 6379,
    "User": "",
    "Password": ""
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": 1025
  }
}
```

Configure your personal keys in `appsettings.Development.json`:
- `FinnhubToken`: Your Finnhub API token
- `stripeRestrictedKey`: Your Stripe test restricted key
- `AdminSeed`: Default admin account credentials (seeded automatically)

---

### 4. Start the Backend API

```bash
cd Hi-Trade
dotnet run
```

The API will be available at:
- **API URL**: `https://localhost:5001` or `http://localhost:5000`
- **Swagger UI**: [https://localhost:5001/swagger](https://localhost:5001/swagger)

---

### 5. Start Frontend Application (Optional)

```bash
cd Hi-Trade.Angular
npm install
ng serve
```

The Angular application will be available at [http://localhost:4200](http://localhost:4200).

## API Documentation

The API is documented using Swagger/OpenAPI. Once the backend is running, access the interactive documentation at:

```
https://localhost:5001/swagger
```

### Authentication

Most endpoints (except signup and login) require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer YOUR_JWT_TOKEN
```

## Testing

Unit and integration tests are located in the `Hi-Trade.Tests` project. Run tests using:

```bash
dotnet test Hi-Trade.Tests
```

## Configuration

### Logging

Serilog is configured for structured logging with both console and file output. Logs are stored in the `Logs/` directory with daily rolling files.

### CORS

CORS is enabled for all origins. Update the CORS policy in `Program.cs` for production use:

```csharp
policy.WithOrigins("specific-domain.com")
      .AllowAnyHeader()
      .AllowAnyMethod();
```

### Background Services

The application includes a `PriceBackgroundService` that automatically updates asset prices at regular intervals using the Finnhub API.

## Project Structure

```
Hi-Trade/
??? Controllers/
?   ??? UserController.cs
?   ??? PortfolioController.cs
?   ??? AssetController.cs
??? Program.cs
??? appsettings.json
??? appsettings.Development.json

Hi-Trade.Models/
??? Requests/
??? Responses/
??? Validators/
??? Common/

Hi-Trade.DAL/
??? Context/
??? Repositories/
??? DI/

Hi-Trade.BLL/
??? BLL/
??? DI/

Hi-Trade.Services/
??? Interfaces/
??? Services/
??? DI/

Hi-Trade.Tests/
??? Unit tests
```

## Deployment

### Docker

Build and run with Docker:

```bash
docker build -t hi-trade .
docker run -p 5001:5001 hi-trade
```

### Azure/Cloud

The project is designed to be cloud-ready and can be deployed to Azure App Service, AWS, or other cloud providers supporting .NET 8.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Use Cases

### Individual Investor
Track multiple investment portfolios, diversify assets, and monitor overall investment performance.

### Financial Advisor
Manage client portfolios through multiple portfolio accounts and track performance metrics.

### Investment Enthusiast
Practice trading strategies with real-time market data and build hypothetical portfolios.

## Troubleshooting

### Connection String Issues
Ensure SQL Server is running and the connection string in `appsettings.json` is correct.

### JWT Token Errors
Verify that JWT configuration (Secret, Issuer, Audience) is properly set in `appsettings.json`.

### Missing External Services
Ensure Finnhub API key and Stripe key are configured for full functionality.

### CORS Errors
Check that the Angular URL in `appsettings.json` matches your frontend URL.

## License

This project is open source and available under the MIT License.

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/MedAmine121/Hi-Trade).

---

**Note**: This project is currently on the `docker-setup` branch. Development branches may contain experimental features.