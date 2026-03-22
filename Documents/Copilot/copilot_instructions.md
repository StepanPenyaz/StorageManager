# General Guidelines

- Use .NET 10 and modern C# features
- Use file-scoped namespaces
- Use primary constructors where applicable
- Do not use comments or XML documentation (including <inheritdoc />)
- For frontend code use React and Redux
- All frontend code should be stored in `Frontend` folder

---

# Code Style

- Use one empty line between:
  - properties and methods
  - methods and constructors
- Use expression-bodied members where it improves readability
- Use `var` when the type is obvious
- Use `required` keyword for required properties
- Prefer `init` over `set` for immutability
- Avoid regions
- Use simple collection initializtion
- Prefer using the conditional (ternary) operator `?:` instead of `if` statements for simple conditions.
Examples:
```
var value = condition
    ? resultA
    : resultB;
```

```
public int Cabinet { get; } = cabinet >= 0
    ? cabinet
    : throw new ArgumentOutOfRangeException(nameof(cabinet));
```
- Use the official Redux Style Guide (https://redux.js.org/style-guide/) for frontend code

---

# Project Structure

- Place interfaces in `Interfaces` folder
- Place models (entities) in `Models` folder
- Place enums in `Enums` folder
- Place EF configurations in `Configurations` folder

---

# Naming Conventions

- Use PascalCase for types and public members
- Use camelCase for local variables and parameters
- Prefix interfaces with `I`
- Use singular names for entities (e.g., `Container`, not `Containers`)

---

# Entity Framework قواعد

- Use Fluent API instead of data annotations
- Create separate configuration classes per entity
- Do not configure EF inside DbContext directly
- Use strongly typed IDs where possible

---

# Domain Modeling

- Follow clean architecture principles
- Keep entities persistence-ignorant
- Use aggregate roots (e.g., Container as root)
- Avoid anemic models – include behavior inside entities
- Use value objects for concepts like Location

---

# Collections

- Use `IReadOnlyCollection<T>` for exposed collections
- Use `List<T>` internally
- Do not expose setters for collections

---

# Error Handling

- Use exceptions for invalid domain operations
- Do not return null – use nullable types explicitly or throw

---

# LINQ & Queries

- Prefer method syntax over query syntax
- Avoid unnecessary ToList() calls
- Keep queries readable and simple

---

# Constructors

- Prefer primary constructors
- Validate input in constructors
- Do not allow invalid state

---

# Additional Rules

- Avoid static classes unless necessary
- Avoid magic numbers – use constants or enums
- Keep classes small and focused