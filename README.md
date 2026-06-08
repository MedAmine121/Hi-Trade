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
- **SQL Server** - Primary data store
- **JWT Authentication** - Secure token-based authentication
- **Serilog** - Structured logging
- **Redis** - Caching and session management
- **Swagger/OpenAPI** - API documentation
- **FluentValidation** - Input validation
- **Stripe** - Payment processing

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

- .NET 8 SDK or later
- SQL Server (or SQL Server LocalDB)
- Node.js 18+ (for Angular frontend)
- Redis (optional, for caching)
- Stripe account (for payment processing)
- Finnhub API key (for stock prices)

### Installation & Setup

#### 1. Database Configuration

Update `appsettings.json` with your SQL Server connection string:

```json
"ConnectionStrings": {
  "HiTradeConnection": "Server=YOUR_SERVER;Database=HiTrade;Trusted_Connection=true;"
}
```

#### 2. JWT Configuration

Configure JWT settings in `appsettings.json`:

```json
"JWTOptions": {
  "Secret": "your_secret_key_here",
  "Issuer": "your_issuer",
  "Audience": "your_audience",
  "ExpirationMinutes": 60
}
```

#### 3. External Services Configuration

Configure required external services:

```json
"FinnhubToken": "your_finnhub_api_key",
"stripeRestrictedKey": "your_stripe_key",
"Redis": {
  "EndpointUrl": "localhost",
  "EndpointPort": "6379",
  "User": "",
  "Password": ""
},
"AngularUrl": "http://localhost:4200"
```

#### 4. Run Migrations

Apply Entity Framework Core migrations:

```bash
dotnet ef database update
```

#### 5. Start Backend

```bash
cd Hi-Trade
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger UI at `https://localhost:5001/swagger`

#### 6. Start Frontend (Optional)

```bash
cd Hi-Trade.Angular
npm install
ng serve
```

Angular application will be available at `http://localhost:4200`

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

**Note**: This project is currently on the `unit-tests` branch. Development branches may contain experimental features.