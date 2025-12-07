# 📝 NotesApp API

A simple and secure Notes Web API built with **.NET 9** and **PostgreSQL**, providing user authentication and basic notes management.

---

## 📋 Prerequisites
Before running the project, ensure you have:

- **.NET 9 SDK**
- **PostgreSQL** installed and running
- A valid connection string
- A valid JWT secret key (stored in `appsettings.Development.json` or environment variables)

---

## 🚀 How to Run

### 1️⃣ Create a `appsettings.Development.json` file:
```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY",
    "Issuer": "NotesApp",
    "Audience": "NotesAppUsers",
    "ExpiresInHours": 24
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=NotesApp;Username=postgres;Password=YOUR_PASSWORD"
  }
}

### 2️⃣  Restore dependencies:

dotnet restore

### 3️⃣ Run the API:

dotnet run

### 📘 Swagger (API Documentation):

Swagger UI is enabled automatically in Development mode.

After running the app, open:
https://localhost:7277/swagger


### 👤 Author:

Developed by Youssef Marzouk For learning, portfolio, and backend practice.