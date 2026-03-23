# Storage Initialization — Specification

Description: Simple storage manager with UI representation.

This document describes:
- the UI steps to initialize storage,
- how cabinets, shelves and container groups behave,
- the container numbering and position calculation algorithm,
- how to populate the `[StorageManager].[dbo].[Containers]` table.

---

## UI Flow (high level)

1. Open the burger menu in the UI and select `Init store`.
   The "Storage Initialization" dialog or page appears.

2. In the Storage Initialization view, the user configures cabinets and shelves as described below.

---

## Step 2 — Cabinet / Shelf Configuration

- Each cabinet area is labeled `Cabinet {Number}`.
- Below the label there is a configuration block with:
  - `Shelf count` — number of shelves in the cabinet (integer, minimum 1).
  - `Container groups column count` — number of container-group columns per shelf (integer, minimum 1).

Cabinet behavior:
- A cabinet initially starts with one shelf.
- Changing the `Shelf count` adds or removes that many shelf configuration panels for the cabinet.
  - If increasing, new shelf panels are appended (with defaults).
  - If decreasing, the last shelf panels are removed.

Shelf configuration panel details:
- Each shelf panel contains a vertical list of "group row" selectors. Each group row represents a container group type placed horizontally across the shelf.
- Initially each shelf has a single group-row type selector.
- At the right side of the group-row selector list there is a `+` (add) button.
  - Clicking `+` adds a new group-row selector appended at the bottom of the list.
  - The `+` control is always associated with the bottom-most type selector (i.e., pressing it adds after the last selector).
  - The `+` button adds a new group-row (a new container-group row) to that shelf.

Example initial state:
- Cabinet 1:
  - `Container groups column count`: 3
  - `Shelf count`: 1
- Shelf 1:
  - Group row 1: selector (shows `PX12`, `PX6`, `PX4`, `PX2` options) and `+` button

After clicking `+` once:
- Shelf 1:
  - Group row 1: selector
  - Group row 2: selector (and now the `+` is associated with this second selector)

After user selects types:
- Example final state for Shelf 1:
  - Group row 1: PX12
  - Group row 2: PX6

When the user continues (Next/Apply), the storage grid for the shelf is generated from these settings (see numbering/position algorithm below).

---

## Container group parameters

Each container group type has the following derived parameters:

- `container_qty` (total containers in a single group):
  - PX12 → 12
  - PX6  → 6
  - PX4  → 4
  - PX2  → 2

- `containers_columns_number` (columns inside one group):
  - PX12 → 4
  - PX6  → 2
  - PX4  → 2
  - PX2  → 2

- `containers_rows_number` = `container_qty` / `containers_columns_number`
  (e.g., PX12: 12 / 4 = 3 rows)

Cabinet / shelf parameters (per shelf):
- `cabinet_number` — integer
- `shelf_number` — integer (per cabinet)
- `shelf_container_group_columns_number` — number of group columns across the shelf (user-configurable)
- For each group-row i:
  - `shelf_container_group_row[i].Type` — type (PX12/PX6/PX4/PX2)

---

## Shelf grid calculation (A × B) for a shelf row

Given:
- `groupColumns` = `shelf_container_group_columns_number`
- For a specific group-row type:
  - `containers_columns_number` (as above)
  - `containers_rows_number` (as above)

Compute:
- A = number of columns in the container grid for that group row:
  - A = groupColumns × containers_columns_number
- B = number of rows in the container grid for that group row:
  - B = containers_rows_number

Thus the container grid for the row has A columns and B rows, and total containers for that group-row across the shelf = A × B.

Example:
- groupColumns = 3
- Group-row 1 type = PX12:
  - containers_columns_number = 4
  - containers_rows_number = 3
  - A = 3 × 4 = 12
  - B = 3
  - total containers = 12 × 3 = 36

- Group-row 2 type = PX6:
  - containers_columns_number = 2
  - containers_rows_number = 3
  - A = 3 × 2 = 6
  - B = 3
  - total containers = 6 × 3 = 18

Total containers on shelf = 36 + 18 = 54

---

## Container numbering ranges and starting number

- `container_starting_number` is a storage-wide setting (configured in settings).
- Numbering is sequential and continuous across group-rows in the order they are defined (top to bottom) on a shelf, across shelves and cabinets in whatever ordering you adopt (recommended: cabinet asc, shelf asc, group-row asc, group-column asc, row asc — see algorithm below).
- For each group-row, the container number range is:
  - [ current_start .. current_start + (A×B) - 1 ]
- After filling that group-row, the next group-row's start is increased by (A×B).

Example with `container_starting_number = 1000` and a single cabinet (1), single shelf (1) with two group-rows (PX12, PX6) and `groupColumns = 3`:
- PX12 group-row → 36 containers → numbers 1000 .. 1035
- PX6 group-row  → 18 containers → numbers 1036 .. 1053

Total containers: 54

---
## Mapping to [StorageManager].[dbo].[ContainerGroups]

Target table fields and how to populate them:

- [Id]-  DB identity.
- [Cabinet] — `cabinet_number`.
- [Shelf] — `shelf_number`.
- [GroupRow] — the group-row index (1-based) on the shelf.
- [GroupColumn] — group-column index (1-based). This is which of the `groupColumns` this container belongs to.

## Mapping to [StorageManager].[dbo].[Containers]

Target table fields and how to populate them per container:

- [Id]-  DB identity.
- [Number] — the sequential container number (see numbering algorithm).
- [Type] — group type (PX12/PX6/PX4/PX2).
- [Cabinet] — `cabinet_number`.
- [Shelf] — `shelf_number`.
- [GroupRow] — the group-row index (1-based) on the shelf.
- [GroupColumn] — group-column index (1-based). This is which of the `groupColumns` this container belongs to.
- [PositionRow] — position within the container group grid row (1..B). Use the row index increasing from front to back (or top to bottom) per your UI convention.
- [PositionColumn] — position within a container group column (1..containers_columns_number). This is the sub-column index inside the group.
- [ContainerGroupId] — [ContainerGroups].Id

---

## Algorithm (detailed pseudocode)

Assumptions/definitions:
- `startNumber` = container_starting_number (integer).
- Iterate order: cabinet asc → shelf asc → group-row asc → column across groups asc → row asc → column inside group asc.
- groupColumns = shelf_container_group_columns_number
- For a group-row type:
  - container_qty = lookup(type)  // PX12→12, PX6→6, PX4→4, PX2→2
  - containers_columns_number = lookupColumns(type)  // PX12→4, others→2
  - containers_rows_number = container_qty / containers_columns_number
  - A = groupColumns * containers_columns_number
  - B = containers_rows_number

Pseudocode:

```
startNumber = container_starting_number

for each cabinet in cabinets ordered by cabinet_number:
  for each shelf in cabinet.shelves ordered by shelf_number:
    for each groupRowIndex = 1..shelf.groupRows.count:
      type = shelf.groupRows[groupRowIndex].Type
      container_qty = type_to_qty(type)
      containers_columns_number = type_to_columns(type)
      containers_rows_number = container_qty / containers_columns_number

      groupColumns = shelf.shelf_container_group_columns_number
      A = groupColumns * containers_columns_number  // total columns across shelf for this row
      B = containers_rows_number                    // total rows

      // loop rows then columns to set PositionRow/PositionColumn in an intuitive order:
      for rowIndex = 1 to B:
        for colIndex = 1 to A:
          // determine which groupColumn (1..groupColumns) this column belongs to:
          groupColumnIndex = ceil(colIndex / containers_columns_number)

          // positionColumnInsideGroup is 1..containers_columns_number (1..4 or 1..2)
          positionColumnInsideGroup = ((colIndex - 1) % containers_columns_number) + 1

          container.Number = startNumber
          // Id strategy: either container.Id = startNumber OR let DB assign Id and use Number as unique field.
          container.Id = startNumber  // if Id is not identity
          container.Type = type
          container.Cabinet = cabinet.cabinet_number
          container.Shelf = shelf.shelf_number
          container.GroupRow = groupRowIndex
          container.GroupColumn = groupColumnIndex
          container.PositionRow = rowIndex
          container.PositionColumn = positionColumnInsideGroup
          container.ContainerGroupId = makeContainerGroupId(cabinet, shelf, groupRowIndex, groupColumnIndex)

          // INSERT container into database
          insertContainer(container)

          startNumber = startNumber + 1
```

Notes:
- `PositionRow` and `PositionColumn` are defined such that `PositionRow` indexes 1..B (rows), and `PositionColumn` indexes 1..containers_columns_number (columns inside a group).
- The overall shelf column (1..A) consists of `groupColumns` groups, each group occupying `containers_columns_number` consecutive columns.


## Example (full walkthrough)

Configuration:
- container_starting_number = 1000
- cabinet 1:
  - shelf 1:
    - shelf_container_group_columns_number = 3
    - groupRow[1] = PX12
    - groupRow[2] = PX6

Calculations:
- groupRow[1] PX12:
  - container_qty = 12
  - containers_columns_number = 4
  - containers_rows_number = 3
  - A = 3 * 4 = 12
  - B = 3
  - total = 36 → numbers 1000 .. 1035

- groupRow[2] PX6:
  - container_qty = 6
  - containers_columns_number = 2
  - containers_rows_number = 3
  - A = 3 * 2 = 6
  - B = 3
  - total = 18 → numbers 1036 .. 1053

Final totals:
- Number of containers = 54
- Numbers assigned: 1000 .. 1053

Sample first rows (visual grid):
- For PX12 row (A=12, B=3) the shelf layout has 12 columns × 3 rows. Numbers will fill row-by-row (row 1 col 1..12 → then row 2 col 1..12, etc.)
  - Row 1 (12 cells): 1000 1001 ... 1011
  - Row 2 (12 cells): 1012 ... 1023
  - Row 3 (12 cells): 1024 ... 1035

- For PX6 row (A=6, B=3) the shelf layout has 6 columns × 3 rows:
  - Row 1 (6 cells): 1036 ... 1041
  - Row 2: 1042 ... 1047
  - Row 3: 1048 ... 1053

---