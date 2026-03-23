# Local Deployment (Demo) Requirements

## Purpose
Provide a minimal, easy-to-run local demo of StorageManager using Docker Compose. This is for demo/development only — no production concerns.

## Scope
- Backend: Backend/Storage/Storage.Api (ASP.NET Core)
- Frontend: Frontend (Vite)
- Database: Microsoft SQL Server container
- Orchestration: docker-compose

## Minimal requirements
1. Single command to start demo:
   - docker-compose up --build -d
2. Services:
   - db — MSSQL container with a persistent volume
   - api — ASP.NET Core API, configured from environment variables
   - frontend — built static files served by nginx (or dev server)
3. DB connection string provided via environment variable:
   - ConnectionStrings__StorageDatabase
4. Apply migrations automatically on api startup (convenient for demo).

## Default ports (configurable)
- frontend: http://localhost:3000 (host) → 80 (container)
- api: http://localhost:5000 (host) → 5000 (container)
- mssql: 1433 (host) → 1433 (container)

## Important environment variables (demo)
- SA_PASSWORD — example default acceptable for demo (do not use in real production).
- ACCEPT_EULA=Y
- ConnectionStrings__StorageDatabase=Server=db,1433;Database=StorageManager;User Id=sa;Password=Your_password123!;TrustServerCertificate=true;Encrypt=false
- ASPNETCORE_URLS=http://+:5000
- ASPNETCORE_ENVIRONMENT=Development

## Migrations
- For demo convenience, the API should run migrations on startup (call `db.Database.Migrate()` during app start). This avoids manual steps.

## Deliverables for demo
- docker-compose.yml
- Backend/Storage/Dockerfile (multi-stage)
- Frontend/Dockerfile (build + nginx)
- docker-compose.override.example.yml (optional simple overrides)
- Short README section with the three commands below

## Quick commands
- Start demo: docker-compose up --build -d
- View API logs: docker-compose logs -f api
- Stop demo: docker-compose down

## Acceptance criteria (demo)
1. docker-compose brings up all services without manual DB setup.
2. Frontend loads at http://localhost:3000 and can call the API.
3. API responds on http://localhost:5000 (e.g., /swagger or a health endpoint).
4. Data persists between container restarts (volume for MSSQL exists).

## Notes
- This is strictly for demo/local development. Security best practices and production hardening are out-of-scope.
- Use simple example values for secrets while testing locally; do not commit real secrets.