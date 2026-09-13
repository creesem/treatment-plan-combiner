# Architecture

`TreatmentPlanCombiner` is a console application with a deliberately simple
structure: a thin entry point, a coordinator, and a sequence of independent,
testable services.

## Entry point (`Program.cs`)

Top-level statements. Responsibilities:

- Bind `appsettings.json` into `AppSettings` via the Microsoft configuration binder.
- On binding failure: log to `Logs/application_error_*.log` and exit with
  `ConfigurationError` (`4`).
- Otherwise delegate to `ApplicationRunner.Run()` and return its exit code.

`Program.cs` holds **no business logic** — it only composes the app.

## The pipeline (`Services/ApplicationRunner.cs`)

`ApplicationRunner` orchestrates the pipeline in order:

1. **SettingsValidator** — checks that folders and file names are present and that
   output patterns contain the `{timestamp}` placeholder. On failure → exit code `4`.
2. **FolderInitializer** — create `Output`, `Logs`, `DebugJson` if missing.
3. **InputFileValidator** — verify all six input files exist. On failure → exit
   code `3`.
4. **CsvLoader** — load each CSV into typed records, capturing per-row load issues.
   Malformed rows or an empty file become blocking issues.
5. **ValidationService** — run the full validation rule set over the six datasets
   (see [validation-rules.md](validation-rules.md)).
6. **TreatmentPlanOutputBuilder + CsvOutputWriter** — flatten each plan hierarchy
   into a combined CSV (one row per plan).
7. **HierarchyBuilder + DebugJsonWriter** — write one JSON file per plan containing
   the full plan → problems → goals → objectives → interventions structure.
8. **ValidationReportWriter** — write the machine-readable issue report.

A run summary is printed to the console. Any unexpected exception escapes to
`Program.cs`, which writes an error log and returns `FatalError` (`2`).

## Exit codes

| Code | Constant                          | When                                              |
|------|-----------------------------------|---------------------------------------------------|
| `0`  | `Success`                         | Output written                                   |
| `1`  | `ValidationErrors`                | Blocking validation issues found; no output      |
| `2`  | `FatalError`                      | Unexpected exception (logged)                    |
| `3`  | `MissingInputFiles`               | One or more input files absent                   |
| `4`  | `ConfigurationError`              | `appsettings.json` invalid or incomplete         |

## Services at a glance

| Service | Responsibility |
|---------|----------------|
| `SettingsValidator` | Validates `AppSettings` shape; pattern placeholders. |
| `FolderInitializer` | Creates required output folders. |
| `InputFileValidator` | Confirms the six input CSVs exist. |
| `CsvLoader` | Loads records from CSV with row-level issue capture. |
| `ValidationService` | Applies all rules and returns issues; `HasBlockingIssues`. |
| `HierarchyBuilder` | Assembles plan → … → intervention objects, indexed lookups, null-safe. |
| `TreatmentPlanOutputBuilder` | Flattens a hierarchy into a combined output record. |
| `CsvOutputWriter` | Writes the combined CSV. |
| `ValidationReportWriter` | Writes the validation report CSV. |
| `DebugJsonWriter` | Writes per-plan JSON debug files. |
| `ApplicationErrorLogger` | Writes fatal-error logs without machine/user specifics. |
| `IdNormalizer` | Case/whitespace-insensitive identifier comparison. |

## Design notes

- Services are plain classes with no external dependencies; every pipeline step is
  individually unit-tested.
- Input data never mutates during processing; builders produce new objects.
- Writes happen only after validation passes, so a failed run leaves no partial
  output files behind.