# New solution - quickstart guide

> All the steps outlined here are detailed further on down in the document.

1. Run docker-desktop
2. Run `./configure.ps1`
3. Open solution using Visual Studio, set docker-compose as Startup project and run the solution
4. Run `./create_migration.ps1 '' 'InitialMigration'`
5. Run `./migrate.ps1`
6. Run the solution again.
6. Go to http://localhost:5000/index.html

At this point only `.gitignore` has been committed locally. Now you can make some changes to the source code, push it to GitHub and get it deployed to your provisioned Azure resources:

1. Go to GitHub, create a new repository and add remote origin.
2. `git push -u origin master`
3. You can remove or update some of the endpoints and models if you like.
4. Go to Azure DevOps and create a new project.
5. Open `release_pipeline.yml` and set the value for `adoProject` variable. No need to touch anything if you are ok with the name and ADO project has the same name.
6. Set up [Azure Service Connection](#azure-service-connection). Copy the name from ADO to `release_pipeline.yml` to several properties named `azureSubscription` (or just name it `AzureConnection` in ADO).
7. `git checkout -b feature/initial-code-commit; git add *; git commit -m "Initial code commit."; git push -u origin feature/initial-code-commit`
8. Create and approve PR.
9. `git checkout master; git pull`
10. Now configure the pipelines on ADO. Add three new pipelines (`pr_pipeline`, `build_pipeline`, `release_pipeline`) based off of YAML files with the same name.
11. Configure the pipeline variables for `release_pipeline`. More on that in section [Release pipeline Database Migrations and Provisioning resources](#release-pipeline-database-migrations-and-provisioning-resources).
12. Create a new feature branch `git checkout -b feature/my-first-feature`. Do your work, create a PR and let the `pr_pipeline` do its work.
13. Approve PR. Let `build_pipeline` and `release_pipeline` do their work. You will probably need to open `release_pipeline` on the first run and approve some stuff.
14. Provision Azure resources - `release_pipeline` will do the work here as well.
    * Manual provisioning: if you want to test your infrastructure out regardless of the pipeline, run `. ./provisionLocal.ps1` and this will provision everything to Azure. It will NOT migrate the database! Make sure you do `az login` first and log in to the correct subscription. Open `variables.ps1` and make sure everything is properly defined.

At this point you have a local environment and Azure Service Bus fully set up, along with ADO pipelines ready deploy your code to a working AppService. Start working on your features!

# Before You Get Started

## Install a Docker host

E.g. Docker Desktop:

    choco install docker-desktop

## Configuration

### Set configuration

Most of the stuff is in the `.env` file. This is a git ignored file, but it has the relevant structure in it. It gets generated by `./configure.ps1`. This script also prepopulates some values. It helps if you have the following beforehand:

- Service Bus connection strings (details above).
- Application Insights connection strings (details above).
- Azure AD authority and API identifier.

### Database configuration

Database connection string for both `Api`, `WorkerServices` and `Migrations` projects is in the `.env` file. This was a deliberate choice, because I wanted the templated project to have a connection string automatically generated and in line with the name of the solution. You will notice there are two connection strings: `ConnectionStrings__TestTemplate9DbConnection` is used by `Api` and `WorkerServices`. `Migrations` has a separate one `ConnectionStrings__TestTemplate9Db_Migrations_Connection` because it is accessing the dockerized database from outside.

Username and password for the database are provided as default values, but you can provide whatever values you want.
Make sure you set the database-related variables (prefixed `DB_`) before you run the solution for the first time, otherwise the database will be configured with given administrator password and a username and password for the application user. If you don't change those values before running the solution you will have to delete the `testtemplate9.sql` container and accompanying volumes. If you change `DB_PASSWORD`, make sure the same value is set in `InitializeTestTemplate9Db.sql` for the login as well.

When you first run the solution, an SQL script found in `src/InitializeTestTemplate9Db.sql` is executed, creating the database with an admin account (password in `DB_ADMIN_PASSWORD`), login and user (`DB_USER` and `DB_PASSWORD`). User is then assigned to read, write and DDL roles.

Application is accessing the database as a `DB_USER`/`DB_PASSWORD`, with a generated connection string found in `ConnectionStrings__TestTemplate9DbConnection` and `ConnectionStrings__TestTemplate9Db_Migrations_Connection`.

# Running The Application

Make sure to set the `docker-compose` as the startup project. The application can be reached by default on `localhost:5000`. You can change this in the `docker-compose.yml`. Just go to `/index.html` to see the initial API.

At this point you have several things up and running:

- API (dockerized)
- Worker service (dockerized)
- Empty Sql Server database with a volume (dockerized)

Now it is time to create some tables in the database. From the root of your solution, first run `.\create_migration.ps1 '' '0001_Initial'` and then `./migrate.ps1`. Now you have to go to the SSMS and register your database server there. It is accessible on localhost, port 1433, with the username and password you set in your `.env` file under `DB_USER` and `DB_PASSWORD`.

# Additional Stuff

## Branching strategy

Feature branches strategy is supported out of the box. This strategy expects all development to go through branches and committing directly to `master` is not allowed. Supported branches:

* `feature/`
* `fix/`

## Pipelines

### Naming the ADO project

`release_pipeline.yml` - `project` property on lines 43, 53, 63 should be the name of your ADO project.

### Azure YAML pipelines:

* `pr_pipeline`
* `build_pipeline`
* `release_pipeline`

All pipelines build and deploy all applications (`Api` and `WorkerServices`) in the solution.

When creating ADO pipelines, name them just like the files are named (minus the `.yml` suffix). Naming the pipelines same as the files is important because the `release_pipeline` is triggered by a successful `build_pipeline` run. If you decide to name your ADO pipelines differently, make sure you change two things in `release_pipeline.yml` - update `source` on line 8 and `definition` on line 40, 50, 60 to match the **build** pipeline name in ADO (if needed).

### Pipeline configuration

#### Azure Service Connection

In `release_pipeline.yml:72` there is an Azure subscription name (property name `azureSubscription` with initial value `AzureConnection_TestTemplate9`) - make sure the name is the same as what is in Azure.

If you are logged into ADO and Azure with different usernames, then you will need to go through additional steps to hook up ADO and Azure (essentially by creating a Service Principal and letting ADO know about it): more details [here](https://www.devcurry.com/2019/08/service-connection-from-azure-devops-to.html). The previous link describes the process nicely, but in case it is down try [this](https://learn.microsoft.com/en-us/azure/devops/pipelines/library/connect-to-azure?view=azure-devops#create-an-azure-resource-manager-service-connection-with-an-existing-service-principal) one.

Additionally, for the release pipeline to be able to register an API with Azure AD, you have to assign the Service Principal (above) an appropriate role (`Application Administrator`) on Entra, as described [here](https://stackoverflow.com/a/66204622/987827).

#### Release pipeline Database Migrations and Provisioning resources

`release_pipeline` deploys to resource group and resources based on pipeline variables:
* `DB_PASSWORD` - administrator password of your choosing.
* `DB_USER` - administrator username of your choosing.
* `SUBSCRIPTION` - Azure subscription identifier
* `LOCATION` - must match names of regions Azure can understand, e.g. `westeurope`.
* `ENVIRONMENT` - a moniker of your choosing to describe what environment you are deploying to.
* `PROJECT_NAME` - a moniker of your choosing to denote the project.

#### First deployment run

* `pr_pipeline` - on your first PR, the `pr_pipeline` will get triggered.
* `build_pipeline` - once you merge the PR, the build pipeline will get triggered. It is similar to `pr_pipeline`, except it uploads artifacts.
* `release_pipeline` - once the `build_pipeline` is done, `release_pipeline` will get triggered, but it will stall. You need to manually give a few permissions, it should start running from there on.

### Branches

**All** pipelines work with `master` branch . If you are using `main`, remember to do a search and replace.

## Provisioning resources on Azure manually

Even though the pipelines are built in such a way to take advantage of Bicep files to provision stuff on Azure, you can run those scripts on your own by executing `. ./provisionLocal.ps1`. Before running that script, look into `variables.ps1` file - it has all the parameters needed to provision, but you can change values if you wish. Make sure the variable values here are the same ones as in the release pipeline, otherwise you will end up with two different resource groups.

## Project naming

All projects have a prefix `TestTemplate9` and pipelines latch onto that detail. If you want to start renaming projects, you should also do a search and replace across all the files in the solution. Be careful!

## Versioning

We are using semver and GitVersion. Each commit message gets a suffix (defined in `./githooks/prepare-commit-msg` and recognized in `GitVersion.yml`). `feature/` branch gets a suffix saying GitVersion should bump minor version. `fix/` branch gets a suffix saying GitVersion should bump patch version. Bumping major version needs to be done manually by tagging a commit. We do not embed the version in the assemblies yet. GitVersion depends on `--no-ff` merges to be able to deduce version successfully. Make sure your ADO project enforces this, do not allow developers to merge PRs differently! 

## Migrations

For migrations to work `.env` file must be properly set up with database credentials and connection string configured.

### Creating migrations

The below commands must be executed from solution root folder using Powershell. If this is the first migration in your project, execute:

    .\create_migration.ps1 '' '0001_Initial'

Every next migration must contain the name of the migration immediately preceeding it:

    .\create_migration.ps1 '0001_Initial' '0002_Second'

### Applying migrations

Command must be executed from solution root folder using Powershell. You will notice it is executing from a Docker container and Docker compose - the reason is this way there is only one `.env` which can be shared by all executeable projects in the solution (`Ąpi`, `Migrations`, `WorkerServices`).

    ./migrate.ps1
