param ($environment, $projectName, $resourceGroupName, $dbUser, $dbPassword, $authAuthority, $authAudience, $authValidIssuer)

Write-Host "##[warning]------ Create Resources START ------"
$jsonResultAll = az deployment group create --resource-group $resourceGroupName --template-file ./deployment/iac.bicep --parameters environment=$environment projectName=$projectName db_user=$dbUser db_password=$dbPassword authAuthority=$authAuthority authAudience=$authAudience authValidIssuer=$authValidIssuer | ConvertFrom-Json
$appServiceWebName = $jsonResultAll.properties.outputs.appServiceWebName.value
$sqlServerName = $jsonResultAll.properties.outputs.sqlServerName.value
Write-Host "##vso[task.setvariable variable=appServiceWebName;isoutput=true]$appServiceWebName"
Write-Host "##vso[task.setvariable variable=sqlServerName;isoutput=true]$sqlServerName"
$dbConnection = $jsonResultAll.properties.outputs.dbConnection.value
Write-Host "##vso[task.setvariable variable=dbConnection;isoutput=true]$dbConnection"
Write-Host "##[warning]------ Create Resources END ------"
