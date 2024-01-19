param ($environment, $projectName, $resourceGroupName, $dbUser, $dbPassword, $authAuthority, $authAudience, $authValidIssuer)

Write-Host "------ Create Resources START ------" -ForegroundColor Yellow
$jsonResultAll = az deployment group create --resource-group $resourceGroupName --template-file ./deployment/iac.bicep --parameters environment=$environment projectName=$projectName db_user=$dbUser db_password=$dbPassword authAuthority=$authAuthority authAudience=$authAudience authValidIssuer=$authValidIssuer | ConvertFrom-Json
$appServiceWebName = $jsonResultAll.properties.outputs.appServiceWebName.value
Write-Host "##vso[task.setvariable variable=appServiceWebName;isoutput=true]$appServiceWebName"
$dbConnection = $jsonResultAll.properties.outputs.dbConnection.value
Write-Host "##vso[task.setvariable variable=dbConnection;isoutput=true]$dbConnection"
Write-Host "------ Create Resources END ------" -ForegroundColor Yellow
