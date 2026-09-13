# TreatmentPlanCombiner

A .NET console application that combines a set of related CSV files into a single
hierarchical treatment-plan document. It validates referential integrity across six
source files, groups the data by treatment plan, and emits a combined CSV plus a
debug JSON snapshot of the resulting hierarchy.

This repository is a **portfolio / demonstration project**. It is intentionally
self-contained: no databases, no web services, no external APIs. Everything runs
from `.NET` and a handful of NuGet packages.

> [!IMPORTANT]
> All input data in `Input/sample/` is **fictional and synthetic**. The sample was
> generated only to demonstrate the pipeline. This project is **not** a production
> system and must **not** be connected to any real EHR, patient, or treatment-plan
> data.

## Features

- Loads six related CSV files (plans, problems, goals, objectives, interventions,
  activities) with per-file, per-row error reporting.
- Validates referential integrity (orphans, duplicates, missing identifiers) and
  data quality (blank narratives, missing child records).
- Distinguishes **blocking** errors from warnings; a run with blocking errors stops
  before any output is written.
- Builds a plan → problem → goal → objective → intervention hierarchy in memory.
- Writes a combined CSV (one row per plan, plan JSON attached) and a debug JSON per
  plan.
- Writes a machine-readable validation report so problems are easy to review.
- Fully configurable via `appsettings.json`.
- Unit-tested with xUnit (46 tests).

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer.
  The project targets `net8.0` with `<RollForward>LatestMajor</RollForward>`, so it
  also runs on .NET 9/10 runtimes.

## Quick start

```powershell
dotnet run --project TreatmentPlanCombiner.csproj
```

With the default settings the app reads `Input/sample/` and writes:

```
Output/treatment_plans_<timestamp>.csv        combined output, one row per plan
Output/DebugJson/<plan id>.json               full hierarchy per plan
Logs/validation_report_<timestamp>.csv        every validation issue found
```

Expected exit codes:

| Code | Meaning                                                    |
|------|------------------------------------------------------------|
| `0`  | Success — output written                                   |
| `1`  | Validation failed — blocking errors found, no output written |
| `2`  | Fatal error — logged to `Logs/application_error_*.log`     |
| `3`  | One or more input files are missing                        |
| `4`  | Configuration error in `appsettings.json`                  |

## Configuration

All settings live in `appsettings.json`:

- `Folders.Input` — directory containing the six input CSV files.
- `Folders.Output`, `Folders.Logs`, `Folders.DebugJson` — output locations.
- `Files.*` — the six input file names and the output/report/log file patterns.
  Output patterns must contain the `{timestamp}` placeholder, which is replaced with
  a run timestamp.

## How it works

The app runs a fixed pipeline (details in [docs/architecture.md](docs/architecture.md)):

1. Load and validate settings.
2. Verify the six input files exist.
3. Load each CSV with row-level error capture.
4. Validate the combined data model.
5. If validation has blocking errors → stop (exit code 1).
6. Build the hierarchy and write outputs.

## Project structure

```
TreatmentPlanCombiner/          console application (Program.cs is the entry point)
  Models/                       records + enums used across the pipeline
  Services/                     pipeline steps (loader, validator, builders, writers)
Tests/TreatmentPlanCombiner.Tests/   xUnit test project
Input/sample/                   fictional sample data committed for demonstration
docs/                           architecture, data contract, validation rules, etc.
```

## Documentation

- [Architecture](docs/architecture.md) — pipeline stages and responsibilities.
- [Data contract](docs/data-contract.md) — input files, columns, and relationships.
- [Validation rules](docs/validation-rules.md) — every rule, its severity, and effect.
- [Design decisions](docs/design-decisions.md) — notable choices and why.
- [Portfolio case study](docs/portfolio-case-study.md) — context and lessons learned.

## License

MIT — see [LICENSE](LICENSE).