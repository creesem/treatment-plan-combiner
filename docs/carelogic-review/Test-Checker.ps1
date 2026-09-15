#requires -Version 5.1
[CmdletBinding()]
param([string]$PowerShellExe='powershell.exe')
$ErrorActionPreference='Stop'
$checker=Join-Path $PSScriptRoot 'Check-CareLogicResults.ps1'
$testRoot=Join-Path ([IO.Path]::GetTempPath()) ('carelogic-selftest-'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory -Path $testRoot
function Assert($condition,[string]$message) { if(-not $condition) {throw $message} }
function Fixture([string]$name) {
    $dir=Join-Path $testRoot $name;$null=New-Item -ItemType Directory -Path $dir
    $data=@{
        plans=@([pscustomobject]@{client_id='0001';document_id='12345678901234567';program_id='1003';txp_begin_date='2026-09-01';txp_created_date='2026-09-01 09:00:00'})
        problems=@([pscustomobject]@{client_id='0001';document_id='12345678901234567';mod_tplan_master_id='M1';problem_entity_id='P1'})
        goals=@([pscustomobject]@{client_id='0001';document_id='12345678901234567';mod_tplan_master_id='M1';problem_entity_id='P1';goal_entity_id='G1';objective_entity_id='O1';goal_narrative="Synthetic text, with comma`r`nand a quoted `"word`"."})
        objectives=@([pscustomobject]@{client_id='0001';document_id='12345678901234567';mod_tplan_master_id='M1';problem_entity_id='P1';goal_entity_id='G1';mod_tplan_entity_id='O1'})
        interventions=@([pscustomobject]@{client_id='0001';document_id='12345678901234567';mod_tplan_master_id='M1';problem_entity_id='P1';goal_entity_id='G1';objective_entity_id='O1';mod_tplan_entity_id='I1'})
        activities=@([pscustomobject]@{client_id='0001';document_id='12345678901234567';mod_tplan_master_id='M1';mod_tplan_entity_id='I1';activity_entity_id='';problem_entity_id='';goal_entity_id='';objective_entity_id='';activity_id='A1'})
    }
    return @{Dir=$dir;Data=$data}
}
function Run($f) {
    foreach($n in $f.Data.Keys) { $f.Data[$n] | Export-Csv -LiteralPath (Join-Path $f.Dir ($n+'.csv')) -NoTypeInformation -Encoding UTF8 }
    $out=Join-Path $testRoot ((Split-Path $f.Dir -Leaf)+'-reports')
    $messages=& $PowerShellExe -NoProfile -File $checker -InputFolder $f.Dir -OutputFolder $out 2>&1
    $code=$LASTEXITCODE
    Assert (Test-Path -LiteralPath (Join-Path $out 'findings.csv')) ('No reports: '+($messages -join ' '))
    return @{Exit=$code;Rows=@(Import-Csv -LiteralPath (Join-Path $out 'findings.csv'));Coverage=@(Import-Csv -LiteralPath (Join-Path $out 'coverage.csv'));Out=$out}
}

$f=Fixture 'valid';$r=Run $f
Assert ($r.Exit -eq 0) 'Valid exports should have no row findings.'
Assert (@(Get-Content -LiteralPath (Join-Path $r.Out 'summary.txt')).Count -eq 11) 'Summary must contain 11 readable lines.'
Assert (@($r.Rows | Where-Object {$_.Level -ne 'INFO'}).Count -eq 0) 'Standalone activity fields must not create false positives.'
Assert (@($r.Coverage | Where-Object {$_.Status -eq 'SOURCE_REQUIRED'}).Count -eq 5) 'Source-only checks must stay unverified.'

$f=Fixture 'visible-issues'
$f.Data.plans[0].program_id='9999';$f.Data.plans[0].txp_begin_date='NULL'
$f.Data.goals[0].document_id='other-document'
$r=Run $f
foreach($code in @('PROGRAM_OUTSIDE_EXPECTED_SET','BLANK_RANKED_DATE','NO_MATCHING_BASE_RECORD','PARENT_PATH_NOT_RETURNED')) {
    Assert (@($r.Rows | Where-Object {$_.Check -eq $code}).Count -gt 0) ('Expected '+$code)
}
$idRow=@($r.Rows | Where-Object {$_.Check -eq 'BLANK_RANKED_DATE'})[0]
Assert ($idRow.ClientId -ceq '0001' -and $idRow.DocumentId -ceq '12345678901234567') 'Identifiers must retain leading zeros and precision.'
Assert ((Get-Content -LiteralPath (Join-Path $r.Out 'findings.csv') -Raw) -notmatch 'Synthetic text') 'Narrative must not be included in findings.'

$f=Fixture 'shared-goal'
$f.Data.problems+= [pscustomobject]@{client_id='0001';document_id='12345678901234567';mod_tplan_master_id='M1';problem_entity_id='P2'}
$f.Data.goals+= [pscustomobject]@{client_id='0001';document_id='12345678901234567';mod_tplan_master_id='M1';problem_entity_id='P2';goal_entity_id='G1';objective_entity_id='O2';goal_narrative='Synthetic second branch'}
$f.Data.objectives+= [pscustomobject]@{client_id='0001';document_id='12345678901234567';mod_tplan_master_id='M1';problem_entity_id='P2';goal_entity_id='G1';mod_tplan_entity_id='O2'}
$r=Run $f
Assert ($r.Exit -eq 1) 'Shared goal should require review.'
Assert (@($r.Rows | Where-Object {$_.Check -eq 'SHARED_GOAL_BRANCH'}).Count -eq 2) 'Both shared-goal records must be locatable.'
Assert (@($r.Rows | Where-Object {$_.Check -eq 'SHARED_GOAL_BRANCH' -and $_.SourceRecord -eq '2'}).Count -eq 1) 'Record number must remain correct after a quoted multiline field.'
Assert (@($r.Rows | Where-Object {$_.Level -eq 'ERROR'}).Count -eq 0) 'Valid repeated relationships are not proven defects.'
Assert (@($r.Rows | Where-Object {$_.Check -eq 'PARENT_PATH_NOT_RETURNED'}).Count -eq 0) 'Both valid paths should match.'

$f=Fixture 'missing-input';$f.Data.Remove('goals');$r=Run $f
Assert ($r.Exit -eq 2) 'Missing file must mark incomplete.'
Assert (@($r.Coverage | Where-Object {$_.Status -eq 'NOT_RUN'}).Count -gt 0) 'Dependent checks must be marked NOT_RUN.'

$f=Fixture 'missing-column';$f.Data.plans[0].PSObject.Properties.Remove('txp_created_date');$r=Run $f
Assert ($r.Exit -eq 2) 'Missing ranked-date column must mark incomplete.'
Assert (@($r.Coverage | Where-Object {$_.Check -eq 'MISSING_RANKED_DATES' -and $_.Status -eq 'NOT_RUN'}).Count -eq 1) 'Do not mistake absent column for passed date check.'

$f=Fixture 'wrong-ancestor';$f.Data.objectives[0].problem_entity_id='P-other';$r=Run $f
Assert (@($r.Rows | Where-Object {$_.Check -eq 'PARENT_PATH_NOT_RETURNED' -and $_.File -eq 'objectives.csv'}).Count -eq 1) 'Goal ID alone must not match an objective with a different problem.'

$f=Fixture 'duplicate-base';$f.Data.plans+= $f.Data.plans[0].PSObject.Copy();$r=Run $f
Assert (@($r.Rows | Where-Object {$_.Check -eq 'MULTIPLE_BASE_ROWS'}).Count -eq 2) 'Both duplicate base records must be flagged.'

$f=Fixture 'uppercase-headers'
foreach($n in @($f.Data.Keys)) {
    $f.Data[$n]=@($f.Data[$n] | ForEach-Object { $h=[ordered]@{};foreach($p in $_.PSObject.Properties){$h[$p.Name.ToUpperInvariant()]=$p.Value};[pscustomobject]$h })
}
$r=Run $f;Assert ($r.Exit -eq 0) 'Uppercase Snowflake headers must be accepted.'

# Header-only CSVs must load and be called out as empty, not silently passed.
$emptyInput=Join-Path $testRoot 'empty';$null=New-Item -ItemType Directory -Path $emptyInput
foreach($file in Get-ChildItem -LiteralPath (Join-Path $testRoot 'valid') -Filter '*.csv') {
    Get-Content -LiteralPath $file.FullName -Encoding UTF8 -TotalCount 1 | Set-Content -LiteralPath (Join-Path $emptyInput $file.Name) -Encoding UTF8
}
$emptyOut=Join-Path $testRoot 'empty-reports'
$null=& $PowerShellExe -NoProfile -File $checker -InputFolder $emptyInput -OutputFolder $emptyOut
Assert ($LASTEXITCODE -eq 1) 'Header-only exports should require review.'
$emptyFindings=@(Import-Csv -LiteralPath (Join-Path $emptyOut 'findings.csv'))
Assert (@($emptyFindings | Where-Object {$_.Check -eq 'EMPTY_RESULT'}).Count -eq 6) 'Every empty result needs a visible flag.'

$f=Fixture 'formula-id';$f.Data.plans[0].client_id='=1+1';$r=Run $f
$flag=@($r.Rows | Where-Object {$_.File -eq 'plans.csv'})
# Add a second base row to ensure the leading equals ID is present in a finding.
$f2=Fixture 'formula-id-visible';$f2.Data.plans[0].client_id='=1+1';$f2.Data.plans[0].program_id='9999';$r=Run $f2
Assert (@($r.Rows | Where-Object {$_.ClientId -ceq "'=1+1"}).Count -gt 0) 'Report identifiers must not become spreadsheet formulas.'

$existing=Join-Path $testRoot 'valid-reports'
$before=(Get-FileHash -LiteralPath (Join-Path $existing 'summary.txt')).Hash
$null=& $PowerShellExe -NoProfile -File $checker -InputFolder (Join-Path $testRoot 'valid') -OutputFolder $existing 2>&1
Assert ($LASTEXITCODE -eq 2) 'Existing output directory must be refused with exit 2.'
Assert ((Get-FileHash -LiteralPath (Join-Path $existing 'summary.txt')).Hash -eq $before) 'Previous reports must not be overwritten.'

Write-Output 'PASS: 12 scenarios, including valid relationships, multiline CSV, ID preservation, missing prerequisites, ancestor paths, empty results, formula-safe reports and overwrite refusal.'
Write-Output ('Synthetic test files and reports: '+$testRoot)
