param ($location, $environment, $projectName)

# Resource group
Write-Host "------ Create Resource Group START ------" -ForegroundColor Yellow
$jsonResultRg = az deployment sub create --location $location --template-file ./deployment/resource-group.bicep --parameters environment=$environment projectName=$projectName location=$location | ConvertFrom-Json
$resourceGroupName = $jsonResultRg.properties.outputs.resourceGroupName.value
Write-Host "Created resource group named: $($resourceGroupName)" -ForegroundColor Green
Write-Host "##vso[task.setvariable variable=resourceGroupName;isoutput=true]$resourceGroupName"
Write-Host "------ Create Resource Group END ------" -ForegroundColor Yellow

# Used only for local deployments.
[hashtable]$Rg = @{}
$RG.ResourceGroupName = $resourceGroupName
return $Rg
