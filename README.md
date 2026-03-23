# StorageManager

A web-based physical storage management system for warehouse cabinets. It tracks containers organized into cabinets, shelves, and container groups, links inventory lots to sections, and supports bulk storage initialization, BSX file import, and XML-based order/update processing.

---

## Table of Contents

- [Getting Started](#getting-started)
  - [Docker Quick Start (Demo)](#docker-quick-start-demo)
  - [Local Run](#local-run)
- [Architecture and Project Structure](#architecture-and-project-structure)
- [Usage Examples and Screenshots](#usage-examples-and-screenshots)
- [Tech Stack and Main Dependencies](#tech-stack-and-main-dependencies)
- [GitHub Copilot Features Used](#github-copilot-features-used)
- [Development](#development)
- [Roadmap and Future Improvements](#roadmap-and-future-improvements)
- [License](#license)

---

## Getting Started

### Docker Quick Start (Demo)

Run the full stack locally with a single command — no manual DB setup required.

### Prerequisites

- [Docker](https://www.docker.com/get-started) with Docker Compose

### Commands

```bash
# Start all services (db, api, frontend)
docker-compose up --build -d

# View API logs
docker-compose logs -f api

# Stop all services
docker-compose down
```

Once running:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000

Data persists between restarts via a Docker volume for MSSQL.

> **Note:** The values in `docker-compose.yml` are for demo/local use only. Do not use them in production.
> To customise ports or credentials, copy `docker-compose.override.example.yml` to `docker-compose.override.yml` and adjust as needed.

---

### Local Run

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

---

## Architecture and Project Structure

```
StorageManager/
├── Backend/Storage/
│   ├── Storage.Api/        — ASP.NET Core Web API (controllers, Program.cs)
│   ├── Storage.Data/       — EF Core DbContext, Fluent API configurations, repository
│   ├── Storage.Domain/     — Domain models, value objects, enums, business rules
│   ├── Storage.Services/   — Application services (initialization, order/update processing, views)
│   └── Storage.Tests/      — xUnit unit tests for domain and services
├── Frontend/               — Vite + React + TypeScript SPA (Redux Toolkit / RTK Query)
└── Documents/
    ├── Copilot/            — Copilot instructions and prompt history
    └── Design documents/   — Domain, data, UI, and feature requirements
```

### Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Domain | `Storage.Domain` | Entities, value objects, enums, domain behaviour |
| Data | `Storage.Data` | EF Core context, Fluent API configurations, repository |
| Services | `Storage.Services` | Business logic — initialization, order/update processing, storage views |
| API | `Storage.Api` | ASP.NET Core controllers, DTOs, dependency registration |
| Frontend | `Frontend/` | React + Redux SPA, RTK Query API client |
| Tests | `Storage.Tests` | xUnit unit tests for domain models and file parsers |

### Key API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/storage/cabinets` | List all cabinets |
| `GET` | `/api/storage/cabinets/{id}/containers` | List containers in a cabinet |
| `POST` | `/api/storage/initialize` | Initialize storage structure |
| `POST` | `/api/storage/bsx-metadata` | Retrieve BSX file metadata |
| `POST` | `/api/storage/process-bsx` | Process a BSX inventory file |
| `POST` | `/api/storage/process-orders` | Process outbound order files |
| `POST` | `/api/storage/process-storage-updates` | Process inbound storage-update files |

---

## Usage Examples and Screenshots

### Storage View

The main screen displays all cabinets and their containers grouped by type. Each container shows its number, type, and section occupancy.

### Storage Initialization Wizard

A 4-step wizard guides the user through:

1. **Initial settings** — configure the container number starting index.
2. **Cabinet and shelf configuration** — define cabinet count, shelf layout, and container group row types.
3. **Initialization preview and execution** — review generated container numbering and create the storage structure.
4. **BSX file processing** — import initial inventory lots from a `.bsx` file.

### Order and Storage-Update Processing

Dedicated panels allow triggering file-based order processing (outbound) and storage-update processing (inbound) with per-file status feedback.

---

## Tech Stack and Main Dependencies

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10 | Runtime and SDK |
| ASP.NET Core | 10 | Web API framework |
| Entity Framework Core | 9.x | ORM and migrations |
| SQL Server / MSSQL | — | Relational database |
| xUnit | 2.x | Unit testing |

### Frontend

| Technology | Version | Purpose |
|------------|---------|---------|
| Vite | 6.x | Build tool and dev server |
| React | 19.x | UI framework |
| TypeScript | 5.x | Static typing |
| Redux Toolkit | 2.x | State management |
| RTK Query | (bundled) | API data fetching and caching |

### Infrastructure

| Technology | Purpose |
|------------|---------|
| Docker + Docker Compose | Containerized local and demo deployment |
| MSSQL Docker image | Database container for demo environment |

---

## GitHub Copilot Features Used

This project was built with extensive use of GitHub Copilot throughout all phases:

- **Copilot Chat (agent mode)** — Used as the primary driver for all code generation. Each feature was implemented by providing a structured prompt referencing design documents, with Copilot generating domain models, data layer, services, controllers, and frontend components end-to-end.
- **Custom instructions (`.github/copilot-instructions.md`)** — A repository-level instructions file was created to give Copilot persistent context about the project's architecture, coding conventions, domain terminology, and folder structure. This reduced prompt verbosity and improved consistency across sessions.
- **Multi-file code generation** — Copilot generated entire feature slices in a single session: domain entity → EF configuration → repository method → service → controller → RTK Query slice → React component.
- **Iterative refinement** — Design documents were used as requirements; Copilot's output was reviewed per PR, edge cases were documented in design files, and Copilot was re-prompted with updated requirements.
- **Test generation** — xUnit tests for domain models and file parsers were generated by Copilot using existing code as context, covering constructor validation, domain rules, and XML parsing edge cases.
- **Documentation** — This README, the phase summary, and the `.github/copilot-instructions.md` file were all drafted by Copilot based on the accumulated design documents and prompt history.

---

## Development

### Running Tests

```bash
cd Backend/Storage
dotnet test Storage.Tests
```

All 39 unit tests cover:
- Domain models: `Location`, `Container`, `ContainerGroup`, `LotSection`
- File parsers: `OrderFileParser`, `StorageUpdateFileParser`

### Linting and Formatting

The project follows the conventions defined in `Documents/Copilot/copilot_instructions.md`:
- Modern C# — file-scoped namespaces, primary constructors, expression-bodied members
- No XML documentation comments
- Fluent API for all EF Core configurations
- Redux Style Guide for frontend TypeScript/React code

No additional linting tools are configured beyond the .NET SDK analyser and TypeScript compiler checks.

### Frontend Type Check

```bash
cd Frontend
npm run build
```

---

## Roadmap and Future Improvements

| Priority | Improvement |
|----------|-------------|
| High | User authentication and role-based access control |
| High | Automated integration and end-to-end tests |
| Medium | Audit logging for all inventory changes |
| Medium | Reporting and analytics dashboards (lot history, occupancy rates) |
| Medium | Lot movement and split operations via the UI |
| Low | Performance indexing on location fields |
| Low | Real-time updates (SignalR) for concurrent warehouse users |
| Low | Mobile-friendly responsive layout |

---

## License

This project is provided for demonstration and educational purposes. No explicit open-source license has been assigned.