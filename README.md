# SimpleServer

**SimpleServer** is a server application that leverages modern technologies to provide a robust and scalable solution. It utilizes CQRS (Command Query Responsibility Segregation), MessagePack for efficient serialization, NetCoreServer for networking, and SQLite for data storage.

## Features

- **CQRS**: Separates the read and write operations to enhance performance and scalability.
- **MessagePack**: Provides fast and compact serialization.
- **NetCoreServer**: A lightweight and high-performance networking library for .NET.
- **SQLite**: A self-contained, serverless database engine used for data storage.

## Getting Started

### Prerequisites

- .NET SDK 8.0
- SQLite
- Entity Framework Core Tools

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/sketcher-git/SimpleServer.git
   ```
2. Restore the project dependencies:
   ```bash
   dotnet restore
   ```
### Database Setup
To create the database, use the following Entity Framework Core command:
```bash
dotnet ef migrations add Create_Database --project Infrastructure --startup-project SimpleServer --context ApplicationWriteDbContext
```
This command will add a migration to create the database schema.
## Usage
After starting the server, you can interact with it according to the CQRS principles and use MessagePack for data serialization.
