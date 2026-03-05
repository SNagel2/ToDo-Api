# ToDo-Backend

.NET Core 10 API with health endpoint for ToDo application.

## Features

- Health endpoint at `/api/health` that returns `true`
- CORS enabled for Angular frontend (http://localhost:4200)
- ASP.NET Core Web API with controllers

## Prerequisites

- .NET 10 SDK

## Running the Application

1. Navigate to the project directory:
   ```bash
   cd ToDo-Backend
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. The API will be available at:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`

4. Test the health endpoint:
   ```bash
   curl http://localhost:5000/api/health
   ```

## Endpoints

- `GET /api/health` - Returns health status (true)

