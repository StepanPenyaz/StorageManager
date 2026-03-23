# Copilot Repository Context — Build Requirements

## Purpose

Define the content and structure of the GitHub Copilot custom instructions file for this repository.
The goal is to give Copilot accurate, up-to-date context so that generated code and suggestions align with the project's architecture, conventions, and domain rules.

---

## Output

Create `.github/copilot-instructions.md` at the repository root.

---

## Required Sections

### 1. Project Overview

- Brief description of the system (storage management for physical cabinets, shelves, and containers)
- List of main components and their roles

### 2. Technology Stack

Backend:
- .NET 10, ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- Clean Architecture / DDD

Frontend:
- Vite + React + TypeScript
- Redux Toolkit (RTK Query)

### 3. Solution Structure

Describe folder layout:
- `Backend/Storage/` — .NET solution with projects:
  - `Storage.Api` — Web API
  - `Storage.Data` — EF Core data layer
  - `Storage.Domain` — Domain models and logic
  - `Storage.Services` — Application services
- `Frontend/` — React SPA
- `Documents/` — Design documents and Copilot instructions

### 4. Coding Conventions

Reference `Documents/Copilot/copilot_instructions.md` for the full list.
Summarize the most impactful rules inline:

- .NET 10 and modern C# (file-scoped namespaces, primary constructors, expression-bodied members)
- No comments or XML documentation
- `var` when type is obvious; `required` + `init` for properties
- No regions; prefer ternary over simple `if`
- Interfaces in `Interfaces/`, models in `Models/`, enums in `Enums/`, EF configurations in `Configurations/`
- Fluent API for EF; separate configuration class per entity
- `IReadOnlyCollection<T>` for exposed collections
- Frontend: Redux Style Guide; RTK Query for all API calls

### 5. Domain Model Summary

Summarize key domain concepts from `Documents/Design documents/domain_requirements.md`:

- **Cabinet** → 4 Shelves → 9 ContainerGroups (3×3) → Containers
- Container types: `PX12`, `PX6`, `PX4`, `PX2`
- **Container** — aggregate root; manages Sections and Lot placement
- **Section** — subdivision of a Container; can hold multiple Lots
- **Lot** — inventory unit; spans 1–3 Sections within one Container
- Location-based model (cabinet, shelf, group position, position in group)

### 6. API Conventions

- Base path: `/api/storage/`
- Controllers in `Storage.Api/Controllers/`
- Services injected via constructor; registered in `Program.cs`
- Connection string name: `StorageDatabase`

### 7. Frontend Conventions

- API proxy: `/api` → `http://localhost:5000` (Vite config)
- Store slices in `src/features/`
- RTK Query API definitions in `src/features/<feature>/<feature>Api.ts`
- Dev server: `http://localhost:5173`

### 8. Key Document References

List paths to authoritative documents that Copilot should consult for detailed rules:

| Topic | Document |
|---|---|
| Coding conventions | `Documents/Copilot/copilot_instructions.md` |
| Domain rules | `Documents/Design documents/domain_requirements.md` |
| Project structure | `Documents/Design documents/project_structure.md` |
| Storage initialization | `Documents/Design documents/storage_init_requirements.md` |
| Data layer | `Documents/Design documents/data_layer_requirements.md` |
| Order processing | `Documents/Design documents/order_processing_1_requirements.md` |
| Local environment | `Documents/Design documents/local_env_requirements.md` |

---

## Constraints

- The file must be readable by GitHub Copilot (plain Markdown at `.github/copilot-instructions.md`)
- Keep the file concise — defer detail to the referenced documents
- Do not duplicate large blocks of text from other documents; use references instead
- Keep domain terminology consistent with `domain_requirements.md`