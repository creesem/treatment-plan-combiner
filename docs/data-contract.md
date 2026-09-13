# Data contract

The app reads **six** CSV files that together describe treatment plans. Each file's
first row must be a header row. Header matching is case-insensitive and tolerant of
spaces/underscores. An unrecognized or missing column is ignored for that file and
recorded as a load issue when it matters.

## Input files

All files live in `Folders.Input` (default `Input/sample`).

| File | Required column(s) | Relates to |
|------|--------------------|------------|
| `treatment_plans.csv` | `Id`, `Patient Id` | root of a plan |
| `problems.csv` | `Id`, `Patient Id`, `Problem Id` | `treatment_plans.Id` via `problems.Id` |
| `goals.csv` | `Id`, `Patient Id`, `Goal Id`, `Problem Id` | `problems.Problem Id` |
| `objectives.csv` | `Id`, `Patient Id`, `Objective Id`, `Goal Id` | `goals.Goal Id` |
| `interventions.csv` | `Id`, `Patient Id`, `Intervention Id`, `Objective Id` | `objectives.Objective Id` |
| `activities.csv` | `Id`, `Patient Id`, `Plan Field Act Activity Id` | each activity |

### Identifier conventions

- Two rows "link" when a child's parent column contains the parent's identifier.
- Identifiers are compared **case-insensitively and with leading/trailing
  whitespace removed** (`IdNormalizer`).
- A treatment plan is identified by its `Id`; lower-level records are identified by
  their `* Id` columns (for example `Goal Id`).

### The `Id` column

For problems, goals, objectives, interventions, and activities, `Id` is the column
that matches the **parent** plan's `Id`. The record's own identifier is the
`* Id` column (e.g. `Goal Id`). This is how the loader and validator know which plan
a record belongs to.

## Relationships

```
Treatment plan (treatment_plans.csv)
└─ Problem (problems.csv, linked via Id = plan Id)
   └─ Goal (goals.csv, linked via Problem Id)
      └─ Objective (objectives.csv, linked via Goal Id)
         └─ Intervention (interventions.csv, linked via Objective Id)
Activities (activities.csv) attach to a plan by Id.
```

## Outputs

### Combined CSV (`Output/treatment_plans_<timestamp>.csv`)

One row per treatment plan:

| Column | Content |
|--------|---------|
| `TreatmentPlanId` | The plan's `Id`. |
| `PatientId` | The plan's `Patient Id`. |
| `TreatmentPlanDataJson` | Full hierarchical plan document as JSON. |

### Debug JSON (`Output/DebugJson/<plan id>.json`)

One file per plan with the complete hierarchy: problems (with goals → objectives →
interventions nested inside) and activities.

### Validation report (`Logs/validation_report_<timestamp>.csv`)

One row per validation issue:

| Column | Content |
|--------|---------|
| `Severity` | `Error` or `Warning`. |
| `IssueType` | Machine-readable issue category (e.g. `OrphanGoal`). |
| `TreatmentPlanId` | Affected plan, when identifiable. |
| `RecordId` | Affected record identifier, when identifiable. |
| `Message` | Human-readable description. |