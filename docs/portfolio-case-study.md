# Portfolio case study

## Summary

This repository is a clean-room, self-contained rebuild of an internal data-
transformation pipeline. The original system loaded six linked CSV exports,
validated them, and combined them into a single treatment-plan document. The
version here preserves the *shape* of the problem while re-implementing the 
solution from scratch with no proprietary code and no real data.

## The problem

Treatment plans were maintained as several related CSV exports (plans, problems,
goals, objectives, interventions, activities). A completed plan had to be:

1. **validated** for referential integrity and completeness,
2. **grouped** into a single hierarchical document per plan, and
3. **exported** in a form reviewers could consume.

Relational validation was the hard part: child records could reference parents that
did not exist (orphans), identifiers could be duplicated, and a plan with missing
pieces should be flagged rather than silently exported.

## What was delivered

- A six-file CSV loader with row-level, machine-readable error capture.
- A validation engine (15 blocking error classes + data-quality warnings) implemented
  with typed enums.
- An indexed, null-safe hierarchy builder (plan → problem → goal → objective →
  intervention).
- Combined CSV + per-plan debug JSON output and a validation report.
- Deterministic exit codes (`0`–`4`) suitable for scripting.
- A 46-test xUnit suite covering the loader, validator, hierarchy builder, output
  builder, CSV writer, and the full application runner.
- Fictional sample data committed so the project runs immediately.

## Notable challenges solved

- **Legacy loader bug.** `CsvHelper.GetRecords<T>()` returns nothing when headers
  are read at an incorrect position. Diagnosed with targeted experiments and fixed in
  the loader (read → read header → materialize records); regression-tested.
- **Quiet data loss.** Malformed rows could be dropped silently. The loader now
  converts unparseable rows into blocking issues with the offending row number
  attached.
- **Orphan vs. blank-parent ambiguity.** A blank parent identifier must produce a
  "missing" error, not a misleading "orphan" one. Validation separates the two cases
  explicitly.
- **Cross-runtime portability.** Built on `net8.0` with `RollForward` so the demo runs
  on .NET 10 machines without requiring a .NET 8 install, while CI uses a pinned
  .NET 8 SDK.

## What I would do differently at scale

- **FluentValidation** or a rule pipeline for the growing rule set.
- **Cancellation + streaming** for very large exports.
- **Per-run correlation IDs** in logs.
- A persistence tier instead of in-memory only.

## Key takeaways

- Clear separation of load → validate → build → write made every stage independently
  testable and dramatically improved confidence in the output.
- Treating bad input as *blocking* (fail loud) rather than filtering it quietly is
  the difference between a pipeline you can trust and one you cannot.