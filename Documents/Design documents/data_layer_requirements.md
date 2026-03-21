# Data Layer Requirements – Storage Management System

## 1. Overview

A new **Data Access Layer (DAL)** project called **Storage.Data** to handle database persistence using **SQL Server Express** with **Windows Authentication** should be created.

---

## 2. Models

Use existing **Storage.Domain** project as a reference for required entities.
Use **domain_requirements.md** as a domain logic reference.

---

## 3. Project Structure

Use the following structure for the project:

```
Storage.Data/
├── Contexts/
│   └── StorageContext.cs
├── Entities/
│   └── (Your domain entities)
├── Repositories/
│   ├── IStorageRepository.cs
│   └── StorageRepository.cs
├── Migrations/
│   └── (EF Core migrations)
└── Configuration/
    └── (Entity configurations)
```

## 4. Database Configuration

- **Database Engine:** SQL Server Express
- **Authentication:** Windows Authentication
- **Connection String Format:** Use integrated security (`Trusted_Connection=true`)
- **Example:** `Server=(local);Database=StorageManager;Integrated Security=true;`

---

## 5. Implementation Scope (Phase 1)

Implement only the **basic read operation** needed to retrieve a list of containers with their empty sections:

- **Method Name:** `GetContainersWithEmptySectionsAsync()`
- **Returns:** A list of Containers with all their sections populated

### 5.1. StorageContext.cs

- Create DbContext class
- Define DbSets for all domain models
- Configure Windows Authentication connection string
- Use `appsettings.json` for connection string storage

### 5.2. Entity Configurations

- Create configuration files with Fluent API configuration
- Example: `SectionConfiguration.cs`, `ContainerConfiguration.cs`
- Map all necessary relationships
- Configure primary keys and foreign keys

### 5.3. Repository Interface & Implementation

- **IStorageRepository.cs:** Define `GetContainersWithEmptySectionsAsync()` method
- **StorageRepository.cs:** Implement the repository with:
  - Dependency injection of StorageContext
  - Query that fetches Containers with related Sections
  - Call `Container.InitializeSections()` to ensure sections are empty/initialized

### 5.4. Initial Migration

- Create an initial migration for the database schema
- Include all necessary tables with proper relationships

---