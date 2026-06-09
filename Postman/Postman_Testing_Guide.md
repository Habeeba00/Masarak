# Postman Testing Guide — Masarak Education API (Phase 2 - Clean Architecture Refactored)

This guide documents the Postman testing suite generated for Phase 2: Authentication & Authorization.

## 1. Getting Started

### Importing Artifacts

1. Open Postman.
2. Click **Import** in the top left.
3. Select and import `Masarak_Phase2.postman_collection.json`.
4. Select and import `Masarak_Local.postman_environment.json`.
5. In the top right corner of Postman, select the `Masarak_Local` environment from the dropdown menu.

### Running the Tests Manually

1. **Register Users**: Go to the **Authentication** folder and run the Register endpoints for Admin, Teacher, Student, and Parent.
2. **Login Users**: Run the Login endpoints. 
   > [!NOTE]
   > The test scripts automatically extract `accessToken` and `refreshToken` and store them in your environment variables (e.g., `adminAccessToken`).
3. **Execute Endpoints**: You can now run endpoints in the other folders (Admin, Teacher, Student, Parent, Shared). Postman will automatically use the correct Bearer tokens.

---

## 2. Newman Execution (CI/CD)

[Newman](https://learning.postman.com/docs/collections/using-newman/command-line-integration-with-newman/) is the command-line collection runner for Postman.

### Installation
```bash
npm install -g newman
```

### Basic Run
Run this command from the `Postman` directory:
```bash
newman run Masarak_Phase2.postman_collection.json -e Masarak_Local.postman_environment.json -k
```

### Generate HTML Report
```bash
npm install -g newman-reporter-htmlextra
newman run Masarak_Phase2.postman_collection.json -e Masarak_Local.postman_environment.json -k -r cli,htmlextra
```

### GitHub Actions Integration Example
Create `.github/workflows/postman-tests.yml`:
```yaml
name: API Tests
on: [push, pull_request]

jobs:
  test-api:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      - name: Install Newman
        run: npm install -g newman
      - name: Run API Tests
        run: newman run Postman/Masarak_Phase2.postman_collection.json -e Postman/Masarak_Local.postman_environment.json -k
```

---

## 3. Authorization Matrix

| Endpoint | Allowed Roles | Expected Status (Allowed) | Expected Status (Denied) |
|---|---|---|---|
| `POST /api/auth/*` | Anonymous | 200 OK | N/A |
| `GET /api/auth/me` | Any Authenticated | 200 OK | 401 Unauthorized |
| `GET /api/admin/*` | Admin | 200 OK | 403 Forbidden |
| `GET /api/teacher/*` | Teacher | 200 OK | 403 Forbidden |
| `GET /api/student/*` | Student | 200 OK | 403 Forbidden |
| `GET /api/parent/*` | Parent | 200 OK | 403 Forbidden |
| `GET /api/shared/analytics` | Admin, Teacher | 200 OK | 403 Forbidden |
| `GET /api/shared/progress/{id}`| Student, Parent | 200 OK | 403 Forbidden |
| `GET /api/shared/notifications`| Any Authenticated | 200 OK | 401 Unauthorized |

---

## 4. API Coverage Report

- **Endpoint Coverage**: 100% of Phase 2 endpoints are documented and tested in this collection.
- **Role Coverage**: 100% of roles (Admin, Teacher, Student, Parent) have complete login/register flows and at least one dedicated Negative Test.
- **Security Coverage**: Includes Negative Tests for expired JWTs, invalid JWT signatures, missing JWTs, wrong role access, reused refresh tokens, revoked refresh tokens, and locked user accounts.

### Negative Testing Workflows

The `Negative Tests` folder ensures the API correctly rejects invalid states:
- **Token Tampering**: Sending modified tokens.
- **Role Escalation**: Attempting to access Admin routes with a Teacher token.
- **Refresh Token Reuse**: Validates the "Cascade Revoke" security feature. Reusing a token simulates a replay attack, which should trigger a `400 Bad Request` and invalidate all sessions.
- **Account Lockout**: Simulating 5 failed logins to test the 15-minute account lock.

---

> [!TIP]
> Ensure your API is running locally on `http://localhost:5000` before running these tests. If your API runs on a different port, update the `baseUrl` variable in your environment settings.
