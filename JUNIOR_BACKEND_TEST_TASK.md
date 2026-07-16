# Junior Backend Developer — Test Assignment

## Event Reservation API

Welcome! This assignment is designed to give you a realistic taste of backend development with ASP.NET Core and to give us a picture of how you learn, think, and work. You have approximately **2-3 weeks** to complete it.

We know you may not have practical ASP.NET Core experience yet — that is expected and completely fine. Part of this assignment is precisely about seeing how you approach an unfamiliar technology. You are **encouraged to use official documentation, tutorials, and any other learning resources** while working on the task.

A few principles before you start:

- **Simple, readable, working code is preferred over clever or heavily abstracted code.** You do not need to apply any specific architectural patterns. A well-organized project that a reviewer can read top to bottom is the goal.
- **Honesty beats polish.** If something does not work or is unfinished, document what you tried, where you got stuck, and what you would do next. A clearly described incomplete attempt is worth far more to us than a silently broken feature.
- **We care about your reasoning.** The README you write is as important as the code itself.

---

## What We Are Looking For

- Your ability to learn an unfamiliar technology independently.
- Solid C# fundamentals.
- Basic backend and database thinking: data modeling, validation, error handling.
- Attention to detail and engagement with the task.
- A project that we can start and explore without friction.
- Clear explanations of the decisions you made.

---

## The Domain

You will build a backend API for managing **events** (concerts, meetups, workshops — anything with a location, a start time, and a limited number of seats). The mandatory part focuses on event management. The additional challenges extend the system with filtering, authentication, and seat reservations.

There is **no UI** — this is an API-only project.

---

## Technology Requirements

You must use:

- **C#** on **.NET 9 or newer**.
- **ASP.NET Core Minimal API** (endpoints defined with `MapGet`, `MapPost`, etc. — not MVC controllers).
- **EF Core** for data access.
- **SQL Server** as the database.
- **Structured logging** — the built-in `ILogger` is enough; you may use another logging library (e.g., Serilog) if you prefer.
- **Docker and Docker Compose**.

### The most important operational requirement

The entire application — API **and** database — must start with a single command from the repository root:

```bash
docker compose up --build
```

After this command finishes, the API must be reachable and fully functional. The reviewer must **not** need to install a database, run scripts by hand, or perform any manual setup steps. If any additional step is unavoidable, it must be clearly documented in the README — but aim for zero.

---

## Part 1 — Mandatory Functionality

This part is required. Complete it before spending time on the additional challenges.

### 1. Event Management

Provide API endpoints to:

- Create an event.
- Get a single event by its ID.
- Update an event.
- Delete an event.
- Get a list of events.

### 2. Event Model

An event must contain at least the following data:

| Field | Notes |
|---|---|
| `Id` | Unique identifier |
| `Name` | Name of the event |
| `Description` | Free-text description |
| `Location` | Where the event takes place |
| `StartsAt` | When the event starts |
| `Capacity` | Total number of seats |
| `CreatedAt` | When the event record was created |

You may add more fields if your design needs them. Think about which fields should be required, and what values make sense. Document your validation decisions.

### 3. Database

- Use a relational database (SQL Server).
- Manage the schema with **SQL Scripts migrations**.
- Add appropriate constraints (and relationships, once you have more than one entity).
- All application data must be persisted in the database — no in-memory storage.
- The database must run inside Docker Compose; do not require any manually installed database.

### 4. API Quality

- **Validate incoming requests.** Invalid input must not reach the database.
- **Return appropriate HTTP status codes** — for example, distinguish between a successful creation, a validation failure, a missing resource, and an unexpected server error.
- **Handle unexpected errors consistently.** An unhandled exception should not leak a raw stack trace to the client; the API should return a consistent error response.

### 5. Logging

- Log important business operations.
- Log unexpected errors.
- Use **structured logging**: include useful identifiers (such as the event ID) as named log properties, not just as text glued into the message string.

### 6. README

Your repository must include a `README.md` that explains:

- How to start the application (the exact commands).
- How database migrations are applied.
- The most important architecture and implementation decisions you made, and why.
- Known limitations of your solution.
- What you would improve or do differently if you had more time.

Write the README for a reviewer who has never seen your project before.

---

## Part 2 — Additional Challenges

The challenges below are **not required** to submit the assignment, but they are where you can really show engagement and depth. They increase in difficulty.

**Important:** an incomplete challenge is perfectly acceptable — as long as you honestly document in the README what you attempted, what works, what does not, and what problems remain. Do not hide unfinished work; describe it.

### Challenge 1 — Filtering, Pagination, and Sorting

Extend the event list endpoint to support:

- Filtering by partial event name (e.g., searching for `"rock"` finds `"Rock Festival 2026"`).
- Filtering by location.
- Filtering by a date range (events starting between two dates).
- Pagination.
- Sorting by event name or by start date.

Filters should be combinable in a single request.

As part of this challenge, **consider database performance**: think about how your queries will behave as the number of events grows. If you add any database indexes, explain in the README which ones you added and why. We are deliberately not telling you which indexes to create — the reasoning is the interesting part.

### Challenge 2 — Authentication and Authorization

Add user accounts and role-based access:

- User registration and login.
- **JWT-based authentication.**
- Two roles with different permissions:
  - **Regular users** can view events and manage only their own reservations (see Challenge 3; if you have not implemented reservations, regular users are simply read-only for events).
  - **Administrators** can create, update, and delete events.

We do not prescribe a specific identity implementation — choose an approach you can understand and explain. Document how a reviewer can obtain a token and call protected endpoints (for example, whether any test users are seeded).

### Challenge 3 — Temporary Seat Reservations

This is the most demanding challenge. It touches on background processing, transactions, concurrency, and careful database design. We intentionally leave the final approach entirely up to you.

Requirements:

- A user can reserve one or more seats for an event.
- A reservation has a status: `Pending`, `Confirmed`, `Expired`, or `Cancelled`.
- A newly created reservation starts as `Pending`.
- A pending reservation **temporarily reduces the number of available seats** for the event.
- A pending reservation can be **confirmed** by the user.
- A pending reservation that is not confirmed in time must **expire automatically**.
- Seats held by expired or cancelled reservations become available again.
- The expiration period must be **configurable**, and must support short values (such as **30 seconds**) so the behavior can be demonstrated during a review.
- Expiration must work correctly **even after the application restarts**. Creating one in-memory timer per reservation is not a sufficient solution.
- The system must **never accept reservations that exceed the event's capacity — including when multiple requests arrive at the same time**. Think carefully about what happens when two users try to reserve the last seat simultaneously.

We are not telling you how to implement expiration or concurrency handling — there are several valid approaches with different trade-offs. Whatever you choose, explain in the README how it works, why you chose it, and what its limitations are.

---

## Optional Bonus Ideas

If you finish early and want to go further, any of the following are welcome. Pick what interests you — none of these are expected:

- Automated tests (unit tests for your logic).
- Integration tests against a real or containerized database.
- Health check endpoints.
- Request correlation IDs in logs.
- Problem Details (`application/problem+json`) error responses.
- Proper `CancellationToken` usage in endpoints and database calls.
- Graceful shutdown of background services.
- Idempotent reservation confirmation (confirming the same reservation twice is safe).
- Caching with Redis or in-memory caching.
- Cache invalidation when events or reservations change.
- Rate limiting.

---

## Practical Notes

- **Scope your time.** A complete, working, well-documented mandatory part is the foundation. Only then move to the challenges, in order.
- **Commit as you go.** A commit history that shows your progression is helpful (and normal); do not squash everything into one commit at the end.
- **Ask if truly blocked.** If a requirement is genuinely ambiguous, you may ask us — but for small decisions, it is better to make a reasonable choice yourself and document it in the README.
- **Using external documentation, tutorials, and searches is allowed and expected.** What matters is that you understand and can explain everything you submit — be prepared to walk through your code and decisions in a follow-up conversation.

---

## How to Submit

Share your solution as a Git repository (for example, a private GitHub repository with access granted to us, or a public one). Make sure the repository contains everything needed to run the project — including the `docker-compose.yml` and the README.

---

## Submission Checklist

Before submitting, verify:

- [ ] `docker compose up --build` starts the API and the database from a clean checkout with no manual steps.
- [ ] All mandatory event CRUD endpoints work.
- [ ] Data is persisted in the database and the schema is managed by EF Core migrations.
- [ ] Invalid requests are rejected with appropriate status codes and clear error responses.
- [ ] Unexpected errors return a consistent error response, not a raw stack trace.
- [ ] Swagger is available and documents all endpoints.
- [ ] Important operations and errors are logged with structured properties.
- [ ] The README covers: startup, Swagger URL, migrations, key decisions, known limitations, and future improvements.
- [ ] Any attempted additional challenges are documented — including what is incomplete and why.
- [ ] You can explain every part of the code you are submitting.

Good luck — we are looking forward to seeing your work!
