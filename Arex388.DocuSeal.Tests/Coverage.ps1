#	Generates an HTML code coverage report for Arex388.DocuSeal.Tests and drops a
#	Coverage.lnk shortcut to it. Run from this directory.
#
#	Requires the local tools from ../.config/dotnet-tools.json ('dotnet tool restore').
#	The run is offline only: DOCUSEAL_LIVE_TESTS is cleared for this process, so the
#	live integration tests are always skipped and no user secrets are needed.
#	Exits non-zero on the first failing step.

$env:DOCUSEAL_LIVE_TESTS = $null;

Remove-Item -Path 'reports' -Recurse -ErrorAction SilentlyContinue;
Remove-Item -Path 'TestResults' -Recurse -ErrorAction SilentlyContinue;
Remove-Item -Path 'Coverage.lnk' -ErrorAction SilentlyContinue;

Write-Host 'Building';

dotnet build > $null;

if ($LASTEXITCODE -ne 0) {
	Write-Error 'Build failed.';

	exit 1;
}

Write-Host 'Collecting';

#	xunit.v3 4.x registers the project as a Microsoft.Testing.Platform application,
#	which the .NET 10 SDK refuses to drive through the VSTest 'dotnet test' path
#	that coverlet.collector hooks. Opt this one invocation back into VSTest.
dotnet test --no-build -p:IsTestingPlatformApplication=false --collect:'XPlat Code Coverage' > $null;

if ($LASTEXITCODE -ne 0) {
	Write-Error 'Tests failed.';

	exit 1;
}

$coberturaPath = Get-ChildItem -Path 'TestResults' -Recurse -Filter 'coverage.cobertura.xml' | Select-Object -First 1;

if ($null -eq $coberturaPath) {
	Write-Error 'No coverage.cobertura.xml was produced by the test run.';

	exit 1;
}

Write-Host 'Reporting';

dotnet reportgenerator -reports:$coberturaPath.FullName -targetdir:'reports' -reporttypes:'Html' > $null;

if ($LASTEXITCODE -ne 0) {
	Write-Error 'Report generation failed. Run ''dotnet tool restore'' from the repository root.';

	exit 1;
}

$indexPath = Get-ChildItem -Path 'reports' -Recurse -Filter 'index.html' | Select-Object -First 1;

if ($null -eq $indexPath) {
	Write-Error 'No report was generated.';

	exit 1;
}

Write-Host 'Shortcutting';

$wscriptShell = New-Object -ComObject WScript.Shell;
$shortcut = $wscriptShell.CreateShortcut("$pwd\Coverage.lnk");
$shortcut.TargetPath = $indexPath.FullName;
$shortcut.Save();

Remove-Item -Path 'TestResults' -Recurse -ErrorAction SilentlyContinue;
