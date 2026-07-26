# Event Reservation API

## How to start the project
Run the following command in the root directory (where the `docker-compose.yml` file is located):
```bash
docker compose up --build
```
This command will automatically start both the .NET API and the SQL Server database containers. 
Once the containers are running, you can explore and test the endpoints via the Swagger UI available at:
`http://localhost:8080/swagger`

## Database Migrations & Startup
Database migrations are applied automatically on application startup. To ensure the API does not fail if the database container takes time to boot, the project utilizes **Docker Healthchecks**. The API container is configured to wait (`depends_on: service_healthy`) until the SQL Server is fully initialized and ready to accept connections before starting the application and applying migrations.

## Architecture decisions
* **Type of Id property:** I used `int` instead of the more complex `Guid` for the `Id` of `Event`. This simplifies testing, improves database index performance, and is perfectly sufficient for a basic CRUD API.
* **Minimal API architecture:** I used the basic syntax for Minimal API directly in `Program.cs` instead of introducing more complex abstractions like a Service Layer. This decision was made to keep the code as simple and readable as possible, following the assignment's core principles.
* **Data Transfer Objects (DTOs):** Incoming request payloads are separated from database entities using `InputEventDto` to prevent over-posting vulnerabilities.
* **Validation & Error Handling:** The API uses `FluentValidation` to strictly validate incoming requests. Any unexpected server errors are caught by a modern `IExceptionHandler` and returned as standard `application/problem+json` Problem Details responses, ensuring no sensitive stack traces are leaked.
* **Structured Logging:** Key business operations and potential validation failures are logged with structured properties (e.g., `{EventId}`) for easier monitoring and debugging.

## Future improvements 
* **Advanced Filtering & Pagination:** Implement filtering by name, date, and location, along with pagination for the `GET /events` endpoint to handle large datasets efficiently.
* **Route Organization:** Use `MapGroup` to prevent code duplication in endpoint paths and to better organize the API structure.
* **Time Types:** Replace `DateTime` with `DateTimeOffset` in the `Event` model to correctly store event times with timezone awareness.