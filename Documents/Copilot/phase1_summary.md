# Phase 1 Summary – Storage Management System

**Project:** Storage Management System  
**Phase:** Phase 1  
**Date:** 2026-03-23

---

## Overview

The Storage Management System is a web-based application for managing physical storage in a warehouse or storage facility. It tracks containers organized into cabinet groups, with each container holding one or more sections. Sections are linked to inventory lots, and the system supports bulk initialization of the storage layout, importing existing inventory from BSX files, and processing inbound and outbound order files.

---

## Completed Features

- **Domain Models** – Core entities: `Container`, `ContainerGroup`, `Section`, `Lot`, `LotSection`, `Location` (value object), `ContainerType` (enum `PX12`, `PX6`, `PX4`, `PX2`).
- **Data Layer** – EF Core `StorageContext` backed by SQL Server, entity configurations using Fluent API, `IStorageRepository` / `StorageRepository`, and database migrations (initial schema + ItemId column type change).
- **Data Seed** – SQL script that seeds the initial cabinet and container structure for demo purposes.
- **Order Processing (Part 1)** – `OrderFileParser` (XML) and `OrderProcessingService` that subtracts lot quantities based on outbound order files and moves processed files to a finished directory.
- **Order Processing (Part 2)** – `StorageUpdateFileParser` (XML) and `StorageUpdateProcessingService` that increases lot quantities based on inbound storage-update files.
- **Storage Initialization** – `StorageInitializationService` supporting:
  - Wizard-driven layout initialization (cabinets, shelves, container groups, containers, sections).
  - BSX file metadata retrieval and batch processing (identity-insert lot IDs, section assignment, quantity merging, processed-item marking).
- **Web API** – ASP.NET Core controllers:
  - `StorageController`: `GET /api/storage/cabinets`, `GET /api/storage/cabinets/{id}/containers`.
  - `StorageInitializationController`: `POST /api/storage/initialize`, `POST /api/storage/bsx-metadata`, `POST /api/storage/process-bsx`, `POST /api/storage/process-orders`, `POST /api/storage/process-storage-updates`.
- **Frontend** – Vite + React + TypeScript SPA with Redux Toolkit and RTK Query:
  - Storage visualization showing cabinets and containers grouped by type.
  - Storage initialization wizard (multi-step form for cabinet/shelf/row configuration).
  - BSX file processing, order processing, and storage update UI panels.
- **Local Environment** – Docker Compose configuration for SQL Server, and a local run guide added to `README.md`.
- **Unit Tests** – xUnit test project (`Storage.Tests`) covering domain models (`Location`, `Container`, `ContainerGroup`, `LotSection`) and file parsers (`OrderFileParser`, `StorageUpdateFileParser`). All 39 tests pass.

---

## Architecture

| Layer | Project | Responsibility |
|-------|---------|---------------|
| Domain | `Storage.Domain` | Entities, value objects, enums, domain behavior |
| Data | `Storage.Data` | EF Core context, configurations, repository |
| Services | `Storage.Services` | Business logic (initialization, order/update processing, view) |
| API | `Storage.Api` | ASP.NET Core controllers, DTOs |
| Frontend | `Frontend/` | React + Redux SPA, RTK Query API client |
| Tests | `Storage.Tests` | xUnit unit tests for domain and services |

---

## Key Design Decisions

- **Clean Architecture** – The solution is split into Domain, Data, Services, and API layers with no upward dependencies from Domain.
- **Section counts are fixed per container type** – `PX12` containers have 3 sections (indices 1, 2, 3); `PX6`, `PX4`, and `PX2` containers have 1 section (index 1). This is enforced by `Container.InitializeSections()` and cannot be overridden.
- **BSX lot IDs use explicit identity insert** – Because the BSX file contains pre-assigned `LotID` values that must be preserved, `SET IDENTITY_INSERT` is used when creating lots during BSX processing.
- **File-based order processing** – Order and storage update files are XML documents dropped into an input directory. After processing, files are renamed and moved to a finished directory.
- **RTK Query for API communication** – The frontend uses Redux Toolkit Query for data fetching and caching, keeping API logic centralized and avoiding manual `useEffect`-based fetching.

---

## Known Limitations / Out of Scope for Phase 1

- No user authentication or authorization.
- No role-based access control.
- No reporting or analytics dashboards.
- No audit logging for inventory changes.
- No automated integration tests or end-to-end tests.
