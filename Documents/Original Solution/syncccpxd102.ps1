# (c)opyright Citrix Systems GmbH
# Author: Christian Ferber
# Date: 12.03.2013
# Version 1.0.2
#------------------START MODULES-------------------------------

Import-Module ActiveDirectory
Add-pssnapin citrix.*

#------------------END MODULES-------------------------------

#------------------BEGIN GLOBAL VARIABLES-------------------------------
$ErrorActionPreference = "SilentlyContinue" 
$catalogname = 'Developer VMs'
$ccpip = '172.16.10.24'
$domain = 'space'
$templatestart = 'DEV'

#------------------END GLOBAL VARIABLES-------------------------------

#------------------START FUNCTIONS-------------------------------

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
	if ($virtualmachine.templatedisplaytext.StartsWith($templatestart))
	{
		$machine = $domain + '\' + $virtualmachine.name
		$ccpvmarray += ,($machine.ToLower(),$virtualmachine.name.ToLower(),$virtualmachine.account.ToLower(),$virtualmachine.templatename)
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

Write-Host "CloudPlatform VMs:"
foreach ($vm in $ccpvmarray) {Write-Host $vm}
Write-Host "XenDesktop VMs:"
foreach ($vm in $xdvmarray){Write-Host $vm}

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

Write-Host "CloudPlatform VMs not in XenDesktop:"
foreach ($vm in $ccpvmsnotinxd)
{Write-Host $vm}

Write-Host "XenDesktop VMs not in CloudPlatform:"
foreach ($vm in $xdvmsnotinccp)
{Write-Host $vm}

# Create new XenDesktop assignments

foreach ($vm in $ccpvmsnotinxd)
{
	$computer = $vm[0].split("\")[1]
	$adcomputerexists="false"
	$adcomputer=get-adcomputer -identity $computer
	$adcomputerexists=$adcomputer.enabled
	if ($adcomputerexists -eq "True")
		{
		Write-Host "AD-Account",$computer,"exists."
		$catalog = (Get-BrokerCatalog -Name $catalogname)
		$newmachine=New-BrokerMachine -MachineName $vm[0] -CatalogUid $catalog.uid 
		if ($newmachine.machinename.ToLower() -eq $vm[0])
			{
			$machinenameincldol = $vm[0] + '$'
			$userincldomain = $domain + '\' + $vm[2]
			Start-Sleep -s 15
			Add-BrokerUser -Name $userincldomain -Machine $vm[0]
			Write-Host $vm[3]
			$desktopgroupname = $vm[3]
			$desktopgroup = Get-BrokerDesktopGroup -Filter {(Name -eq $desktopgroupname)}
			if ($desktopgroup) {Write-Host "Desktop group exists"}
				else
				{
				Write-Host "Desktop group does not exist"
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
		Write-Host "AD-Account ",$computer," does not exist or is disabled."
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
	if ($desktopgroup) {Write-Host "Desktop group still has desktops"}
		else
		{
		Write-Host "Removing Desktop group " $desktopgroupname
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
	Write-Host $vmmachine "SID mismatch --> Deleting VM XD registration"
	$desktopgroupname = $vmproperties.DesktopGroupName
	Remove-BrokerMachine -MachineName $vm -Force -DesktopGroup $desktopgroupname
	Remove-BrokerMachine -MachineName $vm -Force
	}

}

}
#------------------END FUNCTIONS-------------------------------

#------------------START MAIN-------------------------------

while ($true)
{

# Create Catalog in XenDesktop
$catalog = Get-BrokerCatalog -Filter {(Name -eq $catalogname)}
if ($catalog) {Write-Host "Catalog exists"}
	else
	{
	Write-Host "Catalog does not exist"
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
	Write-Host "CloudPlatform Server not reachable. Exiting."
}

Write-Host
Start-Sleep -s 30

}

#------------------END MAIN-------------------------------
