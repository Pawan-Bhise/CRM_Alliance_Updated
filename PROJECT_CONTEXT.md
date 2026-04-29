# Project Context - Alliance CRM

## Purpose

`CallCenterSecure` is a legacy ASP.NET MVC CRM application used for call-center operations. It handles inbound and outbound call workflows, ticket/SR tracking, loan/customer records, and admin master data management.

## Core Stack

- Backend: ASP.NET MVC 5, .NET Framework 4.7.2
- Language: C#
- UI: Razor views, jQuery, Bootstrap, DataTables
- Data access: Entity Framework 6 + Dapper
- Database: SQL Server
- Auth: Forms Authentication with custom membership/role providers

## Repository Layout

- `CallCenterSecure.sln` - Visual Studio solution
- `CallCenterSecure/` - Main web application
- `packages/` - NuGet package folder

### Important folders in `CallCenterSecure/`

- `App_Start/` - Route, filter, and bundle setup
- `Controllers/` - Business workflows and endpoints
- `Data/` - EF `ApplicationDbContext`
- `Models/` and `DataAccess/` - Domain and data models
- `Migrations/` - EF6 migration history
- `Repositories/` - Dapper-based SQL repository code
- `CustomAuthentication/` - Custom auth principal/providers
- `Views/` - Razor UI
- `Scripts/`, `Content/` - Frontend assets

## Main Entry Points

- `CallCenterSecure/Global.asax`
- `CallCenterSecure/Global.asax.cs`
- `CallCenterSecure/App_Start/RouteConfig.cs`
- `CallCenterSecure/App_Start/BundleConfig.cs`

Default route redirects to `Account/Login`.

## Key Controllers

- `CallCenterSecure/Controllers/AccountController.cs` - Login/logout/auth cookie
- `CallCenterSecure/Controllers/HomeController.cs` - Dashboard and core flows
- `CallCenterSecure/Controllers/AllianceInboundController.cs` - Inbound operations
- `CallCenterSecure/Controllers/AllianceOutboundController.cs` - Outbound operations
- `CallCenterSecure/Controllers/AdminController.cs` - User and master-data admin

## Data Layer Notes

- EF6 context: `CallCenterSecure/Data/ApplicationDbContext.cs`
- Migrations: `CallCenterSecure/Migrations/*.cs`
- Hybrid pattern:
  - EF for standard CRUD/domain operations
  - Dapper for custom SQL/reporting (`CallCenterSecure/Repositories/CustomerRepository.cs`)

## Configuration

- Main config: `CallCenterSecure/Web.config`
- Build transforms:
  - `CallCenterSecure/Web.Debug.config`
  - `CallCenterSecure/Web.Release.config`
- Publish profile:
  - `CallCenterSecure/Properties/PublishProfiles/FolderProfile.pubxml`

No Docker, Compose, or GitHub Actions workflow was identified in this snapshot.

## Contributor Gotchas

- Legacy `packages.config` + old-style `.csproj` (not SDK-style)
- Mixed EF and Dapper means schema/query changes may require updates in multiple places
- `Web.config` includes sensitive-looking settings; treat config values carefully
- `bin/` and `obj/` artifacts may exist locally; use source files as ground truth

## Suggested Reading Order

1. `CallCenterSecure/CallCenterSecure.csproj`
2. `CallCenterSecure/Web.config`
3. `CallCenterSecure/Global.asax.cs`
4. `CallCenterSecure/App_Start/RouteConfig.cs`
5. `CallCenterSecure/Data/ApplicationDbContext.cs`
6. `CallCenterSecure/Controllers/AccountController.cs`
7. `CallCenterSecure/Controllers/AllianceInboundController.cs`
8. `CallCenterSecure/Repositories/CustomerRepository.cs`

## Last Updated

2026-04-29
