# GitHub Copilot Instructions — StorageManager

## 1. Project Overview

StorageManager is a physical storage management system for warehouse cabinets.
It manages the assignment of lots (inventory) to containers organized into cabinets, shelves, and container groups.

Main components:
- **Storage.Api** — ASP.NET Core Web API (`/api/storage/`)
- **Storage.Data** — EF Core data layer (SQL Server)
- **Storage.Domain** — Domain models and business logic
- **Storage.Services** — Application services (order processing, storage initialization)
- **Frontend** — Vite + React + TypeScript SPA (Redux Toolkit / RTK Query)

---

## 2. Technology Stack

**Backend**
- .NET 10, ASP.NET Core Web API
- Entity Framework Core with SQL Server
- Clean Architecture / Domain-Driven Design

**Frontend**
- Vite + React + TypeScript
- Redux Toolkit with RTK Query
- Dev proxy: `/api` → `http://localhost:5000`

---

## 3. Solution Structure

```
Backend/Storage/
  Storage.Api/          — Web API, controllers, Program.cs
  Storage.Data/         — DbContext, EF configurations, repository
  Storage.Domain/       — Domain models (entities, value objects)
  Storage.Services/     — Application services
Frontend/               — React SPA (src/features/, src/app/)
Documents/
  Copilot/              — copilot_instructions.md, copilot_prompts_history.md
  Design documents/     — domain, data, UI, and feature requirements
```

---

## 4. Coding Conventions

Refer to `Documents/Copilot/copilot_instructions.md` for the complete rules.
Key points:

- Use .NET 10 and modern C# (file-scoped namespaces, primary constructors)
- No comments or XML documentation
- Use `var` when the type is obvious; `required` + `init` for properties
- No regions; prefer ternary `?:` over simple `if` statements
- Place interfaces in `Interfaces/`, models in `Models/`, enums in `Enums/`, EF configurations in `Configurations/`
- Use Fluent API for EF Core; one configuration class per entity; do not configure EF inside DbContext
- Expose collections as `IReadOnlyCollection<T>`; use `List<T>` internally
- Frontend: follow the Redux Style Guide; use RTK Query for all API calls

---

## 5. Domain Model

Refer to `Documents/Design documents/domain_requirements.md` for full detail.
Key concepts:

- **Cabinet** contains 4 **Shelves**
- Each Shelf contains 9 **ContainerGroups** (3×3 grid)
- Each ContainerGroup holds containers of one type: `PX12`, `PX6`, `PX4`, or `PX2`
- **Container** is the aggregate root; it manages **Sections** and enforces lot placement rules
- **Section** is a subdivision of a Container; can hold multiple **Lots**
- **Lot** spans 1–3 Sections and must not cross containers
- Location is stored as coordinates (cabinet, shelf, group row/column, position row/column)

---

## 6. API Conventions

- All endpoints under `/api/storage/`
- Controllers in `Storage.Api/Controllers/`
- Services injected through constructors; registered in `Program.cs`
- Connection string name in `appsettings.json`: `StorageDatabase`

---

## 7. Frontend Conventions

- RTK Query API slices in `src/features/<feature>/<feature>Api.ts`
- Redux store in `src/app/store.ts`
- Dev server at `http://localhost:5173`; `/api` proxied to `http://localhost:5000`

---

## 8. Key Document References

| Topic | Document |
|---|---|
| Coding conventions | `Documents/Copilot/copilot_instructions.md` |
| Domain rules | `Documents/Design documents/domain_requirements.md` |
| Project structure | `Documents/Design documents/project_structure.md` |
| Storage initialization | `Documents/Design documents/storage_init_requirements.md` |
| Data layer | `Documents/Design documents/data_layer_requirements.md` |
| Order processing | `Documents/Design documents/order_processing_1_requirements.md` |
| Local environment | `Documents/Design documents/local_env_requirements.md` |
