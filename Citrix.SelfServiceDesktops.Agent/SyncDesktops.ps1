<#
    
Copyright © 2013 Citrix Systems, Inc. All rights reserved.

.SYNOPSIS
This script will synchronise desktops in XenDesktop and Cloud Platform.

.DESCRIPTION
 Virtual machines that exist in CloudPlatform with names that match a specified pattern and have Active Directory accounts 
 of the same name will be registered in XenDesktop. Conversely, desktops registered in XenDesktop that match the pattern 
 but do not have a corresponding CloudPlatform virtual machine will be de-registered.

.NOTES
    KEYWORDS: PowerShell, Apache CloudStack, Citrx CloudPlatform, XenDesktop
    REQUIRES: PowerShell Version 2.0 

.LINK
     http://community.citrix.com/
#>
Param 
(  
    [string]$domain = [Environment]::UserDomainName,
      
    [string]$catalogname = 'Self Service Desktops',
      
    [Parameter(Mandatory=$true)]
    [string]$ccpip,
    
    [Parameter(Mandatory=$true)]
    [string]$hostnameprefix
)

#------------------START FUNCTIONS-------------------------------

# Horrible hack to export debug messages to the enclosing windows service. This is required as the PowerShell host 
# invoked from .NET runtime does not support Write-Debug (!)
function WriteDebug($message) 
{
	Write-Output "[Debug]$message"
}

#
# Desktop Names are a prefix followed by one or more digits. Match is case-insensitive
#
function isDesktopName($computerName)
{
	$prefix = $hostnameprefix.ToLower()
    return ($computername.ToLower() -match "$prefix\d+") 
}

#
# A CloudPlatform virtual machine is considered to be a desktop if its display name matches the 
# given pattern and it is not destroyed or expunging.
#
function isCloudPlatformDesktop($virtualmachine) 
{
    # 
    return (isDesktopName $virtualmachine.displayname) -and
           ($virtualmachine.state -ne "Destroyed") -and 
           ($virtualmachine.state -ne "Expunging")  
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
	if (isCloudPlatformDesktop $virtualmachine)
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
        $computername = $virtualmachine.MachineName.split("\")[1]
        if (isDesktopName $computername)
        {
            $xdvmarray += $virtualmachine.MachineName.ToLower()
        }
	}
}
		
# Compare CloudPlatform and XenDesktop VMs

$ccpvmsnotinxd =@()
$xdvmsnotinccp =@()

#WriteDebug "CloudPlatform VMs:"
#foreach ($vm in $ccpvmarray) 
#{ 
#     WriteDebug "$vm"
#}

#WriteDebug "XenDesktop VMs:"
#foreach ($vm in $xdvmarray) 
#{ 
#     WriteDebug "$vm" 
#}

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

if ($ccpvmsnotinxd.length -gt 0)
{
#    WriteDebug "CloudPlatform VMs not in XenDesktop:"
#    foreach ($vm in $ccpvmsnotinxd) 
#    {
#        WriteDebug $vm
#    }
}
if ($xdvmsnotinccp.length -gt 0)
{
#    WriteDebug "XenDesktop VMs not in CloudPlatform:"
#    foreach ($vm in $xdvmsnotinccp) 
#    {
#        WriteDebug $vm
#    }
}

# Create new XenDesktop assignments

foreach ($vm in $ccpvmsnotinxd)
{
	$computer = $vm[0].split("\")[1]
	$adcomputer = $null
	$adcomputer=Get-ADComputer -identity $computer -ErrorAction SilentlyContinue
	if (($adcomputer -ne $null) -and ($adcomputer.enabled -eq "True"))
	{
		WriteDebug "Registering new CloudPlatform desktop $computer in XenDesktop"
		$catalog = Get-BrokerCatalog -Filter {(Name -eq $catalogname)}  
		$newmachine=New-BrokerMachine -MachineName $adcomputer.SID -CatalogUid $catalog.uid 
		if ($newmachine.machinename.ToLower() -eq $vm[0])
		{
			$userincldomain = $domain + '\' + $vm[2]
			Start-Sleep -s 15
			Add-BrokerUser -Name $userincldomain -Machine $vm[0]
			
			#$desktopgroupname = $vm[2] + "_desktops"
            $desktopgroupname = $hostnameprefix
			$desktopgroup = Get-BrokerDesktopGroup -Filter {(Name -eq $desktopgroupname)}
			if ($desktopgroup) 
            { 
                #WriteDebug "Desktop group $desktopgroupname exists"
            } 
			else
			{
				WriteDebug "Creating Desktop group $desktopgroupname"
				$group = New-BrokerDesktopGroup -Name $desktopgroupname -DesktopKind private -ShutdownDesktopsAfterUse $False
				$desktopgroupnamedirect = $desktopgroupname + '_Direct'
				$desktopgroupnameag = $desktopgroupname + '_AG'
				$rule = New-BrokerAccessPolicyRule -AllowedConnections 'NotViaAG' -AllowedProtocols @('RDP','HDX') -AllowedUsers 'AnyAuthenticated' -AllowRestart $True -Enabled $True -IncludedDesktopGroupFilterEnabled $True -IncludedDesktopGroups @($desktopgroupname) -IncludedSmartAccessFilterEnabled $True -IncludedUserFilterEnabled $True -Name $desktopgroupnamedirect
				$rule = New-BrokerAccessPolicyRule -AllowedConnections 'ViaAG' -AllowedProtocols @('RDP','HDX') -AllowedUsers 'AnyAuthenticated' -AllowRestart $True -Enabled $True -IncludedDesktopGroupFilterEnabled $True -IncludedDesktopGroups @($desktopgroupname) -IncludedSmartAccessFilterEnabled $True -IncludedSmartAccessTags @() -IncludedUserFilterEnabled $True -Name $desktopgroupnameag 
			}
            WriteDebug "Adding new machine to desktop group $desktopgroupname"
			Add-BrokerMachine -MachineName $vm[0] -DesktopGroup $desktopgroupname
		}
		else
		{      
            WriteDebug ("Removing " + $newmachine.machinename + " from XenDesktop")
			Remove-BrokerMachine -MachineName $newmachine.machinename -Force
            WriteDebug "Forcing broker cache refresh after name mis-match"
            Update-BrokerNameCache -Machines
		}
	}
	else
	{
		#WriteDebug "AD account $computer does not exist or is disabled."
	}
}

# Delete old XenDesktop assignments

foreach ($vm in $xdvmsnotinccp)
{
    WriteDebug "Removing desktop $vm"
	$vmproperties = Get-BrokerDesktop -MachineName $vm
	$desktopgroupname = $vmproperties.DesktopGroupName
	Remove-BrokerMachine -MachineName $vm -Force -DesktopGroup $desktopgroupname  -ErrorAction SilentlyContinue
	Remove-BrokerMachine -MachineName $vm -Force -ErrorAction SilentlyContinue
	$computer = $vm.split("\")[1]
    WriteDebug "Removing AD Computer $computer"
	Remove-ADComputer $computer -confirm:$false
	$desktopgroup = Get-BrokerDesktop -Filter {(DesktopGroupName -eq $desktopgroupname)}
	if ($desktopgroup) 
    {
        #WriteDebug "Desktop group $desktopgroupname still has desktops"
    }
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
	# Ignore machines that have just been deleted
	if ($xdvmsnotinccp -notcontains $vm)
	{
		$vmmachine = $vm.split("\")[1]
		$vmproperties = Get-BrokerDesktop -MachineName $vm
		$vmxdsid = $vmproperties.SID
		$adproperties = Get-ADComputer -identity $vmmachine
		$vmadsid = $adproperties.SID

		if ($vmxdsid -ne $vmadsid)
		{
			WriteDebug "$vmmachine SID mismatch --> Deleting VM XD registration"
			WriteDebug "AD SID: $vmadsid XD SID: $vmxdsid"
			$desktopgroupname = $vmproperties.DesktopGroupName
			Remove-BrokerMachine -MachineName $vm -Force -DesktopGroup $desktopgroupname
			Remove-BrokerMachine -MachineName $vm -Force
            WriteDebug "Forcing broker cache refresh after sid mis-match"
			Update-BrokerNameCache -Machines
		}
	}
}
}
#------------------END FUNCTIONS-------------------------------

#------------------START MAIN-------------------------------
Import-Module ActiveDirectory
if ((Get-PSSnapin -Name citrix.* -ErrorAction SilentlyContinue) -eq $null) {
    Add-pssnapin citrix.*
}
$ErrorActionPreference = "SilentlyContinue" 

# Create Catalog in XenDesktop
$catalog = Get-BrokerCatalog -Filter {(Name -eq $catalogname)} -ErrorAction SilentlyContinue
if ($catalog -eq $null) 
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
