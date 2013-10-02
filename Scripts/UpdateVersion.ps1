<#
    
Copyright © 2013 Citrix Systems, Inc. All rights reserved.

.SYNOPSIS
This script will write a version number into a Wix Product.wxs file.

.DESCRIPTION


.PARAMETER Version
An optional version number (default is year.month.day e.g. 13.05.15) 
.PARAMETER File
File name for transform

.EXAMPLE
.\UpdateVersion.ps1 -File Product.wxs -Version 13.10.01

.NOTES
    KEYWORDS: PowerShell
    REQUIRES: PowerShell Version 2.0 

.LINK
     http://community.citrix.com/
#>
Param (
    [Parameter(Mandatory=$true)]
    [string]$File,
    [string]$Version
 
)

$this = $MyInvocation.MyCommand.Definition
$dir = Split-Path -parent $this

if ([string]::IsNullOrEmpty($Version)) {
    $version = Get-Date -Format "yy.MM.dd"
} 
if (!(Test-Path $File)) {
    Write-Error "Input file $File does not exist"
    exit 1
}

$xslt = "$dir\Release.xslt"
if (!(Test-Path $xslt)) {
    Write-Error "Xslt transform $xslt does not exist"
    exit 1
}

try {
    $processor = New-Object System.Xml.Xsl.XslCompiledTransform
    $processor.Load($xslt)
    
    $arguments = New-Object  System.Xml.Xsl.XsltArgumentList
    $arguments.AddParam("version", "", "$Version")   
    $output = [System.IO.Path]::GetTempFileName()
    $writer = [System.Xml.XmlWriter]::Create($output)
  
    $processor.Transform($File, $arguments, $writer)
} 
finally {
    if ($writer) {
        $writer.Close()
    }
    $old = "$File.old"
    if (Test-Path "$old") {
        Remove-Item $old
    }
    Move-Item $File $old
    Copy-Item $output $File
    Remove-Item -Path $output
}