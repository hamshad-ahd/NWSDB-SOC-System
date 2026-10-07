#+ NWSDB-SOC-System: National Water Supply and Drainage Board System

An enterprise-grade, distributed Service-Oriented Computing (SOC) system built with **.NET 8**, **Entity Framework Core**, **Docker**, and **Kubernetes**.

---

## 📖 Complete Documentation

- 📘 **[Full System Documentation][Full System Documentation](docs/SYSTEM_DOCUMENTATION.md)** — Architectural design, ER diagrams, 12-step transfer lifecycle, REST API catalog, security models, test results, and deployment guides.
- 📙 **[Third-Party Payment & Transfer Architecture][Third-Party Payment & Transfer Architecture](docs/THIRD_PARTY_TRANSFER_ARCHITECTURE.md)** — Detailed specification of the two-stage separation of concerns (Counter Collection vs NWSDB Settlement Transfer).

---

## 🏗️ Architecture & Component Topology

| Service | Technology | Port | Documentation / UI |
| :--- | :--- | :--- | :--- |
| **`Bank.Api`** | ASP.NET Core 8 Web API | `5001` | [Swagger UI](http://localhost:5001/swagger) |
| **`NWSDB.Api`** | ASP.NET Core 8 Web API | `5002` | [Swagger UI](http://localhost:5002/swagger) |
| **`NWSDB.Website`** | ASP.NET Core 8 MVC | `5003` | [Customer Portal](http://localhost:5003) |
| **`ThirdParty.WebApp`** | ASP.NET Core 8 MVC | `5004` | [Agent Collection Terminal](http://localhost:5004) |
| **`NWSDB.Tests`** | xUnit & In-Memory SQLite | CLI | 100% Passing Automated Suite |

---

## 🚀 Quick Start

### 1. Run via .NET CLI (Local Development)
```bash
# Terminal 1: Core Banking API
dotnet run --project Bank.Api

# Terminal 2: NWSDB Core API
dotnet run --project NWSDB.Api

# Terminal 3: Customer Portal
dotnet run --project NWSDB.Website

# Terminal 4: Third-Party Agent WebApp
dotnet run --project ThirdParty.WebApp
```

### 2. Run with Docker Compose
```bash
docker-compose up -d --build
```

### 3. Deploy to Kubernetes
```bash
kubectl apply -f k8s/bank-api-deployment.yaml
kubectl apply -f k8s/nwsdb-api-deployment.yaml
kubectl apply -f k8s/nwsdb-website-deployment.yaml
kubectl apply -f k8s/thirdparty-webapp-deployment.yaml
```

### 4. Execute Automated Tests
```bash
dotnet test
```

---

## 🧪 Pre-Seeded Test Credentials

### Customer Login (`NWSDB.Website`)
- **Account Number**: `NWSDB-1001`
- **Customer Name**: `Kavindu Perera`

### Valid Card for Testing
- **Card Number**: `45327...........`
- **Cardholder**: `Kavindu Perera`
- **Expiry**: `12/28`
- **CVV**: `123`
