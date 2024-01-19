# Usage: migrations_su.ps1 <previous_migration> <next_migration_number> <next_migration_name>
# Usage: migrations_su.ps1 '0001_Initial' '0003_StudycastIntegration'
$previous_migration=$args[0]
$next_migration_name=$args[1]
$full_script_path="../TestTemplate9.Migrations/TestTemplate9Scripts/" + $next_migration_name + ".sql"
cd ./src/TestTemplate9.Data
dotnet ef migrations add $next_migration_name --startup-project ../TestTemplate9.Api/TestTemplate9.Api.csproj --context TestTemplate9DbContext
if ($previous_migration -eq '')
{
    dotnet ef migrations script --startup-project ../TestTemplate9.Api/TestTemplate9.Api.csproj --context TestTemplate9DbContext -o $full_script_path
}
else
{
    dotnet ef migrations script --startup-project ../TestTemplate9.Api/TestTemplate9.Api.csproj --context TestTemplate9DbContext $previous_migration $next_migration_name -o $full_script_path
}
cd ../..