param (
    [string]$NewProjectName = "NewProjectTemplate",
    [string]$OriginalProjectName = "NewProjectTemplate"
)

[string]$SolutionFolder = [System.IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Path);

Get-ChildItem -recurse $SolutionFolder -include *.cs,*.csproj,*.sln,*.config,*.ps1 | where { $_ -is [System.IO.FileInfo] } | where { !$_.FullName.Contains("\packages\") } | where { !$_.FullName.Contains("\obj\") } | where { !$_.Name.Equals("_SetApplicationName.ps1") } |
Foreach-Object {
    Set-ItemProperty $_.FullName -name IsReadOnly -value $false
    [string]$Content = [IO.File]::ReadAllText($_.FullName)
    $Content = $Content.Replace($OriginalProjectName, $NewProjectName)
    [IO.File]::WriteAllText($_.FullName, $Content)
}

Rename-Item -path ([System.IO.Path]::Combine($SolutionFolder, $OriginalProjectName + '.sln')) -newName ($NewProjectName + '.sln')
Rename-Item -path ([System.IO.Path]::Combine($SolutionFolder, 'Entity\NewProjectTemplateDbConfiguration.cs')) -newName ($NewProjectName + 'DbConfiguration.cs')
Rename-Item -path ([System.IO.Path]::Combine($SolutionFolder, 'Entity\NewProjectTemplateDbContext.cs')) -newName ($NewProjectName + 'DbContext.cs')
Rename-Item -path ([System.IO.Path]::Combine($SolutionFolder, 'Entity\NewProjectTemplateDbContextFactory.cs')) -newName ($NewProjectName + 'DbContextFactory.cs')
