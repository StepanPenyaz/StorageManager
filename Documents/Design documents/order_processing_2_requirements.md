# Order Processing Service Requirements (Part 2) — Storage Management System

## 1. Purpose

Part 2 extends the order-processing feature with:
- storage update file processing (incoming inventory),
- configurable folders from the UI,
- manual execution actions from the UI,
- domain update for `ItemId` type.

This document complements `order_processing_1_requirements.md` and does not replace its existing logic.

---

## 2. References

- `order_processing_1_requirements.md` — base order-processing behavior.
- `Documents\Examples\Storage Updates\1.bsx` — example storage update file.

---

## 3. Functional Scope

The application must support two independent operations:

1. **Process orders (outgoing)**
	- Use existing Part 1 behavior.
	- Reduce stored quantities according to incoming order files.

2. **Process storage updates (incoming)**
	- Add quantities to existing storage records based on storage update files.

---

## 4. UI Requirements

### 4.1 Header menu configuration section

Add a new configuration item in the burger menu with four folder path fields:

1. `Incoming orders folder`
2. `Processed orders folder`
3. `Incoming storage updates folder`
4. `Processed storage updates folder`

Rules:
- All four paths are required.
- Paths must be validated before processing starts.
- `Processed` folders are used for files renamed with `_done` suffix.

### 4.2 Header action buttons

Add two buttons in the right side of the header:

1. `Process orders`
2. `Process storage updates`

Behavior:
- Each button starts only its own flow.
- Show success/error feedback after processing completes.
- Do not block one action permanently after the other runs.

### 4.3 Additional UI changes

- Add a burger-menu item for day/night theme switching.
- After storage initialization wizard closes, refresh and render the storage view automatically.

---

## 5. Process Orders (Outgoing)

`Process orders` must reuse the existing logic from Part 1:
- parse incoming order files,
- decrease lot quantities,
- move processed files to the configured processed-orders folder,
- add `_done` before file extension.

---

## 6. Process Storage Updates (Incoming)

### 6.1 Input interpretation

For each update item:
- `Remarks` contains container number in format `#<CONTAINER_NUMBER>`.
- `ItemID` contains the item identifier.
- `Qty` contains quantity to add.

### 6.2 Processing logic

For each valid item in the update file:
1. Parse container number from `Remarks`.
2. Find the target container by container number.
3. Find a section in that container that already contains the same `ItemId`.
4. Increase the matching lot-section quantity by `Qty`.

If no matching container or section exists:
- skip the item,
- record a warning,
- continue processing the file.

### 6.3 File finalization

After successful file processing:
- move file to configured `Processed storage updates folder`,
- rename with `_done` suffix,
- keep processing next file.

---

## 7. Domain and Data Model Changes

Update domain and database so `ItemId` is a **string**, not numeric.

Examples of valid values:
- `61403pb06`
- `2493b`
- `3024`

Impact:
- update model/property type,
- update EF mapping and migration,
- ensure parsing and comparisons use string semantics.

---

## 8. Validation and Error Handling

- Skip invalid records and continue processing remaining items.
- Log warnings for skipped records (invalid `Remarks`, missing container, missing matching section, invalid quantity).
- Return a clear summary in UI:
  - processed files count,
  - processed items count,
  - warning count,
  - error count.
- If a file cannot be read, mark it as failed and continue with other files.

---

## 9. Acceptance Criteria

1. User can configure all four folder paths in UI.
2. `Process orders` runs existing outgoing logic from Part 1.
3. `Process storage updates` increases quantities based on update files.
4. Storage update parsing uses `Remarks=#<containerNumber>`, `ItemID`, and `Qty`.
5. Processed storage update files are moved with `_done` suffix to configured folder.
6. `ItemId` is stored and handled as string end-to-end.
7. Theme toggle option exists in burger menu.
8. Storage view refreshes automatically after initialization wizard closes.
9. UI shows processing summary with success/warning/error information.
