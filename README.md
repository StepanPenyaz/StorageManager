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

**1.2. Seed initial data**

Open `Documents/Scripts/initial_data_seed.sql` in SQL Server Management Studio (or `sqlcmd`) and execute it against the `StorageManager` database.

Using `sqlcmd`:

```bash
sqlcmd -S localhost\SQLEXPRESS -d StorageManager -E -i "Documents/Scripts/initial_data_seed.sql"
```

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