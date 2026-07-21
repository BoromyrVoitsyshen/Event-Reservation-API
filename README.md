# Event Reservation API

## How to start the project
Run the following command in the root directory (where the `docker-compose.yml` file is located):
```bash
docker compose up --build
```
This command will automatically start both the .NET API and the SQL Server database containers. 
You can test the endpoints using `.http` files in Visual Studio or VS Code (targeting `http://localhost:8080`).

## Database Migrations
Database migrations are applied automatically on application startup. To ensure the API waits for the SQL Server container to fully initialize before attempting to apply migrations in `Program.cs` currently implemented a workaround, preventing startup crashes.

## Architecture decisions
* **Type of Id property:** I used `int` instead of the more complex `Guid` for the `Id` of `Event`. This simplifies testing, improves database index performance, and is perfectly sufficient for a basic CRUD API.
* **Minimal API architecture:** I used the basic syntax for Minimal API directly in `Program.cs` instead of introducing more complex abstractions like a Service Layer. This decision was made to keep the code as simple and readable as possible, following the assignment's core principles.

## Future improvements (If I had more time)
* **DTO (Data Transfer Objects):** Separate database entities from incoming request payloads to prevent over-posting vulnerabilities (e.g., implementing `CreateEventDto`).
* **Validation:** Integrate the `FluentValidation` library to validate incoming data (e.g., ensuring `Capacity` is greater than 0) and return standardized `400 Bad Request` errors.
* **Time Types:** Replace `DateTime` with `DateTimeOffset` in the `Event` model to correctly store event times with timezone awareness.
* **Route Organization:** Use `MapGroup` to prevent code duplication in endpoint paths and to better organize the API structure.