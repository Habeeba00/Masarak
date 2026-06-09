# Phase 2 Documentation — Masarak Education API

## Table of Contents
1. Architecture Overview
2. Authentication Flow
3. Authorization Flow
4. Identity Design Decision
5. JWT Design
6. Refresh Token Flow
7. Security Decisions
8. Database Changes
9. API Endpoints Reference
10. Phase 2 Testing Guide
11. Future Improvements

---

## 1. Architecture Overview

```
Masarak.sln
├── 1. Core/
│   ├── Masarak.Domain/               # Entities (e.g., RefreshToken, User), Constants (AppRoles)
│   └── Masarak.Application/          # Interfaces (IAuthService), DTOs
├── 2. Infrastructure/
│   └── Masarak.Infrastructure/       # Context, Migrations, Seeders, Services (AuthService, JwtService)
└── 3. Presentation/
    └── Masarak.API/                  # Controllers, Policies, Extensions, Program.cs
```

**Design approach**: Clean Architecture. The Domain is completely independent. The Application layer holds business contracts and DTOs. Infrastructure implements persistence (EF Core) and external concerns (Auth implementation). The Presentation (API) wires dependencies together and exposes endpoints.

---

## 2. Authentication Flow

```
Client                          AuthController              AuthService             DB
  │                                   │                          │                   │
  │── POST /api/auth/register ────────►│                          │                   │
  │                                   │── RegisterAsync() ───────►│                   │
  │                                   │                          │── Check email ────►│
  │                                   │                          │── Hash password    │
  │                                   │                          │── Save User ──────►│
  │                                   │                          │── Create profile ──►│
  │                                   │                          │── Save RT ─────────►│
  │◄─ 200 { accessToken, refreshToken }─────────────────────────────────────────────  │
  │                                   │                          │                   │
  │── POST /api/auth/login ───────────►│                          │                   │
  │                                   │── LoginAsync() ──────────►│                   │
  │                                   │                          │── Check lockout    │
  │                                   │                          │── Verify password  │
  │                                   │                          │── Check IsActive   │
  │                                   │                          │── Issue tokens ────►│
  │◄─ 200 { accessToken, refreshToken }──────────────────────────────────────────────  │
```

---

## 3. Authorization Flow

```
Client (with Bearer token)      ASP.NET Core Middleware         Controller
  │                                   │                              │
  │── GET /api/admin/dashboard ───────►│                              │
  │   Authorization: Bearer <JWT>      │                              │
  │                                   │── Validate JWT signature      │
  │                                   │── Check expiry                │
  │                                   │── Extract claims              │
  │                                   │── Evaluate [Authorize(Policy="AdminOnly")]
  │                                   │── role claim == "Admin"? ─────►│
  │◄─ 200 OK ─────────────────────────────────────────────────────────│
```

---

## 4. Identity Design Decision

### Decision: Custom User entity (NOT ASP.NET Core Identity IdentityUser)

**Rationale:**

| Factor | ASP.NET Core Identity | Custom (chosen) |
|---|---|---|
| Existing schema | Breaks Phase 1 tables | Preserves all Phase 1 work |
| Flexibility | Fixed 20+ Identity columns | Only columns you need |
| Migration complexity | Requires renaming/merging tables | Single new `refresh_tokens` table |
| JWT generation | Still need custom service | Clean, explicit control |
| Password hashing | BCrypt via Identity | PBKDF2-SHA512, OWASP compliant |
| Lockout | Built-in | Implemented in AuthService |

ASP.NET Core Identity is excellent for scaffolding from scratch. When a schema already
exists and is in production, integrating Identity requires significant table restructuring.
The custom approach gives equivalent security with zero migration risk.

---

## 5. JWT Design

### Token Claims

```json
{
  "sub":    "42",
  "email":  "teacher@school.com",
  "name":   "Ahmed Hassan",
  "jti":    "uuid-ties-AT-to-RT",
  "role":   "Teacher",
  "userid": "42",
  "nbf":    1717459200,
  "exp":    1717462800,
  "iss":    "MasarakApi",
  "aud":    "MasarakClients"
}
```

- **`sub` / `userid`**: Both set so standard and custom claim readers work
- **`jti`**: Unique JWT ID — binds the Refresh Token to exactly this Access Token
- **`role`**: Used by `[Authorize(Roles="...")]` and `[Authorize(Policy="...")]`
- **`ClockSkew = TimeSpan.Zero`**: No grace period; tokens expire precisely

### Token Lifetimes

| Token | Dev | Production |
|---|---|---|
| Access Token | 120 minutes | 60 minutes |
| Refresh Token | 30 days | 7 days |

---

## 6. Refresh Token Flow

```
Client                              Server
  │                                   │
  │── POST /api/auth/refresh ──────────►│
  │   { accessToken, refreshToken }    │
  │                                   │ 1. Validate AT signature (ignore expiry)
  │                                   │ 2. Extract jti from AT
  │                                   │ 3. Lookup RT in DB
  │                                   │ 4. Check: IsRevoked? → reject + cascade revoke
  │                                   │ 5. Check: IsUsed?    → reject (already rotated)
  │                                   │ 6. Check: Expired?   → reject
  │                                   │ 7. Check: jti match? → reject on mismatch
  │                                   │ 8. Mark old RT as IsUsed, set ReplacedByToken
  │                                   │ 9. Issue new AT + new RT
  │◄─ 200 { newAccessToken, newRefreshToken }
```

### Token Theft Detection

If a Refresh Token is reused after already being marked `IsUsed`, the system
treats this as evidence of token theft and **cascade-revokes all active refresh
tokens for that user**, forcing a full re-login on all devices.

---

## 7. Security Decisions

| Decision | Implementation |
|---|---|
| Password hashing | PBKDF2-SHA512, 100,000 iterations (OWASP min) |
| JWT signing | HMAC-SHA256 (HS256) |
| Refresh token storage | DB-backed, not in cookies |
| Token rotation | Every refresh issues a completely new pair |
| Reuse detection | Cascade revoke all user sessions |
| Lockout | 5 failed attempts → 15-minute lockout |
| Password change | Revokes all active refresh tokens |
| HTTPS enforcement | `RequireHttpsMetadata = true` in JWT bearer |
| ClockSkew | Zero — tokens expire exactly when they should |
| Secret key | Must be ≥ 256 bits; use environment variables in production |

### Production Checklist

- [ ] Replace `JwtSettings:SecretKey` with a 256-bit random value stored in Azure Key Vault / AWS Secrets Manager
- [ ] Set `RequireHttpsMetadata = true` (already done in code)
- [ ] Move DB lockout to a Redis-backed store for multi-instance deployments
- [ ] Add rate limiting middleware to `/api/auth/login` and `/api/auth/register`
- [ ] Implement email confirmation flow (structure is described, sending is Phase 3)
- [ ] Add audit logging for all auth events

---

## 8. Database Changes

### New Table: `refresh_tokens`

| Column | Type | Notes |
|---|---|---|
| Id | INT IDENTITY PK | |
| UserId | INT FK | → users.UserId |
| Token | NVARCHAR(500) UNIQUE | Base64-encoded 64 random bytes |
| JwtId | NVARCHAR(100) | Matches `jti` claim of the paired Access Token |
| IsUsed | BIT DEFAULT 0 | True after rotation |
| IsRevoked | BIT DEFAULT 0 | True after logout/revoke/theft detection |
| CreatedAt | DATETIME2 DEFAULT GETDATE() | |
| ExpiresAt | DATETIME2 | |
| ReplacedByToken | NVARCHAR(500) NULL | Rotation audit trail |
| RevokedReason | NVARCHAR(255) NULL | Human-readable reason |

### Modified Column

`users.PasswordHash` widened from `NVARCHAR(255)` to `NVARCHAR(500)` to accommodate
the PBKDF2 hash format: `base64(16-byte-salt):base64(32-byte-hash)`.

---

## 9. API Endpoints Reference

| Method | Route | Auth Required | Roles |
|---|---|---|---|
| POST | /api/auth/register | No | — |
| POST | /api/auth/login | No | — |
| POST | /api/auth/refresh | No | — |
| POST | /api/auth/logout | No | — |
| POST | /api/auth/change-password | Yes | Any |
| POST | /api/auth/revoke-token | Yes | Any (Admin can revoke others) |
| GET | /api/auth/me | Yes | Any |
| GET | /api/admin/dashboard | Yes | Admin |
| GET | /api/admin/users | Yes | Admin |
| DELETE | /api/admin/users/{id} | Yes | Admin |
| GET | /api/teacher/classes | Yes | Teacher |
| POST | /api/teacher/sessions | Yes | Teacher |
| GET | /api/teacher/assignments | Yes | Teacher |
| POST | /api/teacher/grade/{id} | Yes | Admin, Teacher |
| GET | /api/student/courses | Yes | Student |
| GET | /api/student/grades | Yes | Student |
| POST | /api/student/assignments/{id}/submit | Yes | Student |
| GET | /api/student/exams | Yes | Student |
| GET | /api/parent/children | Yes | Parent |
| GET | /api/parent/children/{id}/grades | Yes | Parent |
| GET | /api/parent/children/{id}/attendance | Yes | Parent |
| GET | /api/shared/progress/{id} | Yes | Student, Parent |
| GET | /api/shared/analytics | Yes | Admin, Teacher |
| GET | /api/shared/notifications | Yes | Any |

---

## 10. Phase 2 Testing Guide

### Prerequisites
- SQL Server running locally
- .NET 9 SDK installed
- Project built successfully

---

### Step 1: Apply Migration

```bash
# Option A: EF Core CLI (recommended)
dotnet ef migrations add Phase2_Auth
dotnet ef database update

# Option B: Raw SQL (if EF CLI unavailable)
# Open SQL Server Management Studio
# Run: Migrations/Phase2_AddRefreshTokens.sql
```

---

### Step 2: Run the Application

```bash
dotnet run
# Navigate to: http://localhost:5000 (Swagger UI opens at root)
```

Roles and the Admin user are seeded automatically on startup. Console will show:
```
[Seeder] Roles seeded.
[Seeder] Admin user created.
         Email   : admin@masarak.com
         Password: Admin@12345!
```

---

### Step 3: Register a New User

**POST** `/api/auth/register`

```json
{
  "fullName": "Ahmed Hassan",
  "email": "teacher@masarak.com",
  "password": "Teacher@12345!",
  "confirmPassword": "Teacher@12345!",
  "phone": "+201001234567",
  "country": "EG",
  "role": "Teacher"
}
```

**Expected Response (200):**
```json
{
  "success": true,
  "accessToken": "",
  "refreshToken": "XyZ1a2b3c4...",
  "accessTokenExpiry": "2026-06-04T15:00:00Z",
  "refreshTokenExpiry": "2026-06-11T13:00:00Z",
  "user": {
    "userId": 2,
    "fullName": "Ahmed Hassan",
    "email": "teacher@masarak.com",
    "role": "Teacher",
    "isActive": true
  }
}
```

Register more users to test other roles:
- Role: `"Student"`, `"Parent"`, `"Admin"`

---

### Step 4: Login

**POST** `/api/auth/login`

```json
{
  "email": "admin@masarak.com",
  "password": "Admin@12345!"
}
```

Copy the `accessToken` from the response.

---

### Step 5: Authorize in Swagger

1. Click the **Authorize 🔒** button at the top of Swagger UI
2. Enter: `Bearer <paste your accessToken here>`
3. Click **Authorize** → **Close**

---

### Step 6: Test Admin Endpoint

**GET** `/api/admin/dashboard`

Expected: `200 OK` with Admin message.

Login as a Teacher and retry — expected: `403 Forbidden`.

---

### Step 7: Test Teacher Endpoint

Login as Teacher, copy token, authorize in Swagger.

**GET** `/api/teacher/classes`

Expected: `200 OK`.

Try with Admin token — expected: `403 Forbidden`.

---

### Step 8: Test Student Endpoint

Register/login as Student.

**GET** `/api/student/courses`

Expected: `200 OK`.

---

### Step 9: Test Parent Endpoint

Register/login as Parent.

**GET** `/api/parent/children`

Expected: `200 OK`.

---

### Step 10: Test GET /api/auth/me

With any valid token:

**GET** `/api/auth/me`

Expected:
```json
{
  "userId": 1,
  "fullName": "System Administrator",
  "email": "admin@masarak.com",
  "role": "Admin",
  "isActive": true
}
```

---

### Step 11: Test Refresh Token

**POST** `/api/auth/refresh`

```json
{
  "accessToken": "<expired or current access token>",
  "refreshToken": "<your refresh token>"
}
```

Expected: New `accessToken` + new `refreshToken`.

**Test reuse detection**: Send the same request again with the already-used refresh token.
Expected: `400` + "Refresh token has already been used." + all sessions revoked.

---

### Step 12: Test Change Password

Authorize with a valid token.

**POST** `/api/auth/change-password`

```json
{
  "currentPassword": "Admin@12345!",
  "newPassword": "Admin@NewPass99!",
  "confirmNewPassword": "Admin@NewPass99!"
}
```

Expected: `200 OK`. All existing refresh tokens are revoked. Re-login required.

---

### Step 13: Test Logout

**POST** `/api/auth/logout`

```json
{
  "refreshToken": "<your refresh token>"
}
```

Expected: `200 OK`. The refresh token is revoked. Attempting to use it for refresh will now fail.

---

### Step 14: Test Revoke Token (Admin)

Login as Admin.

**POST** `/api/auth/revoke-token`

```json
{
  "refreshToken": "<any user's refresh token>"
}
```

Expected: `200 OK`. Admins can revoke any token. Regular users can only revoke their own.

---

### Step 15: Test Lockout

Send 5 failed login requests in a row:

**POST** `/api/auth/login`
```json
{ "email": "admin@masarak.com", "password": "WrongPassword" }
```

On the 6th attempt:
Expected: `401` + "Account temporarily locked due to multiple failed login attempts."

Wait 15 minutes or restart the application to reset (in-memory lockout).

---

## 11. Future Improvements

| Item | Priority | Phase |
|---|---|---|
| Email confirmation flow | High | Phase 3 |
| Password reset via email token | High | Phase 3 |
| Redis-backed lockout (multi-instance safe) | Medium | Phase 3 |
| Rate limiting on auth endpoints | High | Phase 3 |
| Refresh token cleanup job (prune expired) | Low | Phase 3 |
| Repository/UoW pattern | Medium | Phase 3 |
| Audit log for auth events | Medium | Phase 3 |
| Two-factor authentication (TOTP) | Medium | Phase 4 |
