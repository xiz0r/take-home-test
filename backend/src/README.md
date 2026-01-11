## Running the Backend

### Prerequisites
- .NET SDK 10
- Docker (optional, for containerized setup)

### Local setup (without Docker)
To build the backend, navigate to the `src` folder and run:
```sh
dotnet build
```

To run all tests:
```sh
dotnet test
```

To start the main API:
```sh
cd Fundo.Applications.WebApi
dotnet run
```

The following endpoint should return **200 OK**:
```http
GET -> http://localhost:5000/loans
```

### Development mode
Swagger UI is enabled only in Development. Run the API with the environment set to Development:
```sh
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

Swagger UI (Development only) is available at:
```http
GET -> http://localhost:5000/
```

The OpenAPI JSON is available at:
```http
GET -> http://localhost:5000/swagger/v1/swagger.json
```

### Docker setup
From the repository root (where `docker-compose.yml` lives):
```sh
docker compose up --build
```

The API will be available at:
```http
GET -> http://localhost:8080/loans
```

Swagger UI (Development only) is available at:
```http
GET -> http://localhost:8080/
```

The OpenAPI JSON is available at:
```http
GET -> http://localhost:8080/swagger/v1/swagger.json
```

