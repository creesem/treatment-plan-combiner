#requires -Version 5.1
<#
Read-only audit of the six raw CareLogic V1.0 CSV exports. No database connection.
Run: powershell.exe -NoProfile -File .\Check-CareLogicResults.ps1 -InputFolder C:\CareLogicReview\Exports
Exit codes: 0 = no row findings in completed checks; 1 = findings; 2 = incomplete inputs/checks or execution failure.
Source-only checks always remain unverified, including on exit 0. See README.md.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$InputFolder,
    [string]$OutputFolder,
    [string[]]$AllowedProgramIds = @('1003','1008','1016','1017'),
    [string[]]$NullTokens = @('NULL','\N')
)
$ErrorActionPreference = 'Stop'
$findings = New-Object 'System.Collections.Generic.List[object]'
$coverage = New-Object 'System.Collections.Generic.List[object]'
$inventory = New-Object 'System.Collections.Generic.List[object]'
$datasets = @{}
$entityColumns = @{ plans='document_id'; problems='problem_entity_id'; goals='goal_entity_id'; objectives='mod_tplan_entity_id'; interventions='mod_tplan_entity_id'; activities='mod_tplan_entity_id' }
$names = @('plans','problems','goals','objectives','interventions','activities')

function HeaderKey([string]$s) { return ($s.Trim().ToLowerInvariant() -replace '[\s_]','') }
function Value($row,[string]$column) {
    if ($null -eq $row) { return '' }
    $v = [string]$row.Fields[(HeaderKey $column)]
    $v = $v.Trim()
    if ($NullTokens -contains $v) { return '' }
    return $v
}
function Key($row,[string[]]$cols) {
    $parts = foreach ($col in $cols) { $v=Value $row $col; '{0}:{1}' -f $v.Length,$v }
    return ($parts -join '|')
}
function Finding([string]$level,[string]$code,[string]$dataset,$row,[string]$detail) {
    $record=0
    if ($null -ne $row) { $record=$row.Record }
    $findings.Add([pscustomobject][ordered]@{
        Level=$level; Check=$code; File=($dataset+'.csv'); SourceRecord=$record
        ClientId=(Value $row 'client_id'); DocumentId=(Value $row 'document_id')
        MasterId=(Value $row 'mod_tplan_master_id'); EntityId=(Value $row $entityColumns[$dataset])
        ProblemId=(Value $row 'problem_entity_id'); GoalId=(Value $row 'goal_entity_id')
        ObjectiveId=(Value $row 'objective_entity_id'); Detail=$detail
    })
}
function Ready([string]$check,$requirements) {
    $missing = New-Object 'System.Collections.Generic.List[string]'
    $count=0
    foreach ($name in $requirements.Keys) {
        $d=$datasets[$name]
        if ($null -eq $d -or -not $d.Loaded) { $missing.Add($name+': file not loaded'); continue }
        $count += $d.Rows.Count
        foreach ($col in $requirements[$name]) {
            if (-not $d.Headers.ContainsKey((HeaderKey $col))) { $missing.Add($name+': '+$col) }
        }
    }
    $status='CHECKED';$detail='Completed on loaded records; findings are in findings.csv.'
    if ($missing.Count -gt 0) { $status='NOT_RUN';$detail='Missing prerequisites: '+($missing -join '; ') }
    $coverage.Add([pscustomobject]@{Check=$check;Status=$status;RecordsAvailable=$count;Detail=$detail})
    return ($missing.Count -eq 0)
}
function Groups($rows,[string[]]$cols) {
    $g=@{}
    foreach($r in $rows) {
        $k=Key $r $cols
        if(-not $g.ContainsKey($k)) { $g[$k]=New-Object 'System.Collections.Generic.List[object]' }
        $g[$k].Add($r)
    }
    return ,$g
}
function ExportReport($rows,[string[]]$columns,[string]$path) {
    # Neutralize spreadsheet formulas without placing clinical narratives in reports.
    $safe=@(foreach($r in $rows) {
        $out=[ordered]@{}
        foreach($col in $columns) {
            $v=[string]$r.$col
            if($v -match '^[\s]*[=+@-]') { $v="'"+$v }
            $out[$col]=$v
        }
        [pscustomobject]$out
    })
    if($safe.Count -gt 0) { $safe | Export-Csv -LiteralPath $path -NoTypeInformation -Encoding UTF8 }
    else { Set-Content -LiteralPath $path -Value (($columns | ForEach-Object {'"'+$_+'"'}) -join ',') -Encoding UTF8 }
}

try {
    $inputPath=(Resolve-Path -LiteralPath $InputFolder).ProviderPath
    if(-not (Test-Path -LiteralPath $inputPath -PathType Container)) { throw 'InputFolder must be a directory.' }
    if([string]::IsNullOrWhiteSpace($OutputFolder)) {
        $OutputFolder=Join-Path (Split-Path $inputPath -Parent) ('CareLogic-audit-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[guid]::NewGuid().ToString('N').Substring(0,6))
    }
    $outPath=[IO.Path]::GetFullPath($OutputFolder)
    # Require a new output directory; never overwrite exports or previous reports.
    if(Test-Path -LiteralPath $outPath) { throw 'OutputFolder already exists. Choose a new folder.' }
    $null=New-Item -ItemType Directory -Path $outPath
    foreach($name in $names) {
        $file=Join-Path $inputPath ($name+'.csv')
        $d=@{Loaded=$false;Headers=@{};Rows=@()};$datasets[$name]=$d
        try {
            if(-not(Test-Path -LiteralPath $file -PathType Leaf)) { throw 'Required CSV is missing.' }
            $first=Get-Content -LiteralPath $file -Encoding UTF8 -TotalCount 1
            if([string]::IsNullOrWhiteSpace($first)) { throw 'CSV is empty or has no header.' }
            # A one-line CSV header is required; quoted multiline DATA values are supported.
            $probe=ConvertFrom-Csv -InputObject ($first+"`r`n"+'"__header_probe__"')
            foreach($p in $probe.PSObject.Properties) {
                $k=HeaderKey $p.Name
                if($d.Headers.ContainsKey($k)) { throw 'Duplicate normalized column name.' }
                $d.Headers[$k]=$p.Name
            }
            $records=New-Object 'System.Collections.Generic.List[object]'
            $i=0
            Import-Csv -LiteralPath $file -Encoding UTF8 | ForEach-Object {
                $i++;$fields=@{}
                foreach($p in $_.PSObject.Properties) { $fields[(HeaderKey $p.Name)]=[string]$p.Value }
                $records.Add([pscustomobject]@{Record=$i;Fields=$fields})
            }
            $d.Rows=@($records.ToArray());$d.Loaded=$true
            $inventory.Add([pscustomobject]@{File=($name+'.csv');Status='LOADED';Records=$i;Detail='Header parsed; '+$d.Headers.Count+' columns.'})
            if($i -eq 0) { Finding 'REVIEW' 'EMPTY_RESULT' $name $null 'Header is present but there are no data records. Confirm that an empty result was expected.' }
        } catch {
            $inventory.Add([pscustomobject]@{File=($name+'.csv');Status='NOT_LOADED';Records=0;Detail='Missing, empty, or unreadable CSV/header. Check the file and its export format.'})
            Finding 'ERROR' 'INPUT_NOT_LOADED' $name $null 'File could not be loaded. Dependent checks are NOT_RUN; this is not a clean result.'
        }
    }

    # Core identifiers: actual missing values are different from missing column headings.
    foreach($name in $names) {
        $cols=@('client_id','document_id')
        if($name -ne 'plans') { $cols+=@('mod_tplan_master_id',$entityColumns[$name]) }
        if(Ready ('IDENTIFIERS_'+$name) @{$name=$cols}) {
            foreach($r in $datasets[$name].Rows) {
                foreach($col in $cols) {
                    if((Value $r $col) -eq '') {
                        $level='ERROR';if($name -eq 'activities' -and $col -eq 'mod_tplan_entity_id') {$level='REVIEW'}
                        Finding $level 'MISSING_IDENTIFIER' $name $r ('Blank '+$col+'.')
                    }
                }
            }
        }
    }
    if(Ready 'PROGRAM_SCREEN' @{plans=@('program_id')}) {
        foreach($r in $datasets.plans.Rows) {
            $id=Value $r 'program_id'
            if($id -eq '' -or $AllowedProgramIds -notcontains $id) {
                Finding 'REVIEW' 'PROGRAM_OUTSIDE_EXPECTED_SET' 'plans' $r ('Program is blank or outside the allowed set: '+($AllowedProgramIds -join ', ')+'. Confirm eligibility context; this alone does not prove why it occurred.')
            }
        }
    }
    if(Ready 'MISSING_RANKED_DATES' @{plans=@('txp_begin_date','txp_created_date')}) {
        foreach($r in $datasets.plans.Rows) {
            foreach($col in @('txp_begin_date','txp_created_date')) {
                if((Value $r $col) -eq '') { Finding 'REVIEW' 'BLANK_RANKED_DATE' 'plans' $r ($col+' is blank. Compare the other eligible plans to determine whether the wrong plan was selected.') }
            }
        }
    }
    if(Ready 'ONE_PLAN_PER_CLIENT' @{plans=@('client_id','document_id')}) {
        $groups=Groups $datasets.plans.Rows @('client_id')
        foreach($g in $groups.Values) {
            if($g.Count -gt 1 -and (Value $g[0] 'client_id') -ne '') {
                foreach($r in $g) { Finding 'ERROR' 'MULTIPLE_BASE_ROWS' 'plans' $r 'More than one base record for this client. V1.0 returns one base row per client; confirm the file was not appended or combined across runs.' }
            }
        }
    }
    foreach($name in @('problems','goals','objectives','interventions','activities')) {
        if(Ready ('BASE_LINK_'+$name) @{plans=@('document_id','client_id');$name=@('document_id','client_id')}) {
            $base=Groups $datasets.plans.Rows @('document_id','client_id')
            foreach($r in $datasets[$name].Rows) {
                if((Value $r 'document_id') -ne '' -and (Value $r 'client_id') -ne '' -and -not $base.ContainsKey((Key $r @('document_id','client_id')))) {
                    Finding 'ERROR' 'NO_MATCHING_BASE_RECORD' $name $r 'No base row with this document and client. Compare exports from the same run before treating this as a source-data defect.'
                }
            }
        }
    }

    # Parent checks retain client, document, module/master, and ancestor context.
    $parentChecks=@(
        @{Child='goals';Parent='problems';ChildCols=@('client_id','document_id','mod_tplan_master_id','problem_entity_id');ParentCols=@('client_id','document_id','mod_tplan_master_id','problem_entity_id');Link='problem_entity_id'},
        @{Child='objectives';Parent='goals';ChildCols=@('client_id','document_id','mod_tplan_master_id','problem_entity_id','goal_entity_id');ParentCols=@('client_id','document_id','mod_tplan_master_id','problem_entity_id','goal_entity_id');Link='goal_entity_id'},
        @{Child='interventions';Parent='objectives';ChildCols=@('client_id','document_id','mod_tplan_master_id','problem_entity_id','goal_entity_id','objective_entity_id');ParentCols=@('client_id','document_id','mod_tplan_master_id','problem_entity_id','goal_entity_id','mod_tplan_entity_id');Link='objective_entity_id'},
        @{Child='activities';Parent='interventions';ChildCols=@('client_id','document_id','mod_tplan_master_id','mod_tplan_entity_id');ParentCols=@('client_id','document_id','mod_tplan_master_id','mod_tplan_entity_id');Link='mod_tplan_entity_id'}
    )
    foreach($c in $parentChecks) {
        $req=@{};$req[$c.Child]=$c.ChildCols;$req[$c.Parent]=$c.ParentCols
        if(Ready ('PARENT_PATH_'+$c.Child) $req) {
            $parents=Groups $datasets[$c.Parent].Rows $c.ParentCols
            foreach($r in $datasets[$c.Child].Rows) {
                if((Value $r $c.Link) -eq '') {
                    Finding 'REVIEW' 'NO_PARENT_LINK' $c.Child $r ('No '+$c.Link+'. Confirm whether this standalone/direct relationship is valid for the receiving system.')
                } elseif(-not $parents.ContainsKey((Key $r $c.ChildCols))) {
                    Finding 'REVIEW' 'PARENT_PATH_NOT_RETURNED' $c.Child $r ('No matching '+$c.Parent+' record with the same client, document, master and available ancestor path. May reflect differing export times, a missing branch, or an allowed standalone relationship.')
                }
            }
        }
    }
    # Repeated IDs are evidence for review, not automatically erroneous source rows.
    foreach($name in @('problems','goals','objectives','interventions')) {
        $col=$entityColumns[$name]
        if(Ready ('REPEATED_IDS_'+$name) @{$name=@($col)}) {
            $groups=Groups $datasets[$name].Rows @($col)
            foreach($g in $groups.Values) {
                if($g.Count -gt 1 -and (Value $g[0] $col) -ne '') {
                    foreach($r in $g) { Finding 'REVIEW' 'REPEATED_ENTITY_ID' $name $r ('Entity appears '+$g.Count+' times. Relationships/details can validly repeat it; the local demo combiner rejects repeated IDs. Do not delete rows on this flag alone.') }
                }
            }
        }
    }
    if(Ready 'SHARED_GOAL_BRANCHES' @{goals=@('client_id','document_id','mod_tplan_master_id','goal_entity_id','problem_entity_id')}) {
        $groups=Groups $datasets.goals.Rows @('client_id','document_id','mod_tplan_master_id','goal_entity_id')
        foreach($g in $groups.Values) {
            $parents=@($g | ForEach-Object {Value $_ 'problem_entity_id'} | Where-Object {$_ -ne ''} | Sort-Object -Unique)
            if($parents.Count -gt 1) {
                foreach($r in $g) { Finding 'REVIEW' 'SHARED_GOAL_BRANCH' 'goals' $r 'This goal is used under multiple problems in the same plan. Confirm each objective stays under its own problem/goal branch in the final integration.' }
            }
        }
    }
    # One record per file identifies the format issue; raw exports are not rewritten.
    foreach($name in $names) {
        if(Ready ('LOCAL_DEMO_HEADERS_'+$name) @{$name=@('client_id','document_id')}) {
            if(-not $datasets[$name].Headers.ContainsKey('id') -or -not $datasets[$name].Headers.ContainsKey('patientid')) {
                Finding 'INFO' 'LOCAL_DEMO_HEADER_DIFFERENCE' $name $null 'Raw document_id/client_id headers differ from local demo Id/PatientId. Expected with raw exports. This check does not validate the entire production mapping.'
            }
        }
    }
    foreach($c in @(
        @('SELECTED_ENROLLMENT','Final exports do not retain the selected eligible enrollment/episode or tprog deletion state. Even an allowed program_id does not prove the correct enrollment was used.'),
        @('WINNING_PLAN','The selected row cannot show discarded eligible plans or their dates. Program admission date cp.begin_date is also absent from the base output.'),
        @('PRODUCTION_MAPPING','Production header/value mapping and actual ingestion behavior need the approved consumer contract and a separate import test.'),
        @('FINAL_HIERARCHY','Raw relationship rows cannot prove whether the final Eleos document preserves the correct branches. Compare the combined output.'),
        @('SOURCE_CARDINALITY','No source-table uniqueness, diagnosis code/name pairing, source deletion state or live query compilation check is possible from these files alone.')
    )) { $coverage.Add([pscustomobject]@{Check=$c[0];Status='SOURCE_REQUIRED';RecordsAvailable=0;Detail=$c[1]}) }

    ExportReport $findings @('Level','Check','File','SourceRecord','ClientId','DocumentId','MasterId','EntityId','ProblemId','GoalId','ObjectiveId','Detail') (Join-Path $outPath 'findings.csv')
    ExportReport $coverage @('Check','Status','RecordsAvailable','Detail') (Join-Path $outPath 'coverage.csv')
    ExportReport $inventory @('File','Status','Records','Detail') (Join-Path $outPath 'files.csv')
    $errors=@($findings | Where-Object {$_.Level -eq 'ERROR'}).Count
    $reviews=@($findings | Where-Object {$_.Level -eq 'REVIEW'}).Count
    $infos=@($findings | Where-Object {$_.Level -eq 'INFO'}).Count
    $skipped=@($coverage | Where-Object {$_.Status -eq 'NOT_RUN'}).Count
    $summary=@(
        ('CareLogic result review - '+(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')),
        ('Input: '+$inputPath),
        ('ERROR findings: {0}; REVIEW findings: {1}; INFO findings: {2}' -f $errors,$reviews,$infos),
        ('Checks not run because prerequisites were missing: {0}' -f $skipped),
        'Start with files.csv and coverage.csv. Then filter findings.csv by Check and Level.',
        'SourceRecord is the 1-based DATA RECORD (excluding header), not a physical line when fields contain line breaks. 0 means file-level finding.',
        'Multiple findings may refer to the same record; counts are not unique clients or unique defects.',
        'ERROR = inconsistency in the supplied exports or unusable input; REVIEW = candidate needing explanation; INFO = context.',
        'No findings does not establish that the correct plan/program was chosen. SOURCE_REQUIRED checks remain unverified.',
        'Identifiers are retained to locate records. No narratives, diagnoses, names or emails are copied into the reports.',
        'Open identifiers as TEXT in Excel to preserve leading zeros and long IDs. Keep reports with the source exports under office access controls.'
    )
    Set-Content -LiteralPath (Join-Path $outPath 'summary.txt') -Value $summary -Encoding UTF8
    Write-Output ('Reports: '+$outPath)
    Write-Output ('ERROR={0}; REVIEW={1}; INFO={2}; NOT_RUN={3}. Source comparison still required.' -f $errors,$reviews,$infos,$skipped)
    if($skipped -gt 0 -or @($inventory | Where-Object {$_.Status -ne 'LOADED'}).Count -gt 0) {exit 2}
    if($errors -gt 0 -or $reviews -gt 0) {exit 1}
    exit 0
} catch {
    [Console]::Error.WriteLine('Audit could not complete. Check that InputFolder exists and OutputFolder is a new writable directory. No clean result should be inferred.')
    exit 2
}
