param ($sqlAdminsGroupName, $sqlAdminUserName, $sqlAdminUserPassword, $sqlUsersGroupName, $resourceGroupName, $appServiceWebName, $sqlServerName)

$domain = (az rest --method get --url 'https://graph.microsoft.com/v1.0/domains?$select=id' --query value --output tsv)
####################################################
### Create and Populate Azure Sql Admin Group
####################################################
Write-Host "--- Create and Populate Azure Sql Admin Group - START ---" -ForegroundColor Yellow
$sqlAdminUserPrincipalName = $sqlAdminUserName + '@' + $domain

$sqlAdminsGroupId=(az ad group list --filter "displayName eq '$sqlAdminsGroupName'" --query '[].id' --output tsv)

If ($sqlAdminsGroupId -eq $null) {
    $sqlAdminsGroupId=(az ad group create --display-name $sqlAdminsGroupName --mail-nickname $sqlAdminsGroupName --query id --output tsv)
    Write-Host "Created Entra group '$sqlAdminsGroupName' with group Id: $sqlAdminsGroupId" -ForegroundColor Green
}

$sqlAdminUserId = (az ad user list --filter "userPrincipalName eq '$sqlAdminUserPrincipalName'" --query '[].id' --output tsv)

If ($sqlAdminUserId -eq $null) {
    $sqlAdminUserId = (az ad user create --display-name $sqlAdminUserName --password $sqlAdminUserPassword --user-principal-name $sqlAdminUserPrincipalName --query id --output tsv)
    Write-Host "Created Entra user '$sqlAdminUserPrincipalName' with user Id: $sqlAdminUserId" -ForegroundColor Green
}

$isInGroup = (az ad group member check --group $sqlAdminsGroupId --member-id $sqlAdminUserId  --query value --output tsv)
if ($isInGroup -eq 'false') {
    az ad group member add --group $sqlAdminsGroupId --member-id $sqlAdminUserId
    Write-Host "Added Entra user '$sqlAdminUserId' to group: $sqlAdminsGroupId" -ForegroundColor Green
}
Write-Host "--- Create and Populate Azure Sql Admin Group - END ---" -ForegroundColor Yellow

####################################################
### Create Azure Sql User Group
####################################################
Write-Host "--- Create and Populate Azure Sql User Group - START ---" -ForegroundColor Yellow
$sqlUsersGroupId=(az ad group list --filter "displayName eq '$sqlUsersGroupName'" --query '[].id' --output tsv)
If ($sqlUsersGroupId -eq $null) {
    $sqlUsersGroupId=(az ad group create --display-name $sqlUsersGroupName --mail-nickname $sqlUsersGroupName --query id --output tsv)
    Write-Host "Created Entra group '$sqlUsersGroupName' with group Id: $sqlUsersGroupId" -ForegroundColor Green
}
Write-Host "--- Create and Populate Azure Sql User Group - END ---" -ForegroundColor Yellow

####################################################
### Set Sql Server Admin
####################################################
Write-Host "--- Set Sql Server Admin - START ---" -ForegroundColor Yellow
$sqlServerAdmin = az sql server ad-admin list --resource-group WE-DEV-TESTTEMPLATE9-RG --server-name wedevtesttemplate9sql1 --query '[].login' --output tsv
If ($sqlServerAdmin -eq $null) {
    az sql server ad-admin create --display-name $sqlAdminsGroupName --object-id $sqlAdminsGroupId --resource-group $resourceGroupName --server-name $sqlServerName
    Write-Host "Set group '$sqlAdminsGroupName' as Sql Server Admin for Sql server '$sqlServerName'" -ForegroundColor Green
}
Write-Host "--- Set Sql Server Admin - END ---" -ForegroundColor Yellow

####################################################
### AAD only auth
####################################################
Write-Host "--- AAD only auth - START ---" -ForegroundColor Yellow
$isAdOnlyAuthEnabled = (az sql server ad-only-auth get --resource-group $resourceGroupName --name $sqlServerName --query azureAdOnlyAuthentication --output tsv)
If ($isAdOnlyAuthEnabled -eq "false") {
    az sql server ad-only-auth enable --resource-group $resourceGroupName --name $sqlServerName
    Write-Host "Enabled Azure AD-only auth for Sql server '$sqlServerName'" -ForegroundColor Green
}
Write-Host "--- AAD only auth - START ---" -ForegroundColor Yellow

####################################################
### Create Managed Identity for Web API
####################################################
Write-Host "--- Create Managed Identity for Web API - START ---" -ForegroundColor Yellow
# Enable managed identity on app
$managedIdentityId = (az webapp identity show --resource-group $resourceGroupName --name $appServiceWebName --query principalId --output tsv)
If ($managedIdentityId -eq $null) {
    $managedIdentityId = (az webapp identity assign --resource-group $resourceGroupName --name $appServiceWebName  --query principalId --output tsv)
    Write-Host "Created system-assigned managed identity for '$appServiceWebName' with Id: $managedIdentityId" -ForegroundColor Green
}
# Add Managed Identity to sqlusersgroup
$isInGroup = (az ad group member check --group $sqlUsersGroupId --member-id $managedIdentityId  --query value --output tsv)
if ($isInGroup -eq 'false') {
    az ad group member add --group $sqlUsersGroupId --member-id $managedIdentityId
    Write-Host "Added Entra Managed Identity '$appServiceWebName' with id '$managedIdentityId' to group: $sqlUsersGroupId" -ForegroundColor Green
}
Write-Host "--- Create Managed Identity for Web API - END ---" -ForegroundColor Yellow
