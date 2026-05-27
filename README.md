# R-PAC Production Planning Tool

An internal web application built for R-PAC International to manage and track production orders, machine scheduling, inventory, and shift operations across departments.

Built with **ASP.NET MVC 5**, **SQL Server**, and **SignalR** for real-time updates.

---

## Tech Stack

- ASP.NET MVC 5 (.NET Framework 4.7.2)
- SQL Server (MSSQL) with Dapper ORM
- SignalR for real-time dashboard updates
- Bootstrap 5 + Chart.js for the UI
- SAP Business One Service Layer integration (optional)

---

## Setup

### Prerequisites
- Visual Studio 2019 or later
- SQL Server 2016+ (or SQL Server Express)
- .NET Framework 4.7.2

### 1. Database
Run the SQL script to create the schema and seed initial data:
```
MSSQL Databse File.sql
```

### 2. Connection String
Update `web.config` with your SQL Server connection details:
```xml
<connectionStrings>
  <add name="MSSQLServer"
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=RPACProduction;Integrated Security=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 3. NuGet Packages
Restore packages via Visual Studio (right-click solution → Restore NuGet Packages) or run:
```
nuget restore RPACProductionPlanner.sln
```

### 4. Run
Open `RPACProductionPlanner.sln` in Visual Studio and press **F5** to start.

The default admin credentials are set in the SQL seed script.

---

## Features

- **Production Orders** — Create, track, and update orders by department and shift
- **Gantt Scheduler** — Drag-and-drop machine scheduling with conflict detection
- **Dashboard** — Live KPIs, machine status matrix, and alert notifications
- **Inventory / BOM** — Bill of materials tracking with low-stock alerts
- **Reports** — CSV exports for production schedules and inventory
- **Admin** — User and role management

---

## Project Structure

```
Controllers/     — MVC controllers for each module
Models/          — Data models / entities
Repositories/    — Database access layer (Dapper)
Services/        — Business logic layer
Helpers/         — Utilities (auth, DB connection, session)
Views/           — Razor views
Content/         — CSS and static assets
Scripts/         — JavaScript
```
