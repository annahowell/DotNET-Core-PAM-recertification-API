# Privileged Access Management recertification API 

API written as part of my University dissertation. Allows the projects stakeholders to recertify the privileged access of employee roles using the [frontend](https://github.com/annahowell/PAMrecert-Frontend). Also enables their ticketing system to automatically update the database when a privileged access request is made.

.NET Core and MS SQL Server were stakeholder requirements as this allowed their dev teams to support it going forward.


## Installation

1. Setup MS SQL Server, then create the database and schema using the SQL files in `/sql`.

2. Modify `/PAMrecert/appsettings.json` file such that it reflects your database configuration from Step 1: `"PAMrecertDataBaseConnection": "Server=192.168.0.100;Database=pam;UID=Foo;PWD=bar;"`

3. Edit line 90 of the `PAMrecert/Startup.cs` file and add the location of the [frontend](https://github.com/annahowell/PAMrecert-Frontend) and any other locations which will do API calls to the list of `WithOrigins`. Failing to do this properly will cause CORS errors.

4. Compile the API and point your httpd to the resulting bin directory.


## Screenshots

Role endpoints

![RolePrivs](https://github.com/annahowell/PAMrecert-API/blob/master/screenshots/1.png)
