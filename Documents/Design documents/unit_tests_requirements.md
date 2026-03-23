# Unit Tests Requirements – Storage Management System

## 1. Overview

Unit tests must be written for all domain models with meaningful behavior and for stateless service classes. Tests should be isolated, fast, and not depend on a database or file system unless a temporary in-memory substitute is used.

---

## 2. Test Project

- Create a new xUnit test project named `Storage.Tests`
- Place the project under `Backend/Storage/Storage.Tests/`
- Add the project reference to `Storage.slnx`
- Reference `Storage.Domain` and `Storage.Services` projects
- Use `xunit` as the test framework and `Moq` for mocking

---

## 3. Domain Model Tests

### 3.1. `Location`

Test that `Location` throws `ArgumentOutOfRangeException` for negative values on each of the six coordinate properties: `Cabinet`, `Shelf`, `GroupRow`, `GroupColumn`, `PositionRow`, `PositionColumn`.

Test that `Location` is successfully created when all coordinates are zero or positive.

### 3.2. `Container`

Test that `InitializeSections()` creates **3 sections** (with indices 1, 2, 3) for a `PX12` container.

Test that `InitializeSections()` creates **1 section** (index 1) for each of `PX6`, `PX4`, `PX2` container types.

Test that calling `InitializeSections()` a second time is idempotent (sections are not duplicated).

### 3.3. `ContainerGroup`

Test that `AddContainer` throws `ArgumentNullException` when a null container is passed.

Test that `AddContainer` throws `InvalidOperationException` when the container type does not match the group type.

Test that `AddContainer` throws `InvalidOperationException` when the container location does not match the group's cabinet/shelf/groupRow/groupColumn.

Test that `AddContainer` successfully adds a container when the type and location match.

### 3.4. `LotSection`

Test that `AddQuantity` increases the quantity by the given amount.

Test that `AddQuantity` throws `InvalidOperationException` when the quantity to add is zero or negative.

Test that `SubtractQuantity` decreases the quantity by the given amount.

Test that `SubtractQuantity` throws `InvalidOperationException` when the quantity to subtract exceeds the current quantity.

---

## 4. Service Tests

### 4.1. `OrderFileParser`

Test that `Parse` correctly returns an empty collection when the XML has no `Item` elements.

Test that `Parse` correctly parses a valid XML file with multiple items and returns a collection with the expected `LotId`, `Qty`, and `Remarks` values.

Test that `Parse` skips items missing `LotID` or `Qty` elements.

### 4.2. `StorageUpdateFileParser`

Test that `Parse` correctly returns an empty collection when the XML has no `Item` elements.

Test that `Parse` correctly parses a valid XML file with multiple items and returns a collection with the expected `ItemId`, `Qty`, and `Remarks` values.

Test that `Parse` skips items missing `ItemID` or `Qty` elements.

---

## 5. Acceptance Criteria

- All tests must pass with `dotnet test`
- Tests must not depend on a live database or running service
- Each test method covers exactly one behaviour (single assertion where possible)
- Test class names follow the pattern `<ClassName>Tests` (e.g., `ContainerTests`, `OrderFileParserTests`)
- Test method names follow the pattern `<MethodOrProperty>_<Scenario>_<ExpectedOutcome>`
