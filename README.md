# louichungpos

A developing point-of-sale API with ASP.NET Core and EF Core backend, supporting real-time order updates via SignalR.

## EF Core
- Models representing database tables.
- Handles database access and migrations.

## ASP.NET Core
- Controllers for REST API endpoints.
- DTOs for data transfer to/from clients.
- SignalR hubs for real-time communication.

## CI/CD Pipeline (GitHub Actions)
1. Prepare .NET SDK environment
2. Restore dependencies
3. Build solution
4. Create deployable artifact
5. Login to Azure
6. Deploy to Azure App Service

## Azure Integrations
- **Azure App Service**: Hosts ASP.NET Core API.
- **Azure SQL**: Hosts database for EF Core models.
- **Azure AD**: Authentication for deployment and JWT tokens.
- **Azure SignalR Service**: Hosts SignalR hub for real-time updates.
- **Azure Static Web Apps (SWA)**: Hosts Angular frontend.
