# Domain Requirements – Storage Management System

## 1. Overview

This document describes the domain model for a storage management system.
The system is designed to manage physical storage organized into cabinets, shelves, and containers, with support for flexible lot placement.

---

## 2. Core Concepts

### 2.1 Storage Structure

The storage system is organized directly by cabinets; there is no persisted `Storage` aggregate or name.

- Each **Cabinet** contains **4 Shelves**
- Each **Shelf** contains **9 Container Groups** (3 × 3 grid)
- Each **Container Group** contains containers of a single type

---

### 2.2 Container Types

Supported container types:

- `PX12`
- `PX6`
- `PX4`
- `PX2`

Each type defines:
- Number of containers in a group
- Layout inside the group
- Number of sections inside a container

| Type | Containers per Group | Layout | Sections per Container |
|------|----------------------|--------|------------------------|
| PX12 | 12                   | 4 × 3  | 3                      |
| PX6  | 6                    | 2 × 3  | 1                      |
| PX4  | 4                    | 2 × 2  | 1                      |
| PX2  | 2                    | 2 × 1  | 1                      |

---

### 2.3 Container

A **Container** is the basic storage unit.

Properties:
- Unique identifier
- Sequential number (starting from a configurable index, default `1000`)
- Type (`PX12`, `PX6`, etc.)
- Physical location (cabinet, shelf, position)
- Contains one or more **Sections**

---

### 2.4 Section

A **Section** is a subdivision of a container.

Properties:
- Belongs to exactly one container
- Has an index (e.g., 1–3 for PX12)
- Can store multiple lots
- Can be empty

---

### 2.5 Lot

A **Lot** represents stored inventory.

Properties:
- `LotId` (unique identifier)
- `ItemId`
- Quantity (distributed across sections)

---

### 2.6 Lot–Section Relationship

A **Lot** is assigned to sections via a many-to-many relationship.

Rules:
- A lot can occupy **1 to 3 sections**
- A lot **must not span multiple containers**
- A section can contain **multiple lots**
- Quantity is stored per section

---

## 3. Location Model

Instead of strict hierarchical nesting, the system uses a **location-based model**.

Each container is identified by coordinates:

- Cabinet
- Shelf
- Group position (row, column)
- Position inside group (row, column)

### Benefits:
- Simplifies database queries
- Improves performance (fewer joins)
- Easier UI representation (grid-based)
- Flexible structure (no rigid hierarchy)

---

## 4. Domain Rules

### 4.1 Container Group Rules
- Each group contains containers of **one type only**
- Number of containers must match container type definition

---

### 4.2 Container Rules
- Container number must be unique
- Section count depends on container type

---

### 4.3 Section Rules
- Section belongs to one container only
- Section can contain multiple lots

---

### 4.4 Lot Rules
- A lot:
  - Must belong to a single container
  - Can occupy 1 to 3 sections
  - Can share sections with other lots

---

## 5. Data Model (Conceptual)

### Entities

- **Container**
- **Section**
- **Lot**
- **ContainerGroup**

### Relationship Table

- **LotSection**
  - Links lots to sections
  - Stores quantity per section

---

## 6. Real-Life Scenarios

### Scenario 1: PX6 Container (Single Section)

- One container with 1 section
- Multiple lots (same ItemId) stored in the same section

Result:
- One section linked to multiple lots

---

### Scenario 2: PX12 Container (Split Lots)

- Section 1 → Lot A
- Sections 2 & 3 → Lot B

Result:
- Lot A linked to one section
- Lot B linked to two sections

---

### Scenario 3: PX12 Container (Single Lot Across All Sections)

- One lot spans all 3 sections

Result:
- Same LotId linked to all sections
- Quantity distributed per section

---

## 7. Aggregate Design

Recommended aggregate root:

### **Container**

Responsibilities:
- Manages sections
- Ensures lot placement rules:
  - Max 3 sections per lot
  - Single-container constraint

---

## 8. Key Design Decisions

- Use **location-based modeling** instead of deep hierarchy
- Use **many-to-many relationship (LotSection)** for flexibility
- Store **quantity at section level**
- Keep **Container as aggregate root**

---

## 9. Future Considerations

- Add indexing for location fields (performance)
- Implement validation rules in domain layer
- Support operations:
  - Add lot
  - Move lot
  - Split lot
- Add audit logging (optional)

---
