# Unit tests

Add unit tests for the `backend`. Place them in a separate test project/solution named `Storage.Test`.

Suggested short structure:
- **Project:** `Storage.Test` (use `xUnit`)
- **Test frameworks / libs:** `xUnit`, `Moq` or `NSubstitute` for mocking
- **Project layout:**
	- `Controllers/` — controller tests
	- `Services/` — service tests
	- `Domain/` — domain model tests
- **Run tests:** `dotnet test ./Storage.Test`
- **CI:** run `dotnet test` in pipeline; report test results and coverage
- **Naming:** use descriptive test names (e.g., `When_X_Then_Y` or `Should_DoSomething_When_Condition`)