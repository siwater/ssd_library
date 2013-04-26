Import-Module ActiveDirectory

$username = 'FORMENTERA\Administrator’
$password = '1pass@word1'
$cred = New-Object System.Management.Automation.PSCredential -ArgumentList @($username,(ConvertTo-SecureString -String $password -AsPlainText -Force))

$ErrorActionPreference = "SilentlyContinue"
$DHCPServers = Get-WmiObject Win32_NetworkAdapterConfiguration -Filter "DHCPEnabled=TRUE AND DHCPServer IS NOT NULL" -Property DHCPServer
ForEach ($DHCPServer in $DHCPServers.DHCPServer) {
$URL = "http://$DHCPServer/latest/meta-data/local-hostname"
$metadatarequest=[System.Net.WebRequest]::Create($URL)
$resp=$metadatarequest.GetResponse()
$reqstream=$resp.GetResponseStream()
$sr=new-object System.IO.StreamReader $reqstream
$newhostname=$sr.ReadToEnd()
Write-Host $newhostname

$compaccount = Get-ADcomputer $newhostname -Credential $cred
Write-Host $compaccount
if ($compaccount)
    {
    Write-Host "Computeraccount exists"
    Remove-ADComputer -identity $newhostname -Credential $cred -confirm:$false
    }
    else
    {
    Write-Host "Computeraccount does not exist"
    }

Get-WmiObject Win32_ComputerSystem
If ($newhostname.Length -gt 0) {$(Get-WmiObject Win32_ComputerSystem).Rename($newhostname,$password,$username)}
}
