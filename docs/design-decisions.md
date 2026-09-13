# Design decisions

This document records notable choices and the reasoning behind them.

## Target framework: `net8.0` with `RollForward=LatestMajor`

The app is developed and demonstrated in an environment that only has the .NET 10
SDK/runtime installed. It targets `net8.0` so builds pass strict CI checks on a
pinned .NET 8 SDK. `RollForward=LatestMajor` lets it run locally on the .NET 10
runtime, so no .NET 8 runtime is required on the developer machine.

**Why not `net10.0`?** A portfolio audience (recruiters/interviewers) is more likely
to have a stable LTS .NET runtime installed. `net8.0` is LTS and maximizes the
chance the project runs anywhere without extra installs.

## No external output-generating libraries

The original codebase depended on an internal template/rendering system. This
standalone version re-implements output generation with plain C# (`StringBuilder`,
`System.Text.Json`, `CsvHelper` for CSV reading) — no document-generation
dependencies. This keeps the demo dependency-light (~4 packages total) and makes
every behavior unit-testable.

## CsvHelper requires `Read()` before `ReadHeader()`

The loader calls `csv.Read()` then `csv.ReadHeader()` explicitly. Using
`GetRecords<T>()` on its own causes CsvHelper to attempt a header read position that
fails ("header with name … was not found"), which silently yields empty results.
This was diagnosed against a real failure and is covered by tests.

## Loader vs. validator responsibilities

The loader is responsible for **shape** (can a row be parsed? does a header exist?)
and the validator for **semantics** (are identifiers valid? relationships intact?).
Malformed rows and empty files produce `MalformedCsvRow` **errors** at load time, so
broken input is never silently dropped.

## Typed enums instead of strings

`IssueType` and `IssueSeverity` are enums rather than free-form strings. This makes
the validation report machine-readable, makes the rule set enumerable in tests, and
removes a whole class of typos.

## Cross-platform path and console handling

- `InvariantGlobalization` keeps formatting deterministic regardless of locale.
- Output file names use a `{timestamp}` placeholder replaced at runtime, so two runs
  never overwrite each other.
- Line endings use the current platform's default so generated CSVs open cleanly on
  Windows and Unix.

## Committed sample data, ignored real data

`Input/sample/` is committed so the demo runs out of the box; anything else under
`Input/` is git-ignored so private/real exports can never be committed accidentally.
All sample data is fictional.

## What is deliberately out of scope

- No database, web API, or external services — the demo is self-contained.
- No authentication/authorization — it is a batch pipeline, not a service.
- No telemetry or third-party logging frameworks.
- Output is a debug/example format, not a regulated clinical document.