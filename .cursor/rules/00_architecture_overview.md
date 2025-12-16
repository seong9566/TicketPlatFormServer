---
trigger: always_on
---

# Project Architecture Overview

## Layered Architecture
This project uses a layered architecture to separate concerns and improve maintainability:

- **Controllers**: Handle HTTP requests and responses. They acts as the entry point for the API.
- **Services**: Handle business logic, validation, and orchestrate operations. They convert DTOs to Entities and vice versa.
- **Repositories**: Directly interact with the database via EF Core or Dapper. They are responsible for data persistence.
- **DTOs (Data Transfer Objects)**: Used `only` at the Controller/Service layer boundary. They define the contract for API requests and responses.
- **Entities**: Used `only` in the Repository layer and below. They represent the database schema.

## Core Principles
- **Separation of Concerns**: Each layer has a distinct responsibility.
- **Dependency Flow**: Controllers -> Services -> Repositories.
- **Data Flow**: DTOs (Controller <-> Service) | Entities (Service <-> Repository <-> DB).
