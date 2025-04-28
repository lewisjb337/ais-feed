# AIS Feed

**AIS Feed** is a high-performance backend service built with **.NET 8 Web API**.  
It fetches AIS (Automatic Identification System) vessel data from an external AIS API and stores it into a **Supabase** database.  
The service is optimized for speed, reliability, and scalability, serving as a crucial data ingestion layer for marine tracking applications.

---

## Features

- 🚢 Fetches real-time AIS vessel data
- 🛢️ Maps AIS fields (MMSI, IMO, dimensions, etc.) into a normalized database structure
- ⚡ Efficient and asynchronous HTTP requests
- 🧹 Data sanitization and validation
- 🗄️ Stores records securely into **Supabase** (PostgreSQL)
- 🔄 Supports batch processing and easy scheduling (e.g., with a CRON job or Windows Service)
- 🛠️ Written in modern C# (.NET 8) with clean architecture principles

---

## Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 8)
- **Database:** Supabase (PostgreSQL)
- **Language:** C#
- **HTTP Client:** `HttpClientFactory` for dependency injection and efficiency

---

## Usage

- The API automatically pulls AIS data from the configured external service.
- Vessel data is mapped and inserted into your Supabase database.
- You can integrate this service in several ways:
  - **Scheduled Task**: Use CRON jobs, Hangfire, or Azure Functions to trigger the data pull periodically.
  - **Background Service**: Wrap the logic inside a hosted background service that runs on a timer.
  - **Microservice**: Deploy it as a standalone service in a microservices architecture.
- Extend easily by:
  - Adding filters (e.g., specific regions or vessel types)
  - Handling retries and error management
  - Expanding data models as needed

---

## Why I Built AIS Feed

AIS Feed was developed to:

- Build a scalable and modular vessel tracking ingestion service
- Demonstrate backend expertise with **.NET 8** and **cloud databases**
- Provide reliable vessel data for frontend applications (such as **FleetKonnect** and others)
- Showcase practical integration between external APIs and managed cloud databases (Supabase)
- Solve the problem of gathering structured, real-time AIS data for marine-focused applications

---

## License

This project is open-source under the [MIT License](LICENSE).
