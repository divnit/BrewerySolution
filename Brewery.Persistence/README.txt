# Brewery API 🍺

A sample ASP.NET Core Web API that integrates with the [Open Brewery DB](https://www.openbrewerydb.org/) and stores breweries in a local **SQLite database**.

## Features
- ASP.NET Core 8 Web API
- Entity Framework Core (SQLite)
- Auto-seeding from Open Brewery DB
- API Versioning (`v1`)
- Swagger UI for testing
- Serilog logging

## 🚀 Getting Started

### 1. Clone the repo
git clone https://github.com/divnit/BrewerySolution
cd BrewerySolution


cd Brewery.Api
dotnet ef migrations add InitialCreate --project ../Brewery.Persistence --startup-project .
dotnet ef database update --project ../Brewery.Persistence --startup-project .

dotnet run --project Brewery.Api


Swagger UI

HTTPS → https://localhost:7000/swagger
HTTP → http://localhost:5094/swagger

Endpoints (v1)
GET /api/v1/breweries → list all breweries
GET /api/v1/breweries/{id} → get brewery by id
GET /api/v1/breweries/autocomplete?term=lag → search breweries
GET /api/v1/breweries/random → random brewery


By default uses SQLite file: breweries.db
To reset database:

dotnet ef database drop --project ../Brewery.Persistence --startup-project .
dotnet ef database update --project ../Brewery.Persistence --startup-project .


=====================================================

From scatch 

1) Create Projects

# Web API
dotnet new webapi -n Brewery.Api

# Class Library for Application Layer
dotnet new classlib -n Brewery.Application

# Class Library for Persistence (EF Core + SQLite)
dotnet new classlib -n Brewery.Persistence

2) Add Projects to Solution

dotnet sln add Brewery.Api/Brewery.Api.csproj
dotnet sln add Brewery.Application/Brewery.Application.csproj
dotnet sln add Brewery.Persistence/Brewery.Persistence.csproj

3) Add Project References

dotnet add Brewery.Api/Brewery.Api.csproj reference Brewery.Application/Brewery.Application.csproj
dotnet add Brewery.Api/Brewery.Api.csproj reference Brewery.Persistence/Brewery.Persistence.csproj
dotnet add Brewery.Persistence/Brewery.Persistence.csproj reference Brewery.Application/Brewery.Application.csproj

4) Install Required Packages

# EF Core & SQLite
dotnet add Brewery.Persistence package Microsoft.EntityFrameworkCore.Sqlite
dotnet add Brewery.Persistence package Microsoft.EntityFrameworkCore.Design

# ASP.NET Core versioning & swagger
dotnet add Brewery.Api package Microsoft.AspNetCore.Mvc.Versioning
dotnet add Brewery.Api package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
dotnet add Brewery.Api package Swashbuckle.AspNetCore

# Logging & Resilience
dotnet add Brewery.Api package Serilog.AspNetCore
dotnet add Brewery.Api package Serilog.Sinks.Console
dotnet add Brewery.Api package Polly


