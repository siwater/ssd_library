<#
    
Copyright © 2013 Citrix Systems, Inc. All rights reserved.

.SYNOPSIS
This script will list the desktops for a specified user.

.DESCRIPTION
 

.NOTES
    KEYWORDS: PowerShell, Apache CloudStack, Citrx CloudPlatform, XenDesktop
    REQUIRES: PowerShell Version 2.0
              XenDesktop PowerShell Snapins. 
            
.LINK
     http://community.citrix.com/
#>
Param 
(  
    [string]$domain = [Environment]::UserDomainName,
      
    [string]$user
)
#
# Any terminating errors should stop the script & return error stream
# for calling context
#
$ErrorActionPreference = "Stop" 

try
{
    if ((Get-PSSnapin -Name citrix.* -ErrorAction SilentlyContinue) -eq $null) {
        Add-PSSnapin citrix.*
    }
    $domainuser = $domain + '\' + $user
    $desktops = Get-BrokerDesktop -AssociatedUserName $domainuser
    $result = @()
    foreach ($desktop in $desktops) {
        if ($desktop -ne $null) {
            $d = @{}
		    $name = $desktop.MachineName
            [string]$state = $desktop.SummaryState
            $d["machine-name"] = $name.Substring($name.IndexOf('\')+1)
            $d["summary-state"] = $state
            $result += $d
        }
    }
    return $result

   
} 
catch [System.Management.Automation.ActionPreferenceStopException] {
    "StopExecution Exception: "+$error[0]
}