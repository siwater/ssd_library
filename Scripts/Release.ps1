<#
    
Copyright © 2013 Citrix Systems, Inc. All rights reserved.

.SYNOPSIS
This script will build a release ZIP of the Self Service Desktops
solution

.DESCRIPTION
The MSI files containing the installers of the Agent and Web App plus the 
release documentation are copied to a named folder within Releases folder
of the the project. A Zip file is then constructed containing the release, and
optionally the named release folder is deleted

.PARAMETER name
An optional name for the release (e.g. SelfServiceDesktops-dd-mm-yyyy).
If not specified the default SelfServiceDesktops-dd-mm-yyyy will be used for the current date.

.PARAMETER clean
Switch to indicate the named release folder can be deleted once the Zip has been created

.EXAMPLE
.\Release.ps1 -Name selfServiceDesktops-01-01-1901

.NOTES
    KEYWORDS: PowerShell
    REQUIRES: PowerShell Version 2.0 

.LINK
     http://community.citrix.com/
#>
Param 
(
    [string]$name,    
    [switch]$clean
)

$scriptsDir = Split-Path -parent $MyInvocation.MyCommand.Definition
$root = Split-Path -Parent $scriptsDir

if ([string]::IsNullOrEmpty($name)) {
    $d = Get-Date -Format dd-MM-yyyy
    $name = "SelfServiceDesktops-$d"
}
$releaseDir = "$root\Releases\$name"
$agentMSI = "$root\Citrix.SelfServiceDesktops.Agent.Setup\bin\Debug\Citrix.SelfServiceDesktops.Agent.Setup.msi"
$webAppMSI = "$root\Citrix.SelfServiceDesktops.WebApp.Setup\bin\Debug\en-us\Citrix.SelfServiceDesktops.WebApp.Setup.msi"
$docs = "$root\Documents\Release Documentation"
$zipfile = "$root\Releases\$name" + ".zip"

if (Test-Path $releaseDir) {
    Write-Error "$releaseDir already exists"
} elseif (Test-Path $zipfile) {
    Write-Error "Release zip $zipfile already exists"
} elseif (!(Test-Path $agentMSI)) {
    Write-Error "$agentMSI does not exist"
} elseif (!(Test-Path $agentMSI)) {
    Write-Error "$webappMSI does not exist"
} elseif (!(Test-Path $Docs)) {
    Write-Error "$docs folder does not exist"
} else {

    Write-Host "Creating $releaseDir"
    $dir = New-Item -ItemType directory $releaseDir
    Copy-Item $agentMSI $releaseDir
    Copy-Item $webAppMSI $releaseDir
    $dir = New-Item -ItemType directory "$releaseDir\Documentation"
    Copy-Item -Recurse "$docs\*" "$releaseDir\Documentation"

    Set-Content $zipfile ("PK" + [char]5 + [char]6 + ("$([char]0)" * 18))
    $zip = Get-Item -Path $zipfile
    $shellApp = New-Object -com shell.application
    $zipPackage = $shellApp.NameSpace($zip.FullName)
    
    $files = Get-ChildItem -Path $releaseDir
    foreach ($file in $files) { 
        $zipPackage.CopyHere($file.FullName) 
        while($zipPackage.Items().Item($file.name) -eq $null){
            Start-Sleep -Milliseconds 100
        }
    } 
    if ($clean) {
        Remove-Item -Path $releaseDir -Recurse -Force
    }
    Write-Host "New release created at: $zipfile"
}

