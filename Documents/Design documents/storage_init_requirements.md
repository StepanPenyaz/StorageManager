# Storage initialization — Requirements (AI prompt)

Purpose
- Provide a precise, unambiguous specification for initializing storage layout records and importing initial inventory from a XML (.bsx) file.

Quick summary
- UI: "Storage Init" wizard (3 stages: initial settings → cabinet/shelf config → optional .bsx import).
- Output: DB records for ContainerGroups, Containers, Sections, Lots and LotSections; and an updated .bsx file with processed flags.
- Numbering: deterministic sequential numeric indices assigned to atomic "slots" according to shelf configuration and container-type slot multipliers.

1. Overview
- Initialize a storage layout (cabinets → shelves → container-group-rows → columns) and create DB records for ContainerGroups, Containers, and Sections.
- Optionally import inventory from a BrickStore XML (.bsx) file by matching <Remarks>#<container_number> entries to container slots and creating/updating Lot and LotSections records.
- Support large XML files via batched processing and incremental save points.

2. High-level constraints and validations
- Required inputs:
  - Container number starting index (positive integer, e.g., 1000)
- Container group row must specify exactly one container type for the entire row.
- All container groups in one shelf must have the same number of columns (columns is a shelf-level parameter).
- Each container group row is defined by:
  - number of rows (vertical repetition count)
  - container type (one of PX12, PX6, PX4, PX2)
- UI must validate:
  - Positive integer start index
  - At least one cabinet defined
  - Each cabinet contains at least one shelf and each shelf contains at least one container group row
  - Container types are valid (from type set)

3. Storage creator — UI behavior and flow
Add "Storage Init" to the header burger menu. Clicking opens "Storage Master" wizard with three stages:

Stage 1 — Initial settings
- Fields:
  - Container number starting index (integer)
- Buttons:
  - Move to next step (enabled when validated)
  - Cancel (close without changes)
- Behavior:
  - Save values in wizard state (no DB writes yet)

Stage 2 — Cabinet & shelf configuration
- UI elements:
  - Add cabinet (creates new cabinet in wizard state with CabinetIndex starting with 1)
  - Per cabinet:
    - Add shelf
  - Per shelf:
    - Columns (once selected this number will be applied to each cabinet shelf)
    - For each container-group-row:
      - Rows (integer — vertical repetition)
      - Container type (select from type set)
    - Preview: computed container counts, slots count, and a visual numbering preview (grid)
    - Save shelf configuration
  - Save cabinet to add another cabinet
- Buttons:
  - Move to next step (enabled if ≥ 1 cabinet configured)
  - Back, Cancel
- UI validations:
  - Columns > 0, rows > 0, container type valid.
  - Show computed totals and per-type counts for user confirmation.

Stage 3 — Initial storage file (optional)
- UI elements:
  - File picker (.bsx)
  - File metadata: filename, size, estimated matching Item nodes
  - Batch size selector (default 1000)
- Buttons:
  - Initialize store (start DB + file processing)
  - Back, Cancel
- Behavior:
  - File selection optional (if omitted, only DB container records are created).
  - On Initialize: run DB creation then file processing (if file provided).
  - Show progress, per-batch stats, and final summary.

Accessibility, progress and messages
- Show real-time progress and a clear success or failure summary.
- On success: show totals (containers created, lots created/updated, nodes processed).
- On failure: show error summary, processing point, and link to error log for diagnostics.

4. Configuration model and examples

Definitions:
- columns (C): number of columns on a shelf (after selection it will be a fixed parameter for whole cabinet).
- group row (G): a row on the shelf having one container type and a rows count R.
- rows (R): number of vertical repetitions for that group row.
- slotsPerCell(T): number of atomic numeric slots assigned to each cell for container type T (see Assumptions mapping).
- Slots produced by group G on a shelf: C × R × slotsPerCell(T)

Example A — 2 group rows, 2 columns per group, both PX12 (defaults)
- Shelf:
  - Columns: 2
  - Group 1: rows = 3, type = PX12 (slotsPerCell = 4)
  - Group 2: rows = 3, type = PX12 (slotsPerCell = 4)
- Total slots = (2 × 3 × 4) + (2 × 3 × 4) = 24 + 24 = 48
- With startIndex = 1000 the assigned indices will be 1000..1047.
- (A printable numbering preview grid is shown in the UI before initialization.)

Example B — 2 group rows, 3 columns per group, top PX6, bottom PX4
- Shelf:
  - Columns: 3
  - Group 1: rows = 3, type = PX6 (slotsPerCell = 2)
  - Group 2: rows = 2, type = PX4 (slotsPerCell = 2)
- Total slots:
  - Group 1 = 3 × 3 × 2 = 18 (1000..1017)
  - Group 2 = 3 × 2 × 2 = 12 (1018..1029)
- Final indices 1000..1029.

IMPORTANT: The UI must present the numbering preview (grid) and let the user confirm before DB creation.

5. Container (slot) numbering algorithm — deterministic

Inputs:
- startIndex (integer)
- cabinets[] (ordered)
- for each cabinet: shelves[] (ordered)
- for each shelf: columns C, groupRows[] (ordered)
- for each groupRow: rows R, containerType T
- slotsPerCell(T) (assumed or configured)

Algorithm (ordered deterministic iteration)
1. currentIndex ← startIndex
2. For each cabinet in creation order:
   For each shelf in creation order:
     For each groupRow (in order added):
       multiplier ← slotsPerCell(T)
       For rowIndex from 0 to R - 1:         // top → bottom within this group row
         For colIndex from 0 to C - 1:       // left → right across columns
           For slotOffset from 0 to multiplier - 1:
             assign ContainerIndex = currentIndex
             persist mapping:
               (Cabinet, Shelf, GroupRowIndex, rowIndex, colIndex, slotOffset) → ContainerIndex
             currentIndex ← currentIndex + 1

Outputs:
- A complete list of numeric indices (ContainerIndex) assigned to slots.
- Container records and Section records will reference these indices per the DB model.

Notes:
- The UI preview must render the grid from the same algorithm (groupRow order → row → column → slot).
- Container record vs slot:
  - Implementation must decide if a DB "Container" maps to a physical cell (one per column×row) and "Section" or "Slot" maps to numeric ContainerIndex; or if Container table stores each slot directly. The prompt requires the agent to follow the numbering algorithm for atomic indices regardless of DB table mapping — map indices to DB schema consistently.

6. Database initialization (DB writes and transactional behavior)

On "Initialize store":
- Create ContainerGroup records for each group row with: CabinetId, ShelfId, GroupRowIndex, Columns, ContainerType, Rows, SlotsPerCell, Count (computed).
- Create Container (or Slot) records for each assigned numeric ContainerIndex with fields:
  - ContainerIndex, ContainerType, ContainerGroupId, CabinetId, ShelfId, ColumnIndex, RowIndexWithinGroup, SlotOffset
- Create default Section records for each Container or per design decision:
  - Default: create N sections per Container (configurable; suggested default: 1 section per slot).
- DB writes must be batched (configurable batch size). Each batch commit is transactional: commit or rollback.
- After each batch commit, persist the updated .bsx file (if processing a file) and record progress.

7. Initial storage file (.bsx) processing rules

XML structure (relevant parts):
- BrickStoreXML → Inventory → Item
  - Item contains ItemID, Qty, LotID, Remarks (optional), etc.

Selection criteria:
- Consider only Item nodes where:
  - Remarks exists and matches regex ^#([0-9]+)$ (capture containerNumber)
  - AND there is no <Processed> child OR <Processed> value != "true" (case-insensitive)

Processing steps for each matching Item node:
1. Extract containerNumber, Qty (number), ItemID, LotID.
2. DB actions (in a per-batch transaction):
   - Upsert Lot record: ensure Lot.Id = LotID and Lot.ItemId = ItemID (if mismatch, log warning).
   - Find a Section within the container referenced by containerNumber to assign the Lot:
     - Prefer an empty Section (no Lot assigned).
     - If all sections occupied, select the section with highest Index (last section) in that container.
     - If containerNumber not found in DB, log a warning and skip this node.
   - Insert or update LotSections:
     - If a LotSections record for (LotId, SectionId) exists: merge quantities by adding Qty (and record audit).
     - Else: insert LotSections(LotId, SectionId, Quantity = Qty, metadata).
3. Mark XML node processed: add child <Processed>true</Processed> to the Item node.
4. Continue until batch size reached.
5. After batch:
   - Commit DB transaction.
   - Save the .bsx file (persist <Processed> flags).
   - Update progress in UI/log.
6. Continue next batch until all nodes processed.

Batching and resiliency:
- Default batch size: 1000 nodes (configurable).
- On DB error in a batch: rollback the batch, do not persist <Processed> for that batch, log details, present error to user.
- Because processed flags are written after each committed batch, the process is resumable.

Idempotency:
- Items with <Processed>true</Processed> are skipped.
- Upsert semantics ensure re-running the import does not duplicate Lot/Slot assignments.
- The agent should record an audit/log for each LotSections update for traceability.

8. Logging and error handling
- Log file next to the .bsx file: <filename>.processing.log with timestamps, processed counts, errors.
- Error handling policy:
  - Non-fatal: skip the node, log warning (e.g., container missing).
  - Fatal for batch: rollback, log error, notify user and stop processing.
- UI should surface:
  - Completed successfully (summary)
  - Completed with warnings (summary + link to log)
  - Failed with error (batch index, reason)

9. Success / Completion UX
- On success show:
  - Total containers/slots created
  - Total lots created/updated
  - Total Item nodes processed
  - Total time
- On completion with errors:
  - Show number of successful nodes, number of failed nodes, link to log and instructions to re-run processing.

10. Acceptance criteria / test cases
- UI validation: blocks invalid fields (empty storage name, non-positive startIndex, no cabinet).
- Numbering: given configuration and startIndex, preview exactly matches DB records after initialization.
- .bsx processing: matching Item nodes converted to Lot/LotSections and <Processed>true</Processed> is added after batch commit.
- Idempotency: re-running does not duplicate data and skips processed nodes.
- Batch resiliency: process resumes from first unprocessed node after interruption.

11. Implementation questions (must resolve before run)
- Confirm slotsPerCell mapping for all container types (default mapping provided above).
- Decide the DB schema mapping for Container vs Slot vs Section (one-to-one or two-layer).
- Default number of Sections per container (if not equal to slots-per-cell).
- Quantity type: integer-only or allow fractional quantities.
- Upsert policy: merge quantities or create separate LotSections history records?

12. Agent instructions (how an automated agent should proceed)
1. Validate inputs (storage name, startIndex, cabinet/shelf config, container types).
2. Present a numbering preview (grid) from the algorithm; require user confirmation before DB writes.
3. Create DB ContainerGroup, Container/Slot and Section records in batches (transactional per batch).
4. If file provided: parse .bsx, locate matching Item nodes, process batches as described and persist <Processed>true</Processed> after each successful batch commit.
5. Maintain and write a processing log file.
6. On error, rollback batch, record detailed log entries and present an actionable error.
7. Produce final summary report and UI popup.

Appendix — Small pseudocode sample (reference)