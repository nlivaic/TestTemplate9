# Used only for local deployments.
. ./deployment/variables.ps1

$account = az account show | ConvertFrom-Json

$entraApiRegistration = ./deployment/entraApiRegistration.ps1 $PROJECT_NAME $account.tenantId

$provisionResourceGroup = ./deployment/provisionResourceGroup.ps1 $LOCATION $ENVIRONMENT $PROJECT_NAME

./deployment/provisionResources.ps1 $ENVIRONMENT $PROJECT_NAME $provisionResourceGroup.resourceGroupName $DB_USER $DB_PASSWORD $entraApiRegistration.authAuthority $entraApiRegistration.authAudience $entraApiRegistration.authValidIssuer