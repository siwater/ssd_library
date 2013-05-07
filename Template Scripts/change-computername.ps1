$username = 'DOMAIN\Administrator’
$password = 'password'

$ErrorActionPreference = "SilentlyContinue"
$DHCPServers = Get-WmiObject Win32_NetworkAdapterConfiguration -Filter "DHCPEnabled=TRUE AND DHCPServer IS NOT NULL" -Property DHCPServer
ForEach ($DHCPServer in $DHCPServers.DHCPServer) {
    $URL = "http://$DHCPServer/latest/meta-data/local-hostname"
    $metadatarequest=[System.Net.WebRequest]::Create($URL)
    $resp=$metadatarequest.GetResponse()
    $reqstream=$resp.GetResponseStream()
    $sr=new-object System.IO.StreamReader $reqstream
    $newhostname=$sr.ReadToEnd()
    Write-Output "Computer will be renamed $newhostname"
    Get-WmiObject Win32_ComputerSystem
    If ($newhostname.Length -gt 0) {
        $(Get-WmiObject Win32_ComputerSystem).Rename($newhostname, $password,$username)
    } else {
        Write-Output "Unable to determine new computer name"
    }
}
