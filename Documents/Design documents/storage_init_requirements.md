# Storage initialization — Requirements (AI prompt)

Purpose
- Provide a precise, unambiguous specification for initializing storage layout records and importing initial inventory from a XML (.bsx) file.

Quick summary
- UI: "Storage Init" wizard (3 stages: initial settings → cabinet/shelf config → optional .bsx import).
- Output: DB records for ContainerGroups, Containers, Sections, Lots and LotSections; and an updated .bsx file with processed flags.
- Numbering: deterministic sequential Container.Number values assigned according to a fixed cabinet→shelf→3×3 grid→within-group-position iteration order.

1. Overview
- Initialize a storage layout (cabinets, each with 4 shelves, each shelf with a 3×3 grid of ContainerGroups) and create DB records for ContainerGroups, Containers, and Sections.
- Optionally import inventory from a BrickStore XML (.bsx) file by matching <Remarks>#<container_number> entries to containers and creating/updating Lot and LotSections records.
- Support large XML files via batched processing and incremental save points.

2. High-level constraints and validations
- Required inputs:
  - Container number starting index (positive integer, e.g., 1000)
- The storage structure is fixed by domain:
  - Each Cabinet contains exactly 4 Shelves (index 1–4).
  - Each Shelf contains exactly 9 Container Groups arranged in a fixed 3×3 grid (groupRow 1–3, groupColumn 1–3).
- Each Container Group (grid cell) has exactly one container type.
- For the configuration UI, all three cells in a group row share the same container type (configured per row; 3 type selections per shelf).
- UI must validate:
  - Positive integer start index
  - At least one cabinet defined
  - Container type is set for all 3 group rows of every shelf of every cabinet
  - Container types are valid (from type set: PX12, PX6, PX4, PX2)

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
    - 4 fixed shelves are automatically shown (no "Add shelf" action needed)
  - Per shelf:
    - Fixed 3×3 grid displayed (3 group rows × 3 group columns)
    - For each group row (3 per shelf):
      - Container type (select from type set; applies to all 3 cells in that row)
    - Preview: computed container counts, section counts, and a visual numbering preview (grid)
    - Save shelf configuration
  - Save cabinet to add another cabinet
- Buttons:
  - Move to next step (enabled if ≥ 1 cabinet configured with all shelves configured)
  - Back, Cancel
- UI validations:
  - Container type must be set for all 3 group rows of each shelf.
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
- groupRow (GR): one of the 3 fixed rows in the shelf 3×3 grid (index 1–3).
- groupColumn (GC): one of the 3 fixed columns in the shelf 3×3 grid (index 1–3).
- positionRow (PR) / positionColumn (PC): coordinates of a container within its ContainerGroup, determined by the type layout.
- containersPerGroup(T): total Container records inside a single ContainerGroup for type T.
- sectionsPerContainer(T): number of Section records created per Container for type T.

Container type layout table:

| Type | Layout (PR × PC) | Containers per Group | Sections per Container |
|------|------------------|----------------------|------------------------|
| PX12 | 4 × 3            | 12                   | 3                      |
| PX6  | 2 × 3            | 6                    | 1                      |
| PX4  | 2 × 2            | 4                    | 1                      |
| PX2  | 1 × 2            | 2                    | 1                      |

Example A — all 9 groups on one shelf are PX12, startIndex = 1000 (1 cabinet, 1 shelf)
- Shelf 3×3 grid: all 9 cells are PX12 (12 containers each)
- Total containers = 9 × 12 = 108 (numbers 1000..1107)
- Iteration order: GR=1, GC=1 → PR 1..4, PC 1..3 → GC=2 → GC=3 → GR=2 → ... → GR=3, GC=3
- (A printable numbering preview grid is shown in the UI before initialization.)

Example B — shelf with group rows 1 & 2 PX6, group row 3 PX4, startIndex = 1000 (1 cabinet, 1 shelf)
- Shelf:
  - Group rows 1 and 2: all 3 cells PX6 (6 containers per group)
  - Group row 3: all 3 cells PX4 (4 containers per group)
- Containers from rows 1 & 2: 6 groups × 6 = 36 (1000..1035)
- Containers from row 3: 3 groups × 4 = 12 (1036..1047)
- Total: 48 containers (1000..1047)

IMPORTANT: The UI must present the numbering preview (grid) and let the user confirm before DB creation.

5. Container numbering algorithm — deterministic

Inputs:
- startIndex (integer)
- cabinets[] (ordered, each with a cabinet index 1..N)
- for each cabinet: 4 fixed shelves (index 1..4)
- for each shelf: 9 fixed ContainerGroups in 3×3 grid (groupRow 1..3, groupColumn 1..3)
- for each ContainerGroup: containerType T (determines layout PR_max × PC_max)
- layout(T): positionRow range 1..PR_max, positionColumn range 1..PC_max (see type layout table)

Algorithm (ordered deterministic iteration)
1. currentNumber ← startIndex
2. For each cabinet (in order, index 1..N):
   For each shelf (index 1 to 4):
     For each groupRow (index 1 to 3):
       For each groupColumn (index 1 to 3):
         T ← containerType of this ContainerGroup
         For positionRow from 1 to layout(T).rows:         // top → bottom within group
           For positionColumn from 1 to layout(T).columns: // left → right within group
             assign Container.Number = currentNumber
             persist mapping:
               (Cabinet, Shelf, GroupRow, GroupColumn, PositionRow, PositionColumn) → Number
             currentNumber ← currentNumber + 1

Outputs:
- A complete ordered list of Container.Number values assigned to physical containers.
- Each Container record references its ContainerGroup and stores its full Location coordinates.

Notes:
- The UI preview must render the grid from the same algorithm (groupRow → groupColumn → positionRow → positionColumn).
- DB mapping: one Container record per physical position in a group (not per section). Sections are sub-records of a Container.

6. Database initialization (DB writes and transactional behavior)

On "Initialize store":
- Create ContainerGroup records for each cell in the 3×3 grid of each shelf with fields:
  - Id (auto-generated), Type, Cabinet, Shelf, GroupRow, GroupColumn
- Create Container records for each physical container in each ContainerGroup with fields:
  - Id (auto-generated), Number, Type, ContainerGroupId
  - Location (owned value object): Cabinet, Shelf, GroupRow, GroupColumn, PositionRow, PositionColumn
- Create Section records for each Container:
  - PX12: 3 sections per container (Index 1, 2, 3)
  - PX6, PX4, PX2: 1 section per container (Index 1)
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
- UI validation: blocks invalid fields (non-positive startIndex, no cabinet, missing container type for any group row).
- Numbering: given configuration and startIndex, preview exactly matches DB records after initialization.
- .bsx processing: matching Item nodes converted to Lot/LotSections and <Processed>true</Processed> is added after batch commit.
- Idempotency: re-running does not duplicate data and skips processed nodes.
- Batch resiliency: process resumes from first unprocessed node after interruption.

11. Implementation notes (resolved by domain)
- Container type layout and containers per group: PX12=12 (4×3), PX6=6 (2×3), PX4=4 (2×2), PX2=2 (1×2). Each position in the type layout corresponds to one Container record (no slotsPerCell multiplier).
- DB schema: Container = one physical container in a group cell; Section = subdivision of a Container. Container.Number is the sequential identifier assigned per the numbering algorithm.
- Sections per container: determined by type — PX12 creates 3 sections (index 1–3), all other types create 1 section (index 1). This is enforced in Container.InitializeSections() and is not configurable.
- Quantity type: integer-only. LotSection.Quantity is int.
- Upsert policy: merge quantities (add Qty to existing LotSection.Quantity). Record an audit log entry for each update.
- bsx LotID: Large external BrickLink identifier (e.g., 432075602). Used as the Lot.Id primary key. The domain's Lot.Id is currently auto-generated (IDENTITY), so explicit insertion must be enabled (e.g., SET IDENTITY_INSERT or reconfiguring ValueGeneratedOnAdd for the initialization flow). Ensure no collision with existing auto-generated Lot.Id values.

12. Agent instructions (how an automated agent should proceed)
1. Validate inputs (startIndex, cabinet count, container types for all group rows on all shelves).
2. Present a numbering preview (grid) from the algorithm; require user confirmation before DB writes.
3. Create DB ContainerGroup, Container, and Section records in batches (transactional per batch).
4. If file provided: parse .bsx, locate matching Item nodes, process batches as described and persist <Processed>true</Processed> after each successful batch commit.
5. Maintain and write a processing log file.
6. On error, rollback batch, record detailed log entries and present an actionable error.
7. Produce final summary report and UI popup.

Appendix — Small pseudocode sample (reference)