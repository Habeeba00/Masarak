# Masarak Project - Phase Progress & Architecture Overview

## 1. Current Architecture
The project follows a **Clean Architecture** approach implemented in .NET 9, divided into three main layers:
- **1. Core (Domain & Application):** Contains business entities, interfaces, constants (e.g., AppRoles), and DTOs. Completely independent of infrastructure.
- **2. Infrastructure:** Implements Data Access using Entity Framework Core (EF Core), database context (`Context.cs`), migrations, seeders, and external services (e.g., `AuthService`, `JwtService`, `PasswordService`).
- **3. Presentation:** The ASP.NET Core API layer containing controllers, routing, authorization policies, middleware, and dependency injection configurations.

## 2. Bounded Contexts & Business Modules
The system is logically divided into the following bounded contexts/modules:
- **Identity & Access Management:** User registration, JWT-based authentication, refresh token rotation, role profiles, and security lockouts.
- **Academic Core:** Management of Grades, Classes, Subjects, and Teaching Assignments.
- **Assessment & Grading:** Handling Assignments, Submissions, Exams, Questions, and Student Performance tracking.
- **Attendance Management:** Tracking student session attendance and instructor QR attendance.
- **Financial & Subscriptions:** Managing Plans, Subscriptions, and Payments.
- **Communications & Analytics:** Notifications, Dashboards, AI Recommendations, and system-wide analytics.

## 3. Existing Database Design
The database utilizes SQL Server via EF Core and consists of 26 core entities with strict relational integrity:
- **Identity:** `users`, `roles`, `refresh_tokens`
- **Profiles:** `students`, `teachers`, `parents`, `parent_student`
- **Academic:** `grades`, `classes`, `student_class`, `subjects`, `teaching_assignments`, `sessions`
- **Assessment:** `assignments`, `submissions`, `exams`, `questions`, `student_exams`, `student_answers`, `student_performance`
- **Financial:** `plans`, `subscriptions`, `payments`
- **Auxiliary:** `attendance`, `ai_recommendations`, `notifications`

## 4. User Roles & Permissions
The system employs a robust Role-Based Access Control (RBAC) mechanism using JWT claims:
- **Roles:** `Admin`, `Teacher`, `Student`, `Parent`.
- **Authorization Policies:**
  - `AdminOnly`: Full CRUD over system resources, user management, and global financial views.
  - `TeacherOnly`: Manage assigned classes, create sessions, post assignments, author exams, and grade submissions.
  - `StudentOnly`: View enrolled courses, submit assignments, take exams, and view own attendance/grades.
  - `ParentOnly`: View linked children's progress, grades, and attendance.
  - Shared Policies: `AdminOrTeacher`, `StudentOrParent`, `AnyAuthenticated`.

## 5. External Integrations
- **Database:** Microsoft SQL Server (via EF Core).
- **Security:** Custom PBKDF2-SHA512 password hashing, JWT Bearer authentication.
- **Testing:** Postman / Newman for automated API Integration and Security Testing.
- **Planned/Implicit:** AI Integration for `AiRecommendation`, Payment Gateway for `Payment` processing.

## 6. Current Progress
### ✅ Completed Phases
- **Phase 1: Initial Architecture & Database Design** - Scaffolded the 26 entities, relationships, and Clean Architecture structure.
- **Phase 2: Authentication & Student Management** - Custom Identity implementation, JWT integration, Refresh Tokens, and base Student CRUD.
- **System Hardening & Security Testing** - Executed forensic architectural audits, resolved referential integrity issues, automated security testing via Newman, and verified SOLID compliance.
- **Architecture Migration Refactor** - Successfully refactored legacy components into strict Clean Architecture layers ensuring 100% functional parity.
