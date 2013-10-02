$scriptsDir = Split-Path -parent $MyInvocation.MyCommand.Definition
$root = Split-Path -Parent $scriptsDir

$path = "C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE"
& $path\devenv $root\SelfServiceDesktops.sln /build