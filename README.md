# ScSoMe

## Database
* In SQL Mgmt Studio -> Import Data-tier Application... to restore the Database from
https://startupcentral.sharepoint.com/sites/StartupCentral/Delte%20dokumenter/Forms/AllItems.aspx?id=%2Fsites%2FStartupCentral%2FDelte%20dokumenter%2FIT%2FScSoMe%2Fdb&viewid=70037813%2D5d56%2D441e%2D9e76%2Db08923a2aeca
  * Note there is a diagram and the _Comments_ table holds both posts and comments

* For now this is a database-first project, so to update the EF model from the Database:
  * Open a Terminal for VS-project ScSoMe.EF hence in the folder _\ScSoMe\ScSoMe.EF>_ and run:
    * dotnet ef dbcontext scaffold  "Server=(LOCAL)\SQLExpress;Database=ScSoMe;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer  -f -d
  * You may need to install or update the EF tool first:
    * dotnet tool install --global dotnet-ef
    * dotnet tool update --global dotnet-ef  

## API
* Is based on OpenAPI so you can run the project _ScSoMe.API_ and interact with it using Swagger UI

## Tests
* The _ScSoMe.API.Test_ are not always stable - sometimes it fails to start the API on port 5000
* To update the generated OpenAPI client e.g., if you added methods/changed signature or the DTO classes:
  * Open the solution twice
  * In one solution Start the API project
  * In the other solution
    * Rightclick on "Dependencies" of e.g. VS-project _ScSoMe.Common_ --> Manage Connected Services
    * Refresh the ScSoMeApi and answer Yes to use the URL: _https://localhost:7297/swagger/v1/swagger.json_    

## Blazor
* CSS isolation does not seem to work with MudBlazor components (the unique identifier is not added to the element). Fix:
  * Wrap the component in a `<div>`
  * Add a CSS `class` to the MudBlazor component
  * In the .razor.css file associated with the component: `::deep .class-name {}`

## MAUI
* `One or more invalid file names were detected`
  * When navigating through Finder on MacOS, a .DS-Store file is created, which is an invalid file name for MAUI Resources
  * To fix, remove the .DS-Store files from Resources/ and all its subfolders
  * Another fix could be to only navigate from the Terminal

## Failed to determine the https port for redirect
* On MacOS, the ports are automatically changed sometimes. To fix this issue, change the port in launchsettings.json to the previous one (or just copy the launchsettings.json from GitHub).