<!--
Sync Impact Report
==================
Version change: [TEMPLATE] → 1.0.0
Modified principles: All (initial population from blank template — first authored constitution)
Added sections:
  - Core Principles (7 principles)
  - Technology Stack Constraints
  - Quality Standards & Completion Criteria
  - Governance
Removed sections: [SECTION_2_NAME], [SECTION_3_NAME] (template slots replaced with named sections)
Templates requiring updates:
  - .specify/templates/plan-template.md ✅ updated — Constitution Check populated with project gates
  - .specify/templates/tasks-template.md ✅ updated — tests changed from OPTIONAL to REQUIRED
  - .specify/templates/spec-template.md ✅ no changes required
Follow-up TODOs: None — all placeholders resolved.
-->

# ToDo Constitution

## Core Principles

### I. Clean Architecture & Inward-Only Dependencies

Dependencies MUST point inward only: `API → Application → Domain`, and `Persistence → Domain`.
No outer layer may be referenced by any inner layer.

- `ToDo.Domain` MUST NOT reference any other project in this solution.
- `ToDo.Application` MUST NOT reference `ToDo.API`, `ToDo.Persistence`, or `ToDo.Infrastructure`.
- `ToDo.Persistence` MUST NOT reference `ToDo.API` or `ToDo.Application`.
- `ToDo.API` MAY reference `ToDo.Application` for use-case delegation but MUST NOT contain business logic.
- `ToDo.Infrastructure` MAY reference `ToDo.Domain` and `ToDo.Application` for contracts only.

**Rationale**: Inward-only dependencies guarantee that inner layers are independently testable and free of
framework coupling, making the architecture reproducible and resilient to technology change.

### II. Domain Isolation (Framework-Independent)

`ToDo.Domain` MUST remain free of all framework and infrastructure dependencies.

- Domain models MUST NOT inherit from EF Core base types or carry `[Required]`/`[Key]` data annotations.
- `ToDo.Domain` MUST NOT reference FluentValidation, ASP.NET Core, Entity Framework Core, or any NuGet
  package outside the .NET BCL.
- Domain logic (invariants, value objects) MUST be expressed as pure C# classes and records.

**Rationale**: A framework-free Domain is the foundational guarantee of Clean Architecture. Framework
leakage into the Domain makes it untestable in isolation and undermines layer separation.

### III. Application Layer Coordination (CQRS-Inspired Handlers)

The Application layer coordinates all use cases through dedicated request handler or service classes.

- Each handler or service MUST have a single responsibility: one use case per class.
- Handlers MUST be independently testable without infrastructure; all I/O MUST be abstracted via interfaces
  defined in the Application layer.
- All cross-cutting concerns (validation registration, exception types) MUST live in `ToDo.Application`.
- `ToDo.Application` MUST NOT reference EF Core, SQL, or any persistence-specific type.
- Business rules and orchestration logic MUST NOT reside in controllers or repositories.

**Rationale**: Single-responsibility handlers map one-to-one with user stories, enabling independent
testing, clear ownership, and safe parallel feature development.

### IV. Persistence Isolation & Repository Abstraction

All database access MUST go through `ICrudRepository<T>` (or a derived interface). Direct use of
`DbContext` outside `ToDo.Persistence` is prohibited.

- `DbContext` and EF Core entity configurations MUST remain confined to `ToDo.Persistence`.
- Every repository implementation MUST implement the `ICrudRepository<T>` contract defined in Application
  or Domain.
- Persistence entity classes MUST NOT cross layer boundaries; they MUST be mapped to/from Domain models
  at the Persistence boundary.
- EF Core migrations and seed data MUST be managed exclusively within `ToDo.Persistence`.

**Rationale**: Isolating `DbContext` in Persistence protects the rest of the system from ORM coupling,
enables swapping storage implementations, and keeps integration-test scope bounded.

### V. API Contract Preservation & Explicit Mapping

The existing HTTP API contract (routes, HTTP verbs, request/response shapes) MUST NOT be changed without
an explicit, approved contract amendment.

- All API input/output MUST use dedicated DTO classes; Domain models MUST NOT be serialized directly.
- Mapping between DTOs ↔ Domain models ↔ Entities MUST be performed by dedicated mapper classes; inline
  or ad hoc mapping within controllers, handlers, or repositories is prohibited.
- Controllers MUST contain no business logic; they delegate to Application handlers and translate results
  to HTTP responses.
- Request validation MUST use FluentValidation registered in the Application layer; data-annotation
  validation on controllers is prohibited.
- Swagger/OpenAPI documentation MUST remain accurate after every change.

**Rationale**: A stable API contract and explicit mapper classes prevent accidental breaking changes,
enforce separation of concerns, and make contract drift detectable at code-review time.

### VI. Test Coverage & Quality Gates (NON-NEGOTIABLE)

All features MUST be covered by xUnit-based tests at three levels: unit, integration, and E2E.

- **Unit tests** (`ToDo.UnitTest`): Test Application handlers and Domain logic in isolation; no I/O,
  no EF Core.
- **Integration tests** (`ToDo.IntegrationsTest`): Test Persistence repositories against a real SQL Server
  instance via Testcontainers; repositories MUST NOT be mocked in integration tests.
- **E2E tests** (`ToDo.E2ETest`): Test the full HTTP stack end-to-end; exercise the predefined API
  contract.
- All tests MUST pass on every commit before merging to `main`.
- SonarQube analysis MUST be executed and MUST report zero blocker and zero critical issues before a
  feature is considered complete.

**Rationale**: Multi-level testing guarantees that architecture, persistence, and API contract work
correctly together. SonarQube enforces a minimum code-quality floor across the solution.

### VII. AI Contribution Discipline (Minimal & Pattern-Consistent Changes)

AI-generated code MUST follow established project patterns and MUST NOT introduce unsolicited changes.

- AI MUST reuse existing abstractions, base classes, naming conventions, and file-layout patterns before
  creating new ones.
- AI MUST NOT introduce new NuGet packages or frameworks without explicit user approval.
- AI MUST NOT refactor code outside the direct scope of the current task.
- Each AI change MUST be isolated to the minimum set of files necessary, preserving reproducibility and
  ease of review.
- AI MUST NOT alter the API contract, project references, or layer boundaries unless explicitly instructed.
- Standard .NET naming conventions MUST be observed: PascalCase for types and members, camelCase for
  local variables and parameters, `I`-prefix for interfaces.

**Rationale**: Disciplined, minimal changes keep the codebase predictable, reduce merge conflicts, and
ensure the human author retains full understanding and ownership of every change.

## Technology Stack Constraints

The following technology choices are fixed. They MUST NOT be replaced or supplemented without explicit
user approval.

| Concern           | Technology                                                |
|-------------------|-----------------------------------------------------------|
| Framework         | .NET 10 / ASP.NET Core                                    |
| Orchestration     | .NET Aspire 13.1.2                                        |
| Database          | SQL Server (Docker / Testcontainers for tests)            |
| ORM               | Entity Framework Core 10                                  |
| Validation        | FluentValidation 12                                       |
| Observability     | OpenTelemetry (traces, metrics, logs via ServiceDefaults) |
| API Documentation | Swagger / OpenAPI (Swashbuckle 10)                        |
| Testing           | xUnit v3, Testcontainers (MsSql), FluentAssertions        |
| Quality Analysis  | SonarQube (zero blocker/critical issues required)         |

## Quality Standards & Completion Criteria

A feature or task is **complete** when ALL of the following conditions are met:

1. **Architecture preserved**: Inward-only dependencies verified; no layer boundary violations introduced.
2. **API contract intact**: No HTTP routes, verbs, or DTO shapes altered beyond the approved scope.
3. **Domain isolation verified**: `ToDo.Domain` has acquired no new framework dependencies.
4. **Repository contract respected**: All data access routes through `ICrudRepository<T>`; no direct
   `DbContext` usage outside `ToDo.Persistence`.
5. **Mapper classes present**: Every DTO ↔ Domain ↔ Entity translation uses a dedicated mapper class.
6. **All tests pass**: Unit, integration, and E2E suites report zero failures.
7. **SonarQube clean**: Analysis reports zero blocker and zero critical issues.
8. **No unrelated changes**: Diff contains only files required by the current task.

## Governance

This constitution supersedes all other development guidelines for this repository.
Amendments require:

1. A written justification describing why the change is necessary.
2. A `CONSTITUTION_VERSION` increment: MAJOR for principle removals/redefinitions, MINOR for new
   principles or materially expanded guidance, PATCH for clarifications or wording.
3. An update to `LAST_AMENDED_DATE`.
4. Propagation of any impacted constraints to `.specify/templates/` files and `CLAUDE.md`.

All pull requests MUST be reviewed against this constitution before merging. Any violation blocks merge.

**Version**: 1.0.0 | **Ratified**: 2026-05-14 | **Last Amended**: 2026-05-14
