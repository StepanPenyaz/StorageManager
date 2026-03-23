# Local Environment Requirements

## Prerequisites

The following software must be installed before running the project locally:

| Software | Version | Notes |
|---|---|---|
| .NET SDK | 10.0 or later | Required to build and run the backend |
| Node.js | 18.0 or later | Required to run the frontend |
| SQL Server Express | 2019 or later | Required database engine |
| SQL Server Management Studio (optional) | Any | For database administration |

## Database Requirements

- SQL Server Express instance must be running and accessible at `localhost\SQLEXPRESS`
- Windows Integrated Security must be enabled
- A database named `StorageManager` must exist
- The schema must be created using EF Core migrations (`dotnet ef database update`)

## Backend Requirements

- API must run and listen on `http://localhost:5000`
- The connection string in `appsettings.json` must point to the local SQL Server Express instance
- `ASPNETCORE_ENVIRONMENT` should be set to `Development`

## Frontend Requirements

- Dependencies must be installed via `npm install` from the `Frontend` folder
- The development server must be started via `npm run dev` from the `Frontend` folder
- The Vite development server proxies all `/api` requests to `http://localhost:5000`
- The frontend application is accessible at `http://localhost:5173` by default

## Environment Setup Order

1. Start SQL Server Express
2. Apply database migrations
3. Start the backend API
4. Start the frontend development server
