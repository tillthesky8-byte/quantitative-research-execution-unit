# QREU Workspace Instructions

- Target .NET 10 / C# 12 with nullable reference types enabled and implicit usings on.
- Keep changes small and focused; preserve the existing project structure and naming unless a change explicitly requires refactoring.
- Prefer sealed concrete types, constructor injection, and `init`-only properties for simple models.
- Follow the existing namespace layout: `Application`, `Domain`, `Core`, `Modules`, and `Writer`.
- For CLI work, keep `System.CommandLine` usage centralized in `src/QREU.Application/Options/OptionFactory.cs` and command types under `src/QREU.Application/Commands/`.
- Prefer structured logging over ad hoc console output for application behavior; keep `Console.WriteLine` only for direct CLI feedback or temporary diagnostics.
- Reuse centralized log templates from `Domain.Other.LogMessages` when adding or changing log messages.
- Read configuration through `AppSettings` and keep defaults consistent with `appsettings.json` and `configs/`.
- Use the existing custom console formatter and ANSI color helpers when changing console logging output.
- Prefer `dotnet build` and `dotnet test` for validation; keep tests in `tests/QREU.Tests/`.
- When reviewing changes, use the repo helper script `scripts/review-changes.sh` or an equivalent diff/stat flow that includes untracked files.
- Write commit messages in conventional format, such as `feat(scope): summary` or `fix(scope): summary`.
