# Order Processing Service Requirements (Part 1) – Storage Management System

## 1. Overview

A new **Order Processing Service** should be added to the **Storage.Services** project to process incoming order files in XML format.

The service will:

* Parse order files
* Update storage quantities
* Move processed files to a finished directory

---

## 2. Order File Format

Orders are provided as XML files with the following structure:

```
BrickStoreXML
├── Inventory (Currency, BrickLinkChangelogId)
│   └── Item
│       ├── ItemID
│       ├── Qty
│       ├── Remarks
│       └── LotID
└── GuiState (Application, Version)
    ├── ColumnLayout (Compressed, CDATA)
    └── SortFilterState (Compressed, CDATA)
```

Example files can be found in `Documents/Design Documents/Examples/Orders`.

### Notes:

* Only `Inventory -> Item` nodes are relevant for processing

---

## 3. File Storage Configuration

* Input directory path should be configurable via `appsettings.json`
* Example input path:

```
C:/Storage/Orders/
```

* Processed files should be moved to a configured output directory:

```
C:/Storage/Orders/Finished
```

* File naming convention:

  * Append `_done` postfix before the file extension

### Example:

* Input: `Order 31100917 (Hehacz0.1).bsx`

* Output: `Order 31100917 (Hehacz0.1)_done.bsx`

* The original file must be deleted after successful processing

---

## 4. Implementation Scope (Phase 1)

Implement a service or a group of services responsible for:

* Parsing order files
* Processing items
* Updating storage
* Moving processed files

---

## 4.1. Processing Logic

* Retrieve the list of files from the input directory
* Read each file
* Process only items where the `Remarks` field is present, not empty, and matches the format `#CONTAINER_NUMBER`

Example: `Remarks` value `#1004` → container number = 1004

* Locate the container using the parsed identifier
* Inside the container, find a section containing a lot with `LotID`
* Subtract `Qty` from the matching lot
* Use the repository from **Storage.Data**
* Persist the updated container state
* After all items are processed, save the file with the `_done` postfix to the configured output directory
* Delete the original file
* Continue with the next file in the input directory

---

## 4.2. Processing Examples

### Example 1

**Input:**

* Folder contains 1 file: `order_1.bsx`

**Database state:**

* Container type: px12, container number: 1000

  * Section 1: empty
  * Section 2:

    * lotId1 → 10 items
    * lotId2 → 3 items
  * Section 3:

    * lotId3 → 1 item

**Order contains 2 items:**

1. Item:

   * Remarks = `#1000`
   * LotId = lotId1
   * Quantity = 10

2. Item:

   * Remarks = `#1000`
   * LotId = lotId3
   * Quantity = 1

**Result:**

* File saved as `order_1_done.bsx` in the configured folder

**Container state after processing:**

* Section 1: empty
* Section 2:

  * lotId2 → 3 items
* Section 3: empty

---

### Example 2

**Input:**

* Folder contains 2 files:

  * `order_1.bsx`
  * `order_2.bsx`

**Database state:**

* Container type: px12, container number: 1001

  * Section 1: empty
  * Section 2:

    * lotId1 → 10 items
    * lotId2 → 3 items
  * Section 3:

    * lotId3 → 1 item

**First order (`order_1.bsx`):**

1. Item:

   * Remarks = `#1001`
   * LotId = lotId1
   * Quantity = 10

2. Item:

   * Remarks = `#1001`
   * LotId = lotId2
   * Quantity = 1

**Second order (`order_2.bsx`):**

1. Item:

   * Remarks = `#1001`
   * LotId = lotId2
   * Quantity = 2

2. Item:

   * Remarks = `#1001`
   * LotId = lotId3
   * Quantity = 1

**Result:**

* Files saved as:

  * `order_1_done.bsx`
  * `order_2_done.bsx`

**Final container state:**

* All sections are empty
