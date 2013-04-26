<#
    
Copyright © 2013 Citrix Systems, Inc. All rights reserved.

.SYNOPSIS

.DESCRIPTION
This script will synchronise XenDesktop and Cloud Platform

.NOTES
    KEYWORDS: PowerShell, Apache CloudStack, Citrx CloudPlatform, XenDesktop
    REQUIRES: PowerShell Version 2.0 

.LINK
     http://community.citrix.com/
#>
Param 
(  
    [string]$domain = [Environment]::UserDomainName,
      
    [string]$catalogname = 'Developer VMs',
      
    [Parameter(Mandatory=$true)]
    [string]$ccpip,
    
    [Parameter(Mandatory=$true)]
    [string]$hostnameprefix
)

#------------------START FUNCTIONS-------------------------------

# Horrible hack as the PowerShell host invoked from C# does not support Write-Debug (!)
function WriteDebug($message) 
{
	Write-Output "[Debug]$message"
}

function isDesktop($virtualmachine) 
{
    # A virtual machaine is considered to be a desktop if its display name matches the given pattern
    return $virtualmachine.displayname -match "$hostnameprefix\d+"
}

function syncXenDesktop()
{

# Load VMs in CloudPlatform

$ccpvms = New-Object System.Xml.XmlDocument
$ccpvmlisturl = 'http://'+$ccpip+':8096/client/api?command=listVirtualMachines&listall=true'
$ccpvms.Load($ccpvmlisturl)

$ccpvmarray = @()
$ccpvmarraymachine =@()
$xdvmarray = @()

# $ccpvmarray 0=Name incl. domain 1=Name 2=User 3=Template Description
# Load VMs from CloudPlatform

foreach ($virtualmachine in $ccpvms.listvirtualmachinesresponse.virtualmachine)
{
	if (isDesktop $virtualmachine)
	{
		$machine = $domain + '\' + $virtualmachine.name
		$ccpvmarray += ,($machine.ToLower(),$virtualmachine.name.ToLower(),$virtualmachine.account.ToLower(),
        $virtualmachine.templatename)
		$ccpvmarraymachine += ,($machine.ToLower())
	}
}

# Load VMs from XenDesktop

$xdvms = Get-BrokerMachine -CatalogName $catalogname

if ($xdvms)
{
	foreach ($virtualmachine in $xdvms)
	{
		$xdvmarray += $virtualmachine.MachineName.ToLower()
	}
}
		
# Compare CloudPlatform and XenDesktop VMs

$ccpvmsnotinxd =@()
$xdvmsnotinccp =@()

WriteDebug "CloudPlatform VMs:"
foreach ($vm in $ccpvmarray) {WriteDebug "$vm"}

WriteDebug "XenDesktop VMs:"
foreach ($vm in $xdvmarray){WriteDebug "$vm"}

Foreach ($vm in $ccpvmarray)
{
	If ($xdvmarray -notcontains $vm[0])
	{
		$ccpvmsnotinxd += ,($vm)
	}	
}

Foreach ($vm in $xdvmarray)
{
	If ($ccpvmarraymachine -notcontains $vm)
	{
		$xdvmsnotinccp += ,($vm)
	}
}

WriteDebug "CloudPlatform VMs not in XenDesktop:"
foreach ($vm in $ccpvmsnotinxd) 
{WriteDebug "$vm"}

WriteDebug "XenDesktop VMs not in CloudPlatform:"
foreach ($vm in $xdvmsnotinccp) 
{WriteDebug "$vm"}

# Create new XenDesktop assignments

foreach ($vm in $ccpvmsnotinxd)
{
	$computer = $vm[0].split("\")[1]
	$adcomputer = $null
	$adcomputer=Get-ADComputer -identity $computer -ErrorAction SilentlyContinue
	if (($adcomputer -ne $null) -and ($adcomputer.enabled -eq "True"))
		{
		WriteDebug "AD account $computer exists."
		$catalog = Get-BrokerCatalog -Filter {(Name -eq $catalogname)}  
		$newmachine=New-BrokerMachine -MachineName $vm[0] -CatalogUid $catalog.uid 
		if ($newmachine.machinename.ToLower() -eq $vm[0])
			{
			$machinenameincldol = $vm[0] + '$'
			$userincldomain = $domain + '\' + $vm[2]
			Start-Sleep -s 15
			Add-BrokerUser -Name $userincldomain -Machine $vm[0]
			
			$desktopgroupname = $vm[3]
			$desktopgroup = Get-BrokerDesktopGroup -Filter {(Name -eq $desktopgroupname)}
			if ($desktopgroup) {WriteDebug "Desktop group $desktopgroupname exists"}
				else
				{
				WriteDebug "Desktop group  $desktopgroupname does not exist"
				New-BrokerDesktopGroup -Name $vm[3] -DesktopKind private -ShutdownDesktopsAfterUse $False -TimeZone 'W. Europe Standard Time'
				$desktopgroupnamedirect = $desktopgroupname + '_Direct'
				$desktopgroupnameag = $desktopgroupname + '_AG'
				New-BrokerAccessPolicyRule -AllowedConnections 'NotViaAG' -AllowedProtocols @('RDP','HDX') -AllowedUsers 'AnyAuthenticated' -AllowRestart $True -Enabled $True -IncludedDesktopGroupFilterEnabled $True -IncludedDesktopGroups @($desktopgroupname) -IncludedSmartAccessFilterEnabled $True -IncludedUserFilterEnabled $True -Name $desktopgroupnamedirect
				New-BrokerAccessPolicyRule -AllowedConnections 'ViaAG' -AllowedProtocols @('RDP','HDX') -AllowedUsers 'AnyAuthenticated' -AllowRestart $True -Enabled $True -IncludedDesktopGroupFilterEnabled $True -IncludedDesktopGroups @($desktopgroupname) -IncludedSmartAccessFilterEnabled $True -IncludedSmartAccessTags @() -IncludedUserFilterEnabled $True -Name $desktopgroupnameag 
				}
			Add-BrokerMachine -MachineName $vm[0] -DesktopGroup $vm[3]
			}
			else
			{
			Remove-BrokerMachine -MachineName $newmachine.machinename -Force
			}
		}
		else
		{
		WriteDebug "AD account $computer does not exist or is disabled."
		}
}

# Delete old XenDesktop assignments

foreach ($vm in $xdvmsnotinccp)
{
	$vmproperties = Get-BrokerDesktop -MachineName $vm
	$desktopgroupname = $vmproperties.DesktopGroupName
	Remove-BrokerMachine -MachineName $vm -Force -DesktopGroup $desktopgroupname
	Remove-BrokerMachine -MachineName $vm -Force
	$computer = $vm.split("\")[1]
	Remove-ADComputer $computer -confirm:$false
	$desktopgroup = Get-BrokerDesktop -Filter {(DesktopGroupName -eq $desktopgroupname)}
	if ($desktopgroup) {WriteDebug "Desktop group $desktopgroupname still has desktops"}
		else
		{
		WriteDebug "Removing Desktop group $desktopgroupname"
		Remove-BrokerDesktopGroup -Name $desktopgroupname
		$desktopgroupnamedirect = $desktopgroupname + '_Direct'
		$desktopgroupnameag = $desktopgroupname + '_AG'
		Remove-BrokerAccessPolicyRule -Name $desktopgroupnamedirect
		Remove-BrokerAccessPolicyRule -Name $desktopgroupnameag
		}
}

# Delete re-deployed desktops

foreach ($vm in $xdvmarray)
{
$vmmachine = $vm.split("\")[1]
$vmproperties = Get-BrokerDesktop -MachineName $vm
$vmxdsid = $vmproperties.SID
$adproperties = Get-ADComputer -identity $vmmachine
$vmadsid = $adproperties.SID

if ($vmxdsid -ne $vmadsid)
	{
	WriteDebug "$vmmachine SID mismatch --> Deleting VM XD registration"
	$desktopgroupname = $vmproperties.DesktopGroupName
	Remove-BrokerMachine -MachineName $vm -Force -DesktopGroup $desktopgroupname
	Remove-BrokerMachine -MachineName $vm -Force
	}
}

}
#------------------END FUNCTIONS-------------------------------

#------------------START MAIN-------------------------------

Import-Module ActiveDirectory
if ((Get-PSSnapin -Name citrix.* -ErrorAction SilentlyContinue) -eq $null) {
    Add-pssnapin citrix.*
}

# Create Catalog in XenDesktop
$catalog = Get-BrokerCatalog -Filter {(Name -eq $catalogname)} -ErrorAction SilentlyContinue
if ($catalog) {WriteDebug "Catalog $catalogname exists"}
	else
	{
	WriteDebug "Catalog $catalogname does not exist (creating it)"
	New-BrokerCatalog -Name $catalogname -CatalogKind unmanaged -AllocationType permanent
	}

$ccpserver=New-Object System.Net.Sockets.TCPClient
$ccpserver.Connect("$ccpip",8096)
if ($ccpserver.connected -eq "True")
{
	$ccpserver.Close()
	# Execution of main function

	syncXenDesktop
}
else
{
	Write-Error "CloudPlatform Server port 8096 not reachable. Exiting."
}

#------------------END MAIN-------------------------------
