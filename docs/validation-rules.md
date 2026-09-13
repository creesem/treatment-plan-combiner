# Validation rules

Issues come in two severities:

- **`Error` — blocking.** If any error is found, the app writes **no** output and
  exits with code `1`.
- **`Warning` — non-blocking.** Reported in the validation report only; output is
  still produced.

All rules are implemented in `Services/ValidationService.cs` and enumerated by
`Models/IssueType.cs`.

## Referential integrity (Error)

Rules that stop output when violated:

| Issue | Condition |
|-------|-----------|
| `MissingTreatmentPlanId` | A plan row has a blank `Id`. |
| `MissingPatientId` | A plan row has a blank `Patient Id`. |
| `MissingProblemId` / `MissingGoalId` / `MissingObjectiveId` / `MissingInterventionId` | A record's own `* Id` column is blank. |
| `MissingProblemIdOnGoal` | A goal has a blank `Problem Id`. |
| `MissingGoalIdOnObjective` | An objective has a blank `Goal Id`. |
| `MissingObjectiveIdOnIntervention` | An intervention has a blank `Objective Id`. |
| `OrphanProblem` | A problem's `Id` references a plan that does not exist. |
| `OrphanGoal` | A goal's `Problem Id` references a problem that does not exist. |
| `OrphanObjective` | An objective's `Goal Id` references a goal that does not exist. |
| `OrphanIntervention` | An intervention's `Objective Id` references an objective that does not exist. |
| `DuplicateTreatmentPlanId` / `DuplicateProblemId` / `DuplicateGoalId` / `DuplicateObjectiveId` / `DuplicateInterventionId` | The same identifier appears more than once (compared normalized). |
| `MalformedCsvRow` | A CSV row could not be parsed (e.g. unbalanced quotes) or the file is empty. |

## Data quality (Warning)

Non-blocking but worth reviewing:

| Issue | Condition |
|-------|-----------|
| `TreatmentPlanHasNoProblems` | A plan has no linked problems. |
| `ProblemHasNoGoals` | A problem has no linked goals. |
| `GoalHasNoObjectives` | A goal has no linked objectives. |
| `ObjectiveHasNoInterventions` | An objective has no linked interventions. |
| `MissingProblemNarrative` / `MissingGoalNarrative` / `MissingObjectiveNarrative` / `MissingInterventionNarrative` | The record's narrative field is blank. |

## How decisions are made

- Missing parent identifiers are reported once per offending record (the loop that
  finds the parent issue also emits the orphan check only when an identifier is
  present).
- Orphan checks deliberately compare against the set of **parent** identifiers, so
  a blank parent yields `Missing*` rather than a misleading orphan report.
- Identifiers are normalized before every comparison so `ABC-123` and `abc-123`
  are treated as the same record.