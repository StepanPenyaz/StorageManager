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

### Notes on CORS during local development

The backend registers a CORS policy (`LocalDev`) that explicitly allows the Vite dev-server origin `http://localhost:5173`. The policy is applied before HTTPS redirection so that browser preflight (`OPTIONS`) requests receive the correct `Access-Control-Allow-Origin` header and are not blocked.

**Recommended approach (zero CORS friction):** always let the Vite proxy forward API calls.  
Any `fetch('/api/...')` call from the frontend is proxied by Vite to `http://localhost:5000` and no cross-origin request is made in the browser. CORS headers are only needed when calling the API directly (e.g. via `curl` or from a different port/origin).

**Verifying the preflight manually:**

```bash
curl -i -X OPTIONS "http://localhost:5000/api/storage/init" \
  -H "Origin: http://localhost:5173" \
  -H "Access-Control-Request-Method: POST"
```

Expected response: `HTTP 204` with `Access-Control-Allow-Origin: http://localhost:5173`.