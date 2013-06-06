<#
    
Copyright © 2013 Citrix Systems, Inc. All rights reserved.

.SYNOPSIS
This script will synchronise desktops in XenDesktop and Cloud Platform.

.DESCRIPTION
 Virtual machines that exist in CloudPlatform with names that match a specified pattern will be registered in XenDesktop. Conversely, 
 desktops registered in XenDesktop that match the pattern but do not have a corresponding CloudPlatform virtual machine will be 
 de-registered.

.NOTES
    KEYWORDS: PowerShell, Apache CloudStack, Citrx CloudPlatform, XenDesktop
    REQUIRES: PowerShell Version 2.0
              XenDesktop PowerShell Snapins. 
              ActiveDirectory PowerShell Module. See http://technet.microsoft.com/en-us/library/ee617195.aspx
              PVS 6.1 PowerShell Snapin (install PVS 6.1 Console & use .NET InstallUtil.exe to install Snapin)
    

.LINK
     http://community.citrix.com/
#>
Param 
(  
    [string]$domain = [Environment]::UserDomainName,
      
    [string]$catalogname = 'Self Service Desktops',
    
    [string]$ccpip,
    
    [string]$hostnameprefix,
    
    [string]$templateid,
    
    [string]$isoid,
    
    $devicecollection
)

#------------------START FUNCTIONS-------------------------------

#
# Horrible hack to export debug messages to the enclosing windows service. This is required as the PowerShell host 
# invoked from .NET runtime does not support Write-Debug (!)
#
function WriteDebug($message) 
{
	Write-Output "[Debug]$message"
}

#
# Desktop Names are a prefix followed by one or more digits. Match is case-insensitive
#
function IsDesktopName($computerName)
{
	$prefix = $hostnameprefix.ToLower()
    return (($computerName -ne $null) -and ($computername.ToLower() -match "$prefix\d+")) 
}

#
# A CloudPlatform virtual machine is considered to be a desktop if its display name matches the 
# given pattern and it is not destroyed or expunging.
#
function IsCloudPlatformDesktop($virtualmachine) 
{
    return (IsDesktopName $virtualmachine.displayname) -and
           ($virtualmachine.state -ne "Destroyed") -and 
           ($virtualmachine.state -ne "Expunging")  
}

#
# List the VMs in CloudPlatform that use the given template/ISO and are marked as desktops, The function returns
# an array of virtual machine records. Each VM record is a hash 
#
function GetCloudDesktops($template, $iso) {

    $filter = ""
    if (-not [string]::IsNullOrEmpty($template)) {
        $filter = "&templateid=$template"
    } elseif (-not [string]::IsNullOrEmpty($iso)) {
        $filter = "&isoid=$iso"
    }

    $ccpvms = New-Object System.Xml.XmlDocument
    $ccpvmlisturl = 'http://'+$ccpip+':8096/client/api?command=listVirtualMachines&listall=true'+$filter
    $ccpvms.Load($ccpvmlisturl)
    $ccpvmarray = @()
    $ccpvmarraymachine = @()

    foreach ($virtualmachine in $ccpvms.listvirtualmachinesresponse.virtualmachine)
    {
        if (($virtualmachine -ne $null) -and (IsCloudPlatformDesktop $virtualmachine))
	    {
            $macs = @()
            foreach ($nic in $virtualmachine.nic) 
            {
                $macs += $nic.macaddress
            }		 
            $vmhash = @{}
            $vmhash["id"] = $virtualmachine.id
            $machine = $domain + '\' + $virtualmachine.displayname
            $vmhash["machine"] = $machine.ToLower()
            $vmhash["displayname"] = $virtualmachine.displayname.ToLower()
            $vmhash["user"] = $virtualmachine.account.ToLower()
            $vmhash["macs"] = $macs
		    $ccpvmarray += ,$vmhash
            $ccpvmarraymachine += ,($machine.ToLower())		   
	   }
    }
    return ($ccpvmarray, $ccpvmarraymachine)
}

#
# Reboot a cloud desktop
#
function RebootCloudDesktop($vm) {
    $ccpout = New-Object System.Xml.XmlDocument
    $url = 'http://'+$ccpip+':8096/client/api?command=rebootVirtualMachine&id='+$vm["id"]
    $ccpout.Load($url)
}

#
# Create device in PVS, and join it to the domain
#
function CreatePvsDevice($name, $mac, $site, $collection) 
{
    $pvsmac = $mac.Replace(':','-')
    $result = mcli-add Device -r deviceName=$name,collectionName=$collection,siteName=$site,deviceMac=$pvsmac,copyTemplate=1
    $last = $result[$result.length-1]
    $tokens = $last.Split(' ');
    $id = $tokens[$tokens.length-1]
    $null = mcli-run AddDeviceToDomain -p deviceId=$id
}

#
# Get the specified PVS device as a hash
#
function Get-PvsDevice($name) {
    $list = mcli-get Device -p deviceName=$name 
    $result = @()
    foreach ($item in $list) {
        if ($item.StartsWith("Record #")) {
            $result += @{}
            continue
        }
        $parts = $item.Split(':')       
        if (($parts.Count -eq 2) -and ($result.Count -gt 0)) {
            $value = $parts[1].Trim() 
            if (-not [string]::IsNullOrEmpty($value)) {
                $result[$result.Count-1].Add($parts[0], $value)
            }
        }   
    }
    return ,$result
}

#
# Delete PVS device
#
function DeletePvsDevice($name)
{
    $null = mcli-delete Device -p deviceName=$name
}

#
# Get a list of the desktops registered in the specified catalog
#
function GetXenDesktops($catalog)
{
    $xdvmarray = @()
    $xdvms = Get-BrokerMachine -CatalogName $catalog -ErrorAction SilentlyContinue
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
    return ,$xdvmarray
}


function SyncXenDesktop()
{
    # Load VMs from CloudPlatform
    ($ccpvmarray,$ccpvmarraymachine) = GetCloudDesktops $templateid $isoid

    # Load VMs from XenDesktop
    $xdvmarray = GetXenDesktops $catalogname
	
    # Compare CloudPlatform and XenDesktop VMs
    $ccpvmsnotinxd =@()
    $xdvmsnotinccp =@()

    #WriteDebug "CloudPlatform VMs:"
    #foreach ($vm in $ccpvmarray) 
    #{ 
    #     WriteDebug $vm["machine"]
    #}

    #WriteDebug "XenDesktop VMs:"
    #foreach ($vm in $xdvmarray) 
    #{ 
    #     WriteDebug "$vm" 
    #}    
    Foreach ($vm in $ccpvmarray)
    {
        If ($xdvmarray -notcontains $vm["machine"])
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
        #WriteDebug "CloudPlatform VMs not in XenDesktop:"
        #foreach ($vm in $ccpvmsnotinxd) 
        #{
        #    WriteDebug $vm["machine"]
        #}
    }
    if ($xdvmsnotinccp.length -gt 0)
    {
        #WriteDebug "XenDesktop VMs not in CloudPlatform:"
        #foreach ($vm in $xdvmsnotinccp) 
        #{
        #    WriteDebug "XD Vm not in CP $vm"
        #}
    }
    # Create new XenDesktop assignments
    foreach ($vm in $ccpvmsnotinxd)
    {  
        $computer = $vm["machine"].split("\")[1]
	    $adcomputer = $null  
        try 
        {
            # -ErrorAction does not work with Get-ADComputer (known issue)
	        $adcomputer=Get-ADComputer -identity $computer -ErrorAction SilentlyContinue
        } catch 
        {
        }     
	    if (($adcomputer -ne $null) -and ($adcomputer.enabled -eq "True"))
	    {
            WriteDebug "Registering new CloudPlatform desktop $computer in XenDesktop"
		    $catalog = Get-BrokerCatalog -Filter {(Name -eq $catalogname)}  
		    $newmachine=New-BrokerMachine -MachineName $adcomputer.SID -CatalogUid $catalog.uid 
		    if ($newmachine.machinename.ToLower() -eq $vm["machine"])
		    {
			     $userincldomain = $domain + '\' + $vm["user"]		 
                 Start-Sleep -s 15
			     Add-BrokerUser -Name $userincldomain -Machine $vm["machine"]
			
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
			     Add-BrokerMachine -MachineName $vm["machine"] -DesktopGroup $desktopgroupname
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
            # No AD account for this desktop. If this is a PVS based desktop, create a new device
            # (which will create the AD account). If it is a sysprep desktop the account will be created
            # by the Windows Setup process so there is nothing to do.
            if ($devicecollection -ne $null) { 
                $name = $vm["displayname"]
                WriteDebug "Creating PVS device $name" 
                CreatePvsDevice $name $vm["macs"][0] $devicecollection.site $devicecollection.Name
                RebootCloudDesktop $vm              
            }      
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
        if ($devicecollection -ne $null) { 
            WriteDebug "Deleting PVS device $computer"
            DeletePvsDevice $computer
        }
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

#------------------  START MAIN  ------------------------------

#
# Any terminating errors should stop the script & return error stream
# for calling context
#
$ErrorActionPreference = "Stop" 

try
{
    #
    # If there is a PVS device collection specified, load the PVS snap-in 
    #
    if ($devicecollection -ne $null) {
        if ((Get-PSSnapin -Name mclipssnapin -ErrorAction SilentlyContinue) -eq $null) {
            Add-pssnapin mclipssnapin
        }
        $server = $devicecollection.Server
        $null = mcli-run setupconnection -p server=$server
    }
    Import-Module ActiveDirectory
    if ((Get-PSSnapin -Name citrix.* -ErrorAction SilentlyContinue) -eq $null) {
        Add-PSSnapin citrix.*
    }

    # Create Catalog in XenDesktop if necessary
    $catalog = Get-BrokerCatalog -Filter {(Name -eq $catalogname)}
    if ($catalog -eq $null) 
    {
	   WriteDebug "Catalog $catalogname does not exist (creating it)"
	   New-BrokerCatalog -Name $catalogname -CatalogKind unmanaged -AllocationType permanent
    }
    SyncXenDesktop 
} 
catch [System.Management.Automation.ActionPreferenceStopException] {
    "StopExecution Exception: "+$error[0]
}
finally {
    if ($devicecollection -ne $null) {
        $null = mcli-run unloadconnection
    }
}
    
#------------------ END MAIN  ------------------------------