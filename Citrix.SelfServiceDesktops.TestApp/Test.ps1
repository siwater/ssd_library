
param (
  [string]$catalogname = 'Developer VMs',
  [string]$ccpip = '172.16.10.24',
  [string]$domain = 'space',
  [string]$templatestart = 'DEV'
 )
"Hello world this is a script"
"catalog name: $catalogname"
Write-Output "CPP host: $ccpip"
$ccpvms = New-Object System.Xml.XmlDocument
$ccpvmlisturl = 'http://'+$ccpip+':8096/client/api?command=listVirtualMachines&listall=true'
$ccpvms.Load($ccpvmlisturl)

$ccpvms
return "Bye!"