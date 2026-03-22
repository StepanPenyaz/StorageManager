# DB Initialization Script Requirements – Storage Management System

## 1. Overview

Prepare an SQL script for database initialization.

* Review the **Storage.Data** project
* Use the existing database structure
* Generate an SQL script that fills the database with initial data
* The script should be executable via SQL Server Management Studio

---

## 2. Storage Structure

The store consists of:

* 2 cabinets
* Each cabinet contains 4 shelves
* Each shelf contains 9 container groups (3×3 grid)

---

## 3. Container Group Types

### Cabinet 1

* Shelf 1 → all PX12
* Shelf 2 → all PX12
* Shelf 3 → all PX12
* Shelf 4 → all PX6

### Cabinet 2

* Shelf 1 → all PX12
* Shelf 2 → all PX12
* Shelf 3 → all PX12
* Shelf 4:

```
px6  px6  px6
px6  px6  px6
px4  px4  px4
```

---

## 4. Containers

* Container numbering starts from `1000`
* Container numbers must be unique

---

## 5. Data Population Rules

* Fill **80% of container sections** with random items
* 20% of sections should remain empty

### Lots

* `LotId` must be unique across the entire store
* Each section may contain multiple lots
* All lots in a section must have the **same ItemId**

### Example

Container `1100` (type PX6):

* LotId = `1122`, ItemId = `1`
* LotId = `2233`, ItemId = `1`

---

## 6. Additional Requirements

* Use realistic randomization for:

  * ItemId
  * Quantity
* Maintain referential integrity
* Ensure all foreign key relationships are valid

---

## 7. Output

The final result should be:

* A single SQL script file
* Compatible with SQL Server Management Studio
* Ready to execute without manual modification
* Result file should be added to `Documents/Scripts/` directory with name `initial_data_seed.sql`