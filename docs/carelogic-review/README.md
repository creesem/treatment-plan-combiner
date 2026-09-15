# CareLogic office result review kit

Use this kit to lead the team discussion and locate candidate records in the six raw V1.0 query exports. It does not connect to CareLogic or Snowflake, run vendor SQL, modify exports, or send data anywhere.

## What is included

- `Team-walkthrough.pdf`: speaking notes, team questions, and a guide to the checker results.
- `Visual-review.pdf`: the four illustrated issues discussed in the walkthrough.
- `Check-CareLogicResults.ps1`: the local CSV checker.
- `Test-Checker.ps1`: a self-test that generates only synthetic records in a new temporary folder.
- `Sample-exports` and `Sample-report`: a small fictional example and its expected checker output. These show the fields used by the checker, not every column in the real vendor queries.

## Before leaving for the office

Use this repository's `docs/carelogic-review` folder, or copy the entire folder to an approved local location. The checker uses standard PowerShell and requires no modules or database driver. It is written for Windows PowerShell 5.1 or PowerShell 7. Your office must permit local scripts to run. If script execution is blocked, use your IT-approved method; the kit does not change execution policy.

Verification here: all 12 synthetic scenarios passed in PowerShell 7.6.5. Windows PowerShell 5.1 parsed the script with zero syntax errors, but its execution policy blocked running it here. Full 5.1 runtime behavior has not been verified. Run the self-test in the office-approved environment before using actual exports.

## 1. Save all six raw query results

Use comma-separated UTF-8 CSV with the original column headings. Export IDs as text, without rounding, scientific notation or thousands separators. Save directly from the query tool where possible. Do not open and resave the files in Excel before checking them; long IDs and leading zeros can be altered.

Save the six files in one folder, for example `C:\CareLogicReview\Exports`:

| Vendor query | Save result as |
|---|---|
| SNOWFLAKE_Eleos Base Treatment Plan Query_V1.0.sql | plans.csv |
| SNOWFLAKE_Canopy_Problems_V1.0.sql | problems.csv |
| SNOWFLAKE_Canopy_Goals_V1.0.sql | goals.csv |
| SNOWFLAKE_Canopy_Objectives_V1.0.sql | objectives.csv |
| SNOWFLAKE_Canopy_Interventions_V1.0.sql | interventions.csv |
| SNOWFLAKE_Canopy_Activities_V1.0.sql | activities.csv |

Keep headers even if a result has no records. The header must occupy one line. Quoted commas and line breaks inside data fields are supported. Do not include an Excel `sep=,` line or a title above the header. Uppercase Snowflake headers are accepted, as are differences in spaces and underscores. Duplicate normalized header names are rejected. Semicolon-separated files and spreadsheets are not supported.

Use the same reporting date and as consistent a source refresh as possible for all six exports. Record the query version, export time, timezone, and available Snowflake query ID for each. A record absent from a later export may reflect a changed source rather than a faulty relationship. Keep each run separate; never append a new run to an old file.

## 2. Run the checker

Open PowerShell in the kit folder and run:

```powershell
powershell.exe -NoProfile -File .\Check-CareLogicResults.ps1 -InputFolder "C:\CareLogicReview\Exports"
```

If your approved environment uses PowerShell 7:

```powershell
pwsh -NoProfile -File .\Check-CareLogicResults.ps1 -InputFolder "C:\CareLogicReview\Exports"
```

A new report folder is created beside `Exports`, with a timestamp and unique suffix. Its location is printed at the end. You can instead specify a new folder:

```powershell
.\Check-CareLogicResults.ps1 -InputFolder "C:\CareLogicReview\Exports" -OutputFolder "C:\CareLogicReview\Review-September-15"
```

An existing output folder is refused to protect previous results. The checker reads the files into memory; very large exports may require more memory than is available on an office workstation. It has not been performance-tested against production volumes.

The default allowed programs are `1003, 1008, 1016, 1017`, matching the supplied queries. These are review assumptions, not a universal CareLogic rule. Blank cells, `NULL`, and `\N` are treated as missing values. ID strings are trimmed but never converted to numbers.

## 3. Read the reports in this order

1. **files.csv:** all six files should say `LOADED`. A header-only export has zero records and also produces an `EMPTY_RESULT` review finding.
2. **coverage.csv:** `CHECKED` means the stated export check ran. `NOT_RUN` means required files or columns were missing. `SOURCE_REQUIRED` means the answer is not available from final exports alone.
3. **findings.csv:** filter by `Check` and `Level` to find candidate records. It includes file, source record number, client ID, document ID, master ID and available relationship IDs.
4. **summary.txt:** gives totals, instructions, and limitations. Counts are findings, not unique clients or unique defects; one record may have several flags.

`SourceRecord=1` is the first data record after the header. It is not necessarily physical line 2 because a quoted narrative can span multiple lines. `SourceRecord=0` means a file-level finding. Match IDs as well as the record number. Import report identifier columns into Excel as **Text** to preserve long IDs and leading zeros. Formula-like strings are prefixed with an apostrophe in the reports so they are not executed as spreadsheet formulas.

### What the levels mean

- **ERROR:** an inconsistency in the supplied exports or unusable input. It is not automatically proof of an EHR defect: combined runs and incomplete exports can cause these findings.
- **REVIEW:** a candidate requiring context. Repeated goal IDs and direct/standalone relationships can be valid.
- **INFO:** explanatory context. Raw column-name differences from the local demo are expected and do not establish a vendor contract violation.

### How the flags relate to the four illustrated issues

| Issue | Filter findings.csv by | What this establishes |
|---|---|---|
| 1. Wrong program context | PROGRAM_OUTSIDE_EXPECTED_SET | A blank or unexpected returned program. It does not identify every wrong enrollment. |
| 2. Missing date wins | BLANK_RANKED_DATE | The returned plan begin or creation date is blank. It does not prove another plan should have won. |
| 3. Receiving labels | LOCAL_DEMO_HEADER_DIFFERENCE | Raw document/client headers differ from the local demo's Id/PatientId. File-level information only. |
| 4. Shared goal branches | REPEATED_ENTITY_ID, SHARED_GOAL_BRANCH, PARENT_PATH_NOT_RETURNED, NO_PARENT_LINK | The particular repeated or unmatched relationship records to examine. These are review flags, not instructions to delete rows. |

Additional checks identify missing IDs, multiple base records for one client, and component records without a matching base document/client. Parent comparisons retain client, document, master, and available ancestor IDs so an objective cannot match a goal in a different problem simply because the goal ID is the same. Activity parent checks use the returned intervention ID; blank `activity_entity_id` and blank activity problem/goal/objective fields are allowed for the first activity-query branch.

### Exit codes

- `0`: no ERROR or REVIEW findings in completed export checks. INFO and SOURCE_REQUIRED entries can still exist.
- `1`: ERROR or REVIEW findings exist.
- `2`: inputs/checks are incomplete or the checker could not finish.

**Exit 0 is not approval of the queries or proof that the correct plan was selected.**

## What to request for concerns the final results cannot prove

For a flagged client/document, ask CareLogic to compare the records used before the final selection:

- **Program context:** which live enrollment and episode made the plan eligible, which enrollment supplied the final output, and whether a deleted program link participated. The base export does not retain those selection details.
- **Plan choice:** all eligible candidate plans for that client, their begin/creation dates, program-admission sort dates, and the reason the returned row was selected. Discarded plans are absent from the results.
- **Field compatibility:** the agreed production output headings and a receiving-system import result. This checker only knows the local demo's basic ID names.
- **Relationships:** the full problem/goal/objective path for the flagged IDs, compared with the final combined Eleos output. Raw rows cannot prove how the final document was assembled.

The checker does not infer diagnosis code/name correctness, source-table uniqueness, deleted source rows, future/unsigned plan eligibility, date parsing semantics, or real-time snapshot consistency. It is a focused export triage tool.

## Team evidence log

For each candidate, record: check name; file and SourceRecord; client/document/entity IDs; the team's expected result; observed result; query version/time/query ID; vendor explanation; and status (open, explained as valid, confirmed defect, or resolved). The report includes locator IDs but deliberately omits names, emails, diagnoses and narratives. Store it with the source exports under the same office access controls.

## Run the synthetic self-test

For a quick demonstration before the meeting, run:

```powershell
.\Check-CareLogicResults.ps1 -InputFolder .\Sample-exports
```

The included sample has an unexpected program, a missing begin date, and a valid shared goal. Expect 6 REVIEW findings, 6 INFO findings, no ERROR findings, and exit code 1. The repeated goal is intentional, not a defective clinical relationship. Compare your output with `Sample-report`.

For the full test suite:

```powershell
.\Test-Checker.ps1
```

For PowerShell 7:

```powershell
.\Test-Checker.ps1 -PowerShellExe pwsh
```

The self-test prints PASS and the temporary folder containing synthetic exports and reports. It covers valid data, visible issues, legitimate shared goals, missing input, missing columns, wrong ancestors, duplicate base rows, uppercase headers, empty results, formula-like IDs, and refusal to overwrite existing reports. No production files are used.
