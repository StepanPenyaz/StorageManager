# StorageManager

## Local run

This guide describes how to run the StorageManager project on a local machine.

### Prerequisites

Make sure the following software is installed before proceeding:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (instance name: `SQLEXPRESS`)

### 1. Database setup

**1.1. Apply EF Core migrations**

From the repository root, run the following command to create the database schema:

```bash
dotnet ef database update --project Backend/Storage/Storage.Data --startup-project Backend/Storage/Storage.Api
```

This creates the `StorageManager` database on your local `localhost\SQLEXPRESS` instance using Windows Integrated Security.

### 2. Backend

Navigate to the API project folder and start the development server:

```bash
cd Backend/Storage/Storage.Api
dotnet run
```

The API will be available at `http://localhost:5000`.

### 3. Frontend

In a separate terminal, install dependencies and start the frontend development server:

```bash
cd Frontend
npm install
npm run dev
```

The application will be available at `http://localhost:5173`.

> The Vite development server automatically proxies all `/api` requests to the backend at `http://localhost:5000`.

### 4. CORS in local development

The backend registers a `LocalDev` CORS policy that allows the Vite dev-server origin `http://localhost:5173` with credentials. This policy is intentionally scoped to `localhost` only and must **not** be used in production.

If you call the API directly (bypassing the Vite proxy), use `https://localhost:52515` as the base URL so the HTTPS redirect does not break preflight. OPTIONS preflight requests are short-circuited before the HTTPS redirect, so they always return `204 No Content` with the correct CORS headers.

**Verify preflight with curl:**

```bash
curl -i -X OPTIONS "https://localhost:52515/api/storage/init" \
  -H "Origin: http://localhost:5173" \
  -H "Access-Control-Request-Method: POST" \
  --insecure
```

Expected response headers:
- `HTTP/1.1 204 No Content`
- `Access-Control-Allow-Origin: http://localhost:5173`
- `Access-Control-Allow-Credentials: true`