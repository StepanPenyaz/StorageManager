# Storage Initialization — UI Improvements 3

## Purpose

This document updates the Storage Initialization wizard described in `storage_init_requirements.md`.

The current flow should be extended from **3 steps** to **4 steps**:

1. Initial settings
2. Cabinet and shelf configuration
3. Storage initialization preview and execution
4. Initial storage file (`.bsx`) processing

The goal of this change is to separate **storage structure creation** from **required lot import**, improve clarity for the user, and provide better progress and error reporting.

---

## Summary of Required Changes

- Keep storage structure configuration in the existing earlier steps.
- Convert the current Step 3 into a dedicated **storage initialization result/preview step**.
- Move `.bsx` file selection and processing to a new **Step 4**.
- Add clearer button behavior, validation rules, success states, and error presentation.
- Add a `Back` button for better wizard navigation between steps.

---

## Updated Wizard Flow

### Step 1 — Initial settings

No functional change in this document.

This step still collects:
- container number starting index

---

### Step 2 — Cabinet and shelf configuration

- Add a `-` button with a `Remove group row` hint. The button should remove a group row only when the group row count is greater than `1`.

This step still collects:
- cabinet configuration
- shelf configuration
- container group row types

---

### Step 3 — Storage initialization preview and execution

Step 3 should focus only on **creating the storage structure** and showing the result.

#### Step 3 UI requirements

- Display all validation errors at the top of the step.
- Display storage initialization progress and final result in this step.
- Display the generated shelf numbering preview in a readable table/grid format.
- Add a `Next` button.
- The `Next` button must remain disabled until storage initialization is completed successfully.

#### Step 3 behavior

- When the user starts initialization, the backend should create:
	- `ContainerGroups`
	- `Containers`
	- `Sections`
- The preview shown to the user must match the numbering algorithm defined in `storage_init_requirements.md`.
- If initialization fails, Step 3 must show:
	- an error summary,
	- enough detail for the user to understand what failed,
	- no navigation to Step 4.
- If initialization succeeds, Step 3 must show:
	- success message,
	- summary counts,
	- enabled `Next` button.

#### Step 3 preview format

For each shelf, the UI should render a numbering preview as a table-like layout.

The left-most column should display the container group row type.
The remaining cells should display generated container numbers.

Example:

Starting index: `1000`

Shelf 1 configuration:
- Group row columns count: `3`
- Group row 1: `PX12`
- Group row 2: `PX6`

Illustrative preview:

|------|------|------|------|-----|------|
| PX12 | 1000 | 1001 | 1002 | ... | 1011 |
|      | 1012 | 1013 | 1014 | ... | 1023 |
|      | 1024 | 1025 | 1026 | ... | 1035 |
| PX6  | 1036 | 1037 | 1038 | ... | 1041 |
|      | 1042 | 1043 | 1044 | ... | 1047 |
|      | 1048 | 1049 | 1050 | ... | 1053 |

Notes:
- The preview is illustrative.
- Actual numbering must always be generated strictly by the backend/domain algorithm.
- The UI must not invent an alternative numbering order.

---

### Step 4 — Initial storage file (`.bsx`) processing

Step 4 should handle the required import of lots into already-created containers.

#### Step 4 UI requirements

- Move the `.bsx` file selector from the old Step 3 to Step 4.
- Show selected file metadata:
	- file name,
	- full file path,
	- file size.
- Add an action to start processing the selected file.
- Display processing progress.
- Display all processing errors in the Step 4 wizard popup/panel.
- Display warnings separately from fatal errors when possible.
- Display a success message when processing completes successfully.
- Add a `Finish` button.
- The `Finish` button must remain disabled until file processing finishes successfully.

#### Step 4 behavior

- Step 4 must be accessible only after Step 3 finishes successfully.
- The user must provide a `.bsx` file in Step 4.
- The wizard must not be completed until Step 4 processing finishes successfully.
- The system must process the selected file using the rules from section `7. Initial storage file (.bsx) processing rules` in `storage_init_requirements.md`.

#### Backend requirements for Step 4

- Add a dedicated backend action/endpoint for `.bsx` processing after storage initialization is complete.
- The backend should receive a **file path**, not uploaded file content.
- The backend must read and process the file on the server side.
- The backend must return:
	- processing status,
	- processed item count,
	- warning count,
	- error count,
	- summary message.
- The backend must preserve batching, logging, resumability, and processed-node handling defined in `storage_init_requirements.md`.

#### Error handling for Step 4

- If file processing fails, the wizard must remain on Step 4.
- The user must see:
	- a clear error summary,
	- batch or processing stage where failure occurred,
	- warnings for skipped items,
	- reference to the processing log if available.
- The `Finish` button must stay disabled after a fatal processing error.

#### Success handling for Step 4

After successful processing, show:
- total processed item nodes,
- total created or updated lots,
- total warnings,
- total execution time,
- confirmation that the `.bsx` file was processed successfully.

---

## Navigation Rules

- `Back` should return to the previous step without losing already confirmed data unless the user explicitly resets it.
- `Next` on Step 3:
	- disabled before successful storage initialization,
	- enabled only after successful storage initialization.
- `Finish` on Step 4:
	- disabled before successful `.bsx` processing,
	- enabled only after successful `.bsx` processing.

---

## Validation Rules

### Step 3

- Initialization cannot proceed if previous-step validation has failed.
- All initialization errors must be visible at the top of the step.
- The user must not be allowed to continue to Step 4 when storage creation failed.

### Step 4

- A file path is required and must point to an existing `.bsx` file.
- Invalid file path or inaccessible file must produce a visible validation error.
- Processing must not start if Step 3 has not completed successfully.

---

## UX Requirements

- The wizard should clearly show that Step 3 creates the storage structure and Step 4 imports required initial lot data.
- Long-running operations should show progress indicators.
- Success, warning, and error messages should be visually distinct.
- Error lists should be readable and grouped when multiple issues occur.
- The user should always understand whether:
	- storage creation succeeded,
	- file import succeeded,
	- file import completed with warnings,
	- file import failed.

---

## Acceptance Criteria

1. The wizard contains 4 steps instead of 3.
2. Step 3 is responsible only for storage initialization preview and execution.
3. The `.bsx` file selector is no longer shown in Step 3.
4. Step 4 contains the `.bsx` file-processing UI.
5. The user cannot open Step 4 before Step 3 succeeds.
6. Step 3 shows initialization errors at the top of the step.
7. Step 3 shows a readable numbering preview for shelves.
8. The `Next` button on Step 3 is enabled only after successful storage initialization.
9. Step 4 uses the `.bsx` processing rules from `storage_init_requirements.md`.
10. Step 4 shows progress, warnings, errors, and final summary.
11. The backend processes the `.bsx` file by server-side file path, not by HTTP file upload.
12. The wizard cannot be completed until `.bsx` processing in Step 4 succeeds.

---

## Example file

Example storage file:

`\Documents\Examples\Storage\Store 3_22_2026 5_25 PM.bsx`

---

## Implementation Note

Use `copilot_instructions.md` together with this specification and `storage_init_requirements.md` during implementation.