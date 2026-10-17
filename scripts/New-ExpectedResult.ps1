<#
.SYNOPSIS
	Creates the expected result (.e.cs) for a code generator test file.

.DESCRIPTION
	1. Creates an empty expected result file next to the test file.
	2. Runs the test for that test file.
	3. Copies the generated file (.g.cs) from the build output as the expected file.
	4. Replaces version, generated time and generated ID with their template values.

.PARAMETER FileName
	Path of the test file, relative to the TestFiles folder of the unit test project
	(e.g. 'Sync\TransitionAfterAttribute\SingleAttribute.cs'). A path relative to the
	current directory or an absolute path is accepted as well.

.PARAMETER Framework
	Target framework to run the test on. Default: net10.0.

.PARAMETER Configuration
	Build configuration. Default: Debug.

.EXAMPLE
	.\scripts\New-ExpectedResult.ps1 Sync\OnEntryAttribute\SingleAttribute.cs
#>
[CmdletBinding()]
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string] $FileName,

	[string] $Framework = 'net10.0',

	[string] $Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$projectDir = Join-Path $root 'src\Twia.StateMachine.CodeGenerator.UnitTests'
$testFilesDir = Join-Path $projectDir 'TestFiles'

# Resolve the test file.
$testFile = $null
foreach ($candidate in @((Join-Path $testFilesDir $FileName), $FileName)) {
	if (Test-Path -LiteralPath $candidate -PathType Leaf) {
		$testFile = (Resolve-Path -LiteralPath $candidate).Path
		break
	}
}
if (-not $testFile) {
	throw "Test file '$FileName' not found."
}
if ($testFile -match '\.(e|g)\.cs$' -or $testFile -notlike '*.cs') {
	throw "'$FileName' is not a test source file (expected name.cs)."
}

$relative = [IO.Path]::GetRelativePath($testFilesDir, $testFile)
if ($relative.StartsWith('..')) {
	throw "Test file must be located under '$testFilesDir'."
}

# Determine the display name of the test, as used by the test runner.
$nameLine = Get-Content -LiteralPath $testFile | Where-Object { $_ -match '^Name:\s*(.+?)\s*$' } | Select-Object -First 1
if (-not $nameLine) {
	throw "Test file '$relative' has no 'Name:' in its frontmatter."
}
$name = ($nameLine -replace '^Name:\s*', '').Trim().Trim("'", '"')
$subDir = (Split-Path -Parent $relative) -replace '[\\/]', ' - '
$displayName = if ($subDir) { "${subDir}: $name" } else { $name }

# Escape characters that have a special meaning in a test filter.
$filterValue = [regex]::Replace($displayName, '([\\()&|=!~])', '\$1')

$expectedFile = [IO.Path]::ChangeExtension($testFile, '.e.cs')
$generatedFile = Join-Path $projectDir "bin\$Configuration\$Framework\TestFiles\$([IO.Path]::ChangeExtension($relative, '.g.cs'))"

# 1. Create an empty expected result, so the test runs up to the comparison.
Set-Content -LiteralPath $expectedFile -Value '' -NoNewline
if (Test-Path -LiteralPath $generatedFile) {
	Remove-Item -LiteralPath $generatedFile
}

# 2. Run the test. It is expected to fail, as the expected result is empty.
Write-Host "Running test '$displayName' on $Framework..."
Push-Location $projectDir
try {
	& dotnet test -c $Configuration -f $Framework --filter "Name~$filterValue" | Out-Host
}
finally {
	Pop-Location
}

# 3. Copy the generated file as the expected file.
if (-not (Test-Path -LiteralPath $generatedFile)) {
	Remove-Item -LiteralPath $expectedFile -ErrorAction SilentlyContinue
	throw "Generated file '$generatedFile' was not found. Check the test output above."
}

$text = [IO.File]::ReadAllText($generatedFile)

# 4. Replace version, time and generated ID with template values.
$text = [regex]::Replace($text, 'version \S+ at \d{4}-\d\d-\d\d \d\d:\d\d:\d\d UTC', 'version {{Version}} at {{Time}} UTC')
$text = [regex]::Replace($text, '("Twia\.StateMachine\.CodeGenerator", ")[^"]+"', '$1{{Version}}"')
$text = [regex]::Replace($text, 'UnitTestStateMachine_[0-9a-f]{8}', '{{GeneratedId}}')

[IO.File]::WriteAllText($expectedFile, $text)
Write-Host "Created '$expectedFile'."

# 5. Verify that the test succeeds with the new expected result.
Write-Host "Verifying test '$displayName' on $Framework..."
Push-Location $projectDir
try {
    & dotnet test -c $Configuration -f $Framework --filter "Name~$filterValue" | Out-Host
    $exitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

if ($exitCode -ne 0) {
    throw "The test '$displayName' still fails with the new expected result '$expectedFile'. Check the test output above."
}

Write-Host 'Test succeeded. Review the file before committing.'
