# AGENTS.md

## Project Overview

- **Dual Codebase**: Contains the original Java source (`SweetHome3D-7.5-src`, GPL v2+) and an incremental C# port (`OpenHome3D`).
- **C# Port Goal**: Cross-platform desktop app (Avalonia UI + Godot 3D), preserving 100% `.sh3d` file compatibility. Currently in Phase 1 (Core I/O and Model).

## Exact Commands

### C# Port (Primary Focus)

- **Build**: `dotnet build`
- **Test**: `dotnet test`
- **Test with coverage**: `dotnet test --collect:"XPlat Code Coverage"`

### Java Original (Legacy/Reference Only)

- **Setup**: `.\setup.ps1` (Downloads Apache Ant and source ZIP)
- **Build**: `.\build.ps1` (Requires JDK 17+)
- **Run**: `.\run.ps1`
- **Sync Upstream**: `.\sync-upstream.ps1` (Requires Subversion)

## Architecture & Boundaries

- **Solution**: `OpenHome3D.slnx`
- **Core Library**: `src/OpenHome3D.Core/` (.NET 10, nullable enabled, implicit usings)
- **Tests**: `tests/OpenHome3D.Core.Tests/` (xUnit, Coverlet)
- **`.sh3d` Format**: A standard ZIP archive containing `Home.xml` (or `Home`) and embedded content directories (e.g., `content/`, `0/`, `1/`) for textures/models.
- **Data Models**: Use C# `record` types for immutability (e.g., `Home`, `Wall`, `Room`, `HomePieceOfFurniture`, `HomeProperty`).

## Testing Conventions

- **Roundtrip Validation**: Critical for `.sh3d` I/O. Tests must verify: Load `.sh3d` → Save `.sh3d` → Load new `.sh3d` → Assert deep equality of core properties (no data loss).
- **Test Resources**: `.sh3d` fixtures are in `tests/OpenHome3D.Core.Tests/Resources/`. The `.csproj` is configured to copy these to the output directory (`PreserveNewest`).
- **No Flaky Tests**: Avoid timing-dependent UI tests in the core library. Use deterministic xUnit facts.

## Workflow Gotchas

- **Do not modify** `SweetHome3D-7.5-src/`, this is only for reference. All new feature development belongs in the `src/` and `tests/` C# directories.
- **Licensing**: Any distributed derivative must remain GPL v2+ (or compatible). Do not introduce non-GPL-compatible dependencies in the C# port.
- **Temporary files**: Put all temporary files in the `local/` folder so they are accessible without additional permissions. The `local/` folder is untracked by git.
