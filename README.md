# Dungeon Explorer API

A **C# .NET 8 Web API** for creating and exploring dungeon maps with grid-based pathfinding, paired with an **Aurelia frontend**. The project demonstrates clean architecture, secure design, and containerized deployment.

---

## Table of Contents

* [Overview](#overview)
* [Features](#features)
* [Tech Stack](#tech-stack)
* [Setup and Run](#setup-and-run)
* [API Endpoints](#api-endpoints)
* [Services](#services)
* [Security Flow](#security-flow)
* [Testing](#testing)
* [Docker Deployment](#docker-deployment)
* [SPA Usage](#spa-usage)

---

## Overview

This project allows users to:

* Create a dungeon map with dimensions, start position, goal, and obstacles.
* Compute paths from start to goal, avoiding obstacles (using **A*** or simplified pathfinding).
* Persist dungeon maps in a **SQLite database**.
* Visualize dungeons and computed paths in a simple Aurelia SPA.

The backend is designed with **Clean Architecture principles**, **dependency injection**, and **layered service design**, emphasizing maintainability, security, and testability.

---

## Features

* Minimal, well-designed backend endpoints for dungeon creation, retrieval, and path computation.
* Security features including input validation and role-based access.
* Unit tests covering critical logic and edge cases.
* Containerized deployment with Docker Compose.
* Optional: Token-based authentication for admin operations.

---

## Tech Stack

* **Backend:** .NET 8 (C#), ASP.NET Core Web API
* **Frontend:** Aurelia
* **Database:** SQLite (EF Core)
* **Testing:** xUnit, Moq, FluentAssertions
* **Containerization:** Docker, Docker Compose

---

## Setup and Run

### Prerequisites

* [.NET 8 SDK]
* [Node.js 20+]
* [Docker & Docker Compose]

### Local Backend Setup

```bash
cd backend/DungeonExplorerBackend
dotnet restore
dotnet build
dotnet run
```

API runs by default on `http://localhost:8088`.

### Local Frontend Setup (Aurelia)

```bash
cd frontend/dungeon-ui
npm install
au run --watch
```

Frontend runs by default on `http://localhost:3000`.

---

## API Endpoints

### 1. Create Dungeon

* **POST** `/api/dungeons/create`
* Request body:

```json
{
  "name": "Test Dungeon",
  "width": 10,
  "height": 10,
  "start": { "x": 0, "y": 0 },
  "goal": { "x": 9, "y": 9 },
  "obstacles": [ { "x": 1, "y": 1 }, { "x": 2, "y": 3 } ]
}
```

* Response: Dungeon ID

### 2. Get Dungeon with Solution

* **GET** `/api/dungeons/{id}`
* Response:

```json
{
  "id": 1,
  "name": "Test Dungeon",
  "width": 10,
  "height": 10,
  "start": { "x": 0, "y": 0 },
  "goal": { "x": 9, "y": 9 },
  "obstacles": [...],
  "solutions": {
    "path": [...],
    "computationTimeMs": 12
  }
}
```

---

## Services

The application uses the following services for separation of concerns:

* `DungeonService` – Orchestrates dungeon creation, retrieval, and solution handling.
* `DungeonSolverService` – Solves dungeon paths and delegates to pathfinding algorithms.
* `AStarPathfindingService` – Implements pathfinding logic using A* algorithm.
* `DungeonMapper` – Maps between request/response DTOs and entities.
* `DungeonRepository` – Handles database access for dungeons.
* `UserRepository` – Handles database access for users.
* `AdminSeeder` – Seeds initial admin account in the database.
* `JwtService` – Handles JWT token generation, validation, and authentication.
* `JwtOptionsValidator` – Validates JWT configuration.
* `InputSanitizer` – Cleans user input to prevent XSS and unsafe characters.
* `ResponseHandler` – Standardizes API responses.
* `SecurityFlow` – Handles security validation, authentication, and authorization checks.

---

## Security Flow

Security in the application is implemented via:

1. **Authentication**

   * JWT token generation (`JwtService`)
   * Validation with configuration checks (`JwtOptionsValidator`)

2. **Authorization**

   * Role-based checks (admin vs. regular user) handled in `SecurityFlow` and `DungeonService`

3. **Input Sanitization**

   * `InputSanitizer` ensures safe inputs, preventing XSS or injection

4. **Data Protection**

   * Passwords stored securely with **BCrypt**
   * Token-based authorization for sensitive operations

---

## Testing

Run unit tests with:

```bash
cd backend
dotnet test ./Tests/Tests.csproj
```

Tests cover:

* `AStarPathfindingService` – Pathfinding algorithm logic
* `DungeonService` – Orchestration, creation, and solution handling
* `DungeonSolverService` – Dungeon solving logic
* `InputSanitizer` – Validation of user input
* `SecurityFlow` – JWT authentication, admin authorization, forbidden access
* Repositories – `DungeonRepository` and `UserRepository`
* Response handling – `ResponseHandler`

---

## Docker Deployment

The project is fully containerized using **Docker Compose**.

### 1. Create `.env` file in project root

```env
JWT__Secret=SuperStrongSecret123!VeryLongAndSecureKey2025!!
JWT__Issuer=DungeonApi
JWT__Audience=DungeonApiClient
ADMIN_USERNAME=admin
ADMIN_PASSWORD=StrongPass@2025
```

### 2. Run Docker Compose

```bash
docker-compose up --build
```

### 3. Access Services

* **Backend API:** [http://localhost:8088](http://localhost:8088)
* **Frontend SPA:** [http://localhost:3000](http://localhost:3000)

The SQLite database persists in a Docker volume named `sqlite_data`.

---

## SPA Usage

1. Open the SPA in your browser at [http://localhost:3000](http://localhost:3000).  
2. **Log in** using the admin credentials set in your `.env` file (default: `ADMIN_USERNAME=admin`, `ADMIN_PASSWORD=StrongPass@2025`).  
3. After logging in, fill out the dungeon creation form: width, height, start/goal positions, and obstacles.  
4. Submit the form to create the dungeon.  
5. The map will display in a grid with the computed path highlighted.  
6. Optionally, you can use the API directly via Postman to fetch dungeon details or solutions, but an **authentication token** is required for any restricted endpoints.

---

