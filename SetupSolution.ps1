﻿param (
    [string]$NewSolutionName = "VolejbalApp",
	[string]$NewSolutionCode = "999.XXX",
	[string]$NewWebProjectPort = "9900",
	[string]$NewWebAPIProjectCode = "9901",

    [string]$OriginalSolutionName = "VolejbalApp",
    [string]$OriginalSolutionCode = "999.XXX",
	[string]$OriginalWebProjectPort = "9900",
	[string]$OriginalWebAPIProjectCode = "9901"
)

[string]$SolutionFolder = [System.IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Path);

Get-ChildItem -recurse $SolutionFolder -include *.cs,*.csproj,*.config,*.ps1,*.json,*.tsx,*.cshtml,*.props | where { $_ -is [System.IO.FileInfo] } | where { !$_.FullName.Contains("\packages\") } | where { !$_.FullName.Contains("\obj\") } | where { !$_.FullName.Contains("package.json") } | where { !$_.Name.Equals("_SetApplicationName.ps1") } |
Foreach-Object {
    Set-ItemProperty $_.FullName -name IsReadOnly -value $false
    [string]$Content = [IO.File]::ReadAllText($_.FullName)
    $Content = $Content.Replace($OriginalSolutionName, $NewSolutionName)
    $Content = $Content.Replace($OriginalSolutionCode, $NewSolutionCode)
    $Content = $Content.Replace($OriginalWebProjectPort, $NewWebProjectPort)
    $Content = $Content.Replace($OriginalWebAPIProjectCode, $NewWebAPIProjectCode)
    [IO.File]::WriteAllText($_.FullName, $Content, [System.Text.Encoding]::UTF8)
}

Rename-Item -path ([System.IO.Path]::Combine($SolutionFolder, $OriginalSolutionName + '.sln')) -newName ($NewSolutionName + '.sln')
Rename-Item -path ([System.IO.Path]::Combine($SolutionFolder, 'Entity\VolejbalAppDbContext.cs')) -newName ($NewSolutionName + 'DbContext.cs')
Rename-Item -path ([System.IO.Path]::Combine($SolutionFolder, 'Entity\VolejbalAppDesignTimeDbContextFactory.cs')) -newName ($NewSolutionName + 'DesignTimeDbContextFactory.cs')
Rename-Item -path ([System.IO.Path]::Combine($SolutionFolder, 'Tests\Entity\VolejbalAppDbContextTests.cs')) -newName ($NewSolutionName + 'DbContextTests.cs')
