# Get List of Registered Azure AD Applications using PowerShell
# April 24, 2018 by Morgan
# https://morgantechspace.com/2018/04/get-list-of-registered-azure-ad-apps-powershell.html

Connect-AzureAD
Get-AzureADApplication -All:$true
Get-AzureADServicePrincipal -All:$true | ? {$_.Tags -eq "WindowsAzureActiveDirectoryIntegratedApp"}
Get-AzureADApplication -Filter "DisplayName eq 'TestAppName'"
Get-AzureADApplication -Filter "AppId eq 'ca066717-5ded-411b-879e-741de0880978'"
Get-AzureADApplication -All:$true | Where-Object { $_.PublicClient -ne $true } | FT
Get-AzureADApplication -All:$true | Where-Object { $_.PublicClient -eq $true } | FT
Get-AzureADApplication -All:$true |
Select-Object DisplayName, AppID, PublicClient, AvailableToOtherTenants, HomePage, LogoutUrl  |
Export-Csv ".\AzureADApps.csv"  -NoTypeInformation -Encoding UTF8
