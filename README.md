# 📝 NotesApp API

A secure Notes Management Web API built with **.NET 9** and **PostgreSQL**, featuring JWT authentication, role-based authorization, and comprehensive exception handling.

---

## ✨ Features

- 🔐 JWT Authentication with HttpOnly cookies
- 👥 Role-Based Authorization (User, Admin)
- 📝 Full CRUD operations for Notes
- 🎨 Customizable note colors (Yellow, Blue, Grey)
- 🔒 Secure password hashing with BCrypt
- 🛡️ Global exception handling
- 📊 User management with pagination (Admin only)
- 🗄️ Database indexes for optimized queries
- 📘 Swagger/OpenAPI documentation
- 🐳 Docker support

---

## 🛠️ Tech Stack

- **.NET 9**
- **PostgreSQL**
- **Entity Framework Core 9**
- **JWT Bearer Authentication**
- **BCrypt.Net-Next**
- **Swagger/OpenAPI**
- **Npgsql.EntityFrameworkCore.PostgreSQL**

---

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- PostgreSQL 12+

### Setup

1. **Create `appsettings.Development.json`:**

```json
{
  "ConnectionStrings": {
    "NotesDbConnectionString": "Host=localhost;Port=5432;Database=notesdb;Username=postgres;Password=your_password;"
  },
  "Jwt": {
    "Key": "YOUR_BASE64_ENCODED_SECRET_KEY",
    "Issuer": "NotesApp",
    "Audience": "NotesAppUsers",
    "ExpiresInHours": 24
  },
  "Admin": {
    "Email": "admin@notesapp.com",
    "Password": "Admin@123",
    "Name": "Admin User"
  }
}
```

**Generate JWT Key:**
```bash
# PowerShell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

2. **Run migrations and start:**

```bash
cd Notes.API
dotnet restore
dotnet ef migrations add InitialCreate --context NoteDBContext
dotnet ef database update --context NoteDBContext
dotnet run
```

3. **Access Swagger:**
```
https://localhost:7277/swagger
```

---

## 📚 API Endpoints

### Authentication
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/auth/signup` | Register new user | ❌ |
| `POST` | `/api/auth/signin` | Login | ❌ |
| `GET` | `/api/auth/me` | Get current user | ✅ |
| `POST` | `/api/auth/logout` | Logout | ✅ |
| `GET` | `/api/auth/admin/users` | Get all users (paginated) | ✅ Admin |

### Notes
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/note` | Create note | ✅ |
| `GET` | `/api/note` | Get user's notes | ✅ |
| `GET` | `/api/note/{id}` | Get note by ID | ✅ |
| `PUT` | `/api/note/{id}` | Update note | ✅ |
| `DELETE` | `/api/note/{id}` | Delete note (soft) | ✅ |

---

## 🔐 Authentication

After signup/signin, the JWT token is stored in an **HttpOnly cookie** (`auth_token`). 

For API clients (Swagger/Postman), use the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

**Token Claims:**
- `id`: User GUID
- `email`: User email
- `role`: User role (User/Admin)

---

## 🗄️ Database

### Schema
- **User**: Users table with email, password hash, and soft delete
- **Role**: Roles table (User, Admin)
- **UserRole**: Many-to-many junction table
- **Note**: Notes table with user relationship and soft delete

### Indexes
- `User.Email` (Unique)
- `User.CreatedAt`
- `Note.UserId`
- `Note(UserId, UpdatedAt)` (Composite)
- `UserRole.UserId`
- `UserRole.RoleId`
- `Role.Name` (Unique)

### Data Seeding
The application automatically seeds:
- **Roles**: User, Admin
- **Admin User**: From `appsettings.Development.json` or environment variables

---

## 🔧 Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `POSTGRES_CONNECTION_STRING` | Database connection string | ✅ |
| `JWT_KEY` | Base64-encoded JWT secret | ✅ |
| `ADMIN_EMAIL` | Admin email | ❌ |
| `ADMIN_PASSWORD` | Admin password | ❌ |
| `ADMIN_NAME` | Admin name | ❌ |

**Priority**: Environment variables override `appsettings.json` values.

---

## 📁 Project Structure

```
Notes.API/
├── Common/
│   ├── Dtos/              # ErrorResponseDto, PagedResponseDto
│   ├── Exceptions/        # Custom exceptions
│   └── Middleware/        # GlobalExceptionHandlerMiddleware
├── Infrastructure/
│   ├── Data/              # DataSeeder
│   ├── DBContext/         # NoteDBContext
│   ├── Models/            # User, Note, Role, UserRole
│   └── Migrations/        # EF Core migrations
└── Modules/
    ├── Auth/              # Authentication module
    │   ├── Controllers/
    │   ├── Dtos/
    │   ├── Interfaces/
    │   ├── Providers/     # TokenProvider, HashProvider
    │   ├── Repositories/
    │   ├── Services/
    │   └── Settings/
    └── Notes/             # Notes module
        ├── Controllers/
        ├── Dtos/
        ├── Enums/          # Colors
        ├── Interfaces/
        ├── Repositories/
        └── Services/
```

---

## 🐳 Docker

```bash
docker build -t notesapp-api .
docker run -p 8080:8080 \
  -e POSTGRES_CONNECTION_STRING="your_connection_string" \
  -e JWT_KEY="your_jwt_key" \
  notesapp-api
```

---

## 🧪 Testing

### Using Swagger
1. Open `https://localhost:7277/swagger`
2. Use `/api/auth/signup` or `/api/auth/signin` to authenticate
3. Copy the JWT token from the response
4. Click "Authorize" and paste: `Bearer <token>`

### Example Requests

**Sign Up:**
```json
POST /api/auth/signup
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePass123"
}
```

**Create Note:**
```json
POST /api/note
{
  "title": "My First Note",
  "content": "This is the note content",
  "backgroundColor": "YELLOW"
}
```

---

## 🔒 Security Features

- HttpOnly cookies for token storage
- BCrypt password hashing
- JWT token validation
- Role-based authorization policies
- Soft delete for data retention
- Global exception handling
- CORS configuration

---

## 📝 Notes Features

- **Colors**: Yellow (default), Blue, Grey
- **Soft Delete**: Notes are marked as deleted, not removed
- **User Isolation**: Users can only access their own notes
- **Ordering**: Notes sorted by `UpdatedAt` (descending)

---

## 👤 Author

**Youssef Marzouk** - Developed for learning, portfolio, and backend practice.
