# NWSDB Service-Oriented Computing System

A distributed Service-Oriented Computing (SOC) system developed for the National Water Supply and Drainage Board (NWSDB) use case.

The system consists of independent web applications and REST APIs that communicate through service interfaces, demonstrating service-oriented architecture, API integration, database separation, payment processing, and containerized deployment.

## 📌 Project Overview

The NWSDB SOC System provides a digital platform for managing customer water services, bills, payments, third-party collections, and settlement transfers.

The system is designed around independently deployable services with clear separation between the customer portal, third-party collection application, NWSDB API, and banking API.

## ✨ Key Features

- Customer login and account management
- View current water bills
- View water usage information
- View payment history
- Online card payment
- Third-party customer bill lookup
- Cash and card collection through third-party agents
- Payment receipt generation
- Settlement transfer from third-party collectors to NWSDB
- REST API-based communication between services
- Separate databases for banking and NWSDB operations
- Automated testing
- Docker containerization
- Kubernetes deployment configuration
- Swagger API documentation

## 🏗️ System Architecture

The system contains four main independently deployable applications and a test project.

| Component | Technology | Port | Purpose |
|---|---|---:|---|
| **Bank.Api** | ASP.NET Core 8 Web API | 5001 | Banking and transaction services |
| **NWSDB.Api** | ASP.NET Core 8 Web API | 5002 | Core NWSDB business services |
| **NWSDB.Website** | ASP.NET Core 8 MVC | 5003 | Customer web portal |
| **ThirdParty.WebApp** | ASP.NET Core 8 MVC | 5004 | Third-party collection application |
| **NWSDB.Tests** | xUnit + SQLite | CLI | Automated testing |

### Service Communication

```text
                         ┌──────────────────────┐
                         │   NWSDB.Website      │
                         │   Customer Portal    │
                         └──────────┬───────────┘
                                    │
                                    ▼
                         ┌──────────────────────┐
                         │      NWSDB.Api       │
                         │   Core NWSDB API     │
                         └───────┬───────┬──────┘
                                 │       │
                                 │       ▼
                                 │  ┌───────────────┐
                                 │  │   Bank.Api    │
                                 │  │ Banking API   │
                                 │  └───────┬───────┘
                                 │          │
                                 ▼          ▼
                         ┌────────────┐  ┌────────────┐
                         │ NWSDB DB   │  │  Bank DB   │
                         │  SQLite    │  │   SQLite   │
                         └────────────┘  └────────────┘

                         ┌──────────────────────┐
                         │ ThirdParty.WebApp    │
                         │ Collection Terminal  │
                         └──────────┬───────────┘
                                    │
                                    ▼
                              ┌───────────┐
                              │ NWSDB.Api │
                              └───────────┘
