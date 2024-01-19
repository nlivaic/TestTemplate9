Write-Output "This script will set up the git init (if not done already) and initialize githooks."
Write-Output "Please enter following information to configure TestTemplate9 application for local use."
Write-Output "Values you provide here will be bound to .env file."
Write-Output "Default values are provided for usernames and passwords, but you can enter a different value if you like."
Write-Output "Some inputs do not have default values, you will probably have to get these yourself from external systems (Azure)."
Write-Output "You can rerun the script but no new values will be applied to the .env file."
Write-Output "If you want to edit a previously provided value, it is best to edit .env file manually."

# If none, create a ".env" file
if (!(Test-Path ".env"))
{
   New-Item -name ".env" -type "file" -value @"
APPLICATIONINSIGHTS_CONNECTION_STRING=<applicationinsights_connection_string>
ConnectionStrings__TestTemplate9DbConnection=Data Source=testtemplate9.sql;Initial Catalog=TestTemplate9Db
ConnectionStrings__TestTemplate9Db_Migrations_Connection=Data Source=host.docker.internal,1433;Initial Catalog=TestTemplate9Db;Encrypt=False
ConnectionStrings__MessageBroker=<msg_broker_connection_string>
DB_USER=<db_user>
DB_PASSWORD=<db_pw>
DB_ADMIN_PASSWORD=<db_admin_pw>
AUTH__AUTHORITY=https://login.microsoftonline.com/<auth_tenant_id>/v2.0
AUTH__AUDIENCE=<auth_audience>
AUTH__VALID_ISSUER=https://sts.windows.net/<auth_tenant_id>/
"@
    Write-Host "Created new '.env' file."
}

# If none, create a ".variables.ps1" file
if (!(Test-Path "deployment/variables.ps1"))
{
   New-Item -name "deployment/variables.ps1" -type "file" -value @"
# Used only for LOCAL deployment!
`$SUBSCRIPTION=""
`$LOCATION=""
`$ENVIRONMENT=""
`$PROJECT_NAME=""
`$DB_USER=""
`$DB_PASSWORD=""
"@
    Write-Host "Created new 'deployment/variables.ps1' file."
}

# Database administrator password
$db_admin_pw_default = "Pa55w0rd_1337"
if (!($db_admin_pw = Read-Host "Database admin password [$db_admin_pw_default]")) { $db_admin_pw = $db_admin_pw_default }
# Database username
$db_user_default = "TestTemplate9Db_Login"
if (!($db_user = Read-Host "Database user name [$db_user_default]")) { $db_user = $db_user_default }
# Database password
$db_pw_default = 'Pa55w0rd_1337'
if (!($db_pw = Read-Host "Database user password [$db_pw_default]")) { $db_pw = $db_pw_default }
# Message broker connection string
$msg_broker_connection_string = Read-Host -Prompt 'Message broker connection string (Azure Service Bus)'
# Azure Application Insights Connection String
$applicationinsights_connection_string = Read-Host -Prompt 'Application Insights connection string (Azure)'
# Azure AD Tenant Id
$auth_tenant_id = Read-Host -Prompt 'Azure AD Tenant Id'
# Claim identifying this API
$auth_audience = Read-Host -Prompt 'This APIs audience identifier'

(Get-Content ".env").replace("<db_admin_pw>", $db_admin_pw) | Set-Content ".env"
(Get-Content ".env").replace("<db_user>", $db_user) | Set-Content ".env"
(Get-Content ".env").replace("<db_pw>", $db_pw) | Set-Content ".env"
(Get-Content ".env").replace("<msg_broker_connection_string>", $msg_broker_connection_string) | Set-Content ".env"
(Get-Content ".env").replace("<applicationinsights_connection_string>", $applicationinsights_connection_string) | Set-Content ".env"
(Get-Content ".env").replace("<auth_tenant_id>", $auth_tenant_id) | Set-Content ".env"
(Get-Content ".env").replace("<auth_audience>", $auth_audience) | Set-Content ".env"

# git init only on a new repo
git rev-parse --is-inside-work-tree | Out-Null
if ( $LASTEXITCODE -ne 0)
{
    git init
    git add .gitignore
    git commit -m "gitignore"
}

git config core.hooksPath "./githooks"