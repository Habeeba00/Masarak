# Masarak — Phase 2 Complete Integration Map

## How to read this document

For every entity in the system, this map answers:
- **Purpose** — what the table stores
- **Relationships** — FK connections to other entities
- **Phase 2 changes** — what was added or modified
- **Auth resolution** — how a JWT maps to this entity's rows
- **Policy guard** — which `[Authorize]` policy protects the endpoints that touch it

---

## Authentication Identity Chain

```
JWT token
  └─► "userid" claim (= users.UserId)
        ├─► users                 (always)
        ├─► students  WHERE UserId = @userId   (role == Student)
        ├─► teachers  WHERE UserId = @userId   (role == Teacher)
        └─► parents   WHERE UserId = @userId   (role == Parent)
```

---

## Entity Analysis

---

### 1. Role
| | |
|---|---|
| **Table** | `roles` |
| **Purpose** | Lookup table: Admin, Teacher, Student, Parent |
| **Relationships** | 1:N → User |
| **Phase 2 changes** | None |
| **Auth resolution** | `Role.Name` is written into the JWT `"role"` and `ClaimTypes.Role` claims at token issuance |
| **Policy guard** | AdminOnly (CRUD) |

---

### 2. User ⭐ PRIMARY AUTH ENTITY
| | |
|---|---|
| **Table** | `users` |
| **Purpose** | Central identity — one row per person regardless of role |
| **Relationships** | N:1 → Role · 1:1 → Student · 1:1 → Teacher · 1:1 → Parent · 1:N → Notification · 1:N → Subscription · 1:N → RefreshToken |
| **Phase 2 changes** | `PasswordHash` widened 255→500 · `EmailConfirmed` added · `FailedLoginCount` added · `LockoutEnd` added · `RefreshTokens` navigation added |
| **Auth resolution** | The `"userid"` JWT claim = `UserId`. Every protected service resolves the caller via `db.Users.FindAsync(currentUserId)` |
| **Policy guard** | Owner-scoped: users manage their own account. AdminOnly for listing/deactivating others |

**Updated columns:**
```sql
PasswordHash     NVARCHAR(500)  -- was 255; PBKDF2 format: base64(salt):base64(hash)
EmailConfirmed   BIT DEFAULT 0  -- Phase 2 NEW
FailedLoginCount INT DEFAULT 0  -- Phase 2 NEW; persisted across restarts
LockoutEnd       DATETIME2 NULL -- Phase 2 NEW; null = not locked
```

---

### 3. Student
| | |
|---|---|
| **Table** | `students` |
| **Purpose** | Academic profile for a Student-role user |
| **Relationships** | 1:1 → User · N:1 → Grade · 1:N → ParentStudent · 1:N → StudentClass · 1:N → Attendance · 1:N → Submission · 1:N → StudentExam · 1:N → StudentPerformance · 1:N → AiRecommendation |
| **Phase 2 changes** | None (profile auto-created by `AuthService.CreateRoleProfileAsync` on registration) |
| **Auth resolution** | `db.Students.FirstOrDefault(s => s.UserId == currentUserId)` |
| **Policy guard** | StudentOnly for self-service. AdminOrTeacher for management |

**Service pattern:**
```csharp
// In any StudentController action:
var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == currentUserId);
if (student == null) return Forbid();
```

---

### 4. Teacher
| | |
|---|---|
| **Table** | `teachers` |
| **Purpose** | Professional profile for a Teacher-role user |
| **Relationships** | 1:1 → User · 1:N → TeachingAssignment |
| **Phase 2 changes** | None (auto-created on registration) |
| **Auth resolution** | `db.Teachers.FirstOrDefault(t => t.UserId == currentUserId)` |
| **Policy guard** | TeacherOnly / AdminOrTeacher |

---

### 5. Parent
| | |
|---|---|
| **Table** | `parents` |
| **Purpose** | Guardian profile for a Parent-role user |
| **Relationships** | 1:1 → User · 1:N → ParentStudent |
| **Phase 2 changes** | None (auto-created on registration) |
| **Auth resolution** | `db.Parents.FirstOrDefault(p => p.UserId == currentUserId)` |
| **Policy guard** | ParentOnly |

---

### 6. ParentStudent
| | |
|---|---|
| **Table** | `parent_student` |
| **Purpose** | Links parents to their children (M:M join) |
| **Relationships** | N:1 → Parent · N:1 → Student |
| **Composite PK** | (ParentId, StudentId) |
| **Phase 2 changes** | None |
| **Auth resolution** | Parent endpoint resolves children: `db.ParentStudents.Where(ps => ps.Parent.UserId == currentUserId)` |
| **Policy guard** | ParentOnly (read). AdminOnly (write) |

---

### 7. Grade
| | |
|---|---|
| **Table** | `grades` |
| **Purpose** | Academic year level (Grade 1–12) |
| **Relationships** | 1:N → Student · 1:N → Class · 1:N → Subject |
| **Phase 2 changes** | Seeded (1–12) on startup by `DatabaseSeeder.SeedGradesAsync` |
| **Auth resolution** | No direct auth gate; accessed through related entities |
| **Policy guard** | AdminOnly (CRUD). Any authenticated (read) |

---

### 8. Class
| | |
|---|---|
| **Table** | `classes` |
| **Purpose** | A classroom group within a Grade (e.g. "7A") |
| **Relationships** | N:1 → Grade · 1:N → StudentClass · 1:N → TeachingAssignment |
| **Phase 2 changes** | None |
| **Auth resolution** | Teachers see classes they are assigned to. Students see their enrolled class |
| **Policy guard** | AdminOnly (CRUD). TeacherOnly/StudentOnly (read) |

---

### 9. StudentClass
| | |
|---|---|
| **Table** | `student_class` |
| **Purpose** | Enrols a Student in a Class for an academic year |
| **Composite PK** | (StudentId, ClassId, AcademicYear) |
| **Relationships** | N:1 → Student · N:1 → Class |
| **Phase 2 changes** | None |
| **Auth resolution** | `db.StudentClasses.Where(sc => sc.Student.UserId == currentUserId && sc.IsCurrent)` |
| **Policy guard** | AdminOnly (write). StudentOnly (read own) |

---

### 10. Subject
| | |
|---|---|
| **Table** | `subjects` |
| **Purpose** | An academic subject under a Grade |
| **Relationships** | N:1 → Grade · 1:N → TeachingAssignment · 1:N → StudentPerformance |
| **Phase 2 changes** | None |
| **Auth resolution** | Accessed through TeachingAssignment (teacher) or StudentPerformance (student) |
| **Policy guard** | AdminOnly (CRUD). Any authenticated (read) |

---

### 11. TeachingAssignment
| | |
|---|---|
| **Table** | `teaching_assignments` |
| **Purpose** | Assigns Teacher to Subject+Class for an academic year. The anchor for all content |
| **Unique constraint** | (TeacherId, SubjectId, ClassId, AcademicYear) |
| **Relationships** | N:1 → Teacher · N:1 → Subject · N:1 → Class · 1:N → Session · 1:N → Assignment · 1:N → Exam |
| **Phase 2 changes** | None |
| **Auth resolution** | `db.TeachingAssignments.Where(ta => ta.Teacher.UserId == currentUserId)` |
| **Policy guard** | AdminOnly (write). TeacherOnly (read own). StudentOnly (read for enrolled class) |

---

### 12. Session
| | |
|---|---|
| **Table** | `sessions` |
| **Purpose** | A live or recorded class meeting |
| **Relationships** | N:1 → TeachingAssignment · 1:N → Attendance |
| **Phase 2 changes** | None |
| **Auth resolution** | Teacher creates via their TeachingAssignment. Student reads via enrolled class |
| **Policy guard** | TeacherOnly (write). StudentOnly/ParentOnly (read) |

---

### 13. Attendance
| | |
|---|---|
| **Table** | `attendance` |
| **Purpose** | Records whether a Student attended a Session |
| **Unique constraint** | (SessionId, StudentId) |
| **Relationships** | N:1 → Session · N:1 → Student |
| **Phase 2 changes** | None |
| **Auth resolution** | Students read own: `db.Attendances.Where(a => a.Student.UserId == currentUserId)` |
| **Policy guard** | AdminOrTeacher (write). StudentOnly/ParentOnly (read) |

---

### 14. Assignment
| | |
|---|---|
| **Table** | `assignments` |
| **Purpose** | Homework/coursework posted by a teacher |
| **Relationships** | N:1 → TeachingAssignment (via AssignmentRef FK) · 1:N → Submission |
| **Phase 2 changes** | None |
| **Auth resolution** | Teacher scoped through their TeachingAssignment. Student sees assignments for their class |
| **Policy guard** | TeacherOnly (write). StudentOnly (read + submit) |

---

### 15. Submission
| | |
|---|---|
| **Table** | `submissions` |
| **Purpose** | A student's submitted work for an assignment |
| **Unique constraint** | (AssignmentId, StudentId) |
| **Relationships** | N:1 → Assignment · N:1 → Student |
| **Phase 2 changes** | None |
| **Auth resolution** | Student submits own: service verifies `Submission.Student.UserId == currentUserId` |
| **Policy guard** | StudentOnly (write own). AdminOrTeacher (grade/read all) |

---

### 16. Exam
| | |
|---|---|
| **Table** | `exams` |
| **Purpose** | An exam event under a TeachingAssignment |
| **Relationships** | N:1 → TeachingAssignment · 1:N → Question · 1:N → StudentExam |
| **Phase 2 changes** | None |
| **Auth resolution** | Teacher scoped through TeachingAssignment. Student sees exams for their class |
| **Policy guard** | TeacherOnly (write). StudentOnly (take) |

---

### 17. Question
| | |
|---|---|
| **Table** | `questions` |
| **Purpose** | Individual question inside an Exam |
| **Relationships** | N:1 → Exam · 1:N → StudentAnswer |
| **Phase 2 changes** | None |
| **Auth resolution** | Teacher manages. Student sees questions (without CorrectAns) during exam window |
| **Policy guard** | TeacherOnly (write). StudentOnly (read, CorrectAns stripped from DTO) |

---

### 18. StudentExam
| | |
|---|---|
| **Table** | `student_exams` |
| **Purpose** | One student's attempt at one exam |
| **Unique constraint** | (ExamId, StudentId) |
| **Relationships** | N:1 → Exam · N:1 → Student · 1:N → StudentAnswer |
| **Phase 2 changes** | None |
| **Auth resolution** | Student sees own: `db.StudentExams.Where(se => se.Student.UserId == currentUserId)` |
| **Policy guard** | StudentOnly (write+read own). AdminOrTeacher (read all) |

---

### 19. StudentAnswer
| | |
|---|---|
| **Table** | `student_answers` |
| **Purpose** | One answer to one question within a StudentExam |
| **Unique constraint** | (StudentExamId, QuestionId) |
| **Relationships** | N:1 → StudentExam · N:1 → Question |
| **Phase 2 changes** | None |
| **Auth resolution** | Scoped through StudentExam → Student → UserId |
| **Policy guard** | StudentOnly (submit). AdminOrTeacher (grade/read) |

---

### 20. StudentPerformance
| | |
|---|---|
| **Table** | `student_performance` |
| **Purpose** | Aggregated grade/performance per student per subject per year |
| **Unique constraint** | (StudentId, SubjectId, AcademicYear) |
| **Relationships** | N:1 → Student · N:1 → Subject |
| **Phase 2 changes** | None |
| **Auth resolution** | Student reads own. Parent reads child's. Teacher reads class's |
| **Policy guard** | StudentOnly/ParentOnly/TeacherOnly (read). AdminOrTeacher (write) |

---

### 21. AiRecommendation
| | |
|---|---|
| **Table** | `ai_recommendations` |
| **Purpose** | AI-generated study tips targeted at a specific student |
| **Relationships** | N:1 → Student |
| **Phase 2 changes** | None |
| **Auth resolution** | Student reads own: `db.AiRecommendations.Where(r => r.Student.UserId == currentUserId)` |
| **Policy guard** | StudentOnly/ParentOnly (read). AdminOnly (write/generate) |

---

### 22. Notification
| | |
|---|---|
| **Table** | `notifications` |
| **Purpose** | In-app notification for any User |
| **Relationships** | N:1 → User |
| **Phase 2 changes** | None |
| **Auth resolution** | `db.Notifications.Where(n => n.UserId == currentUserId)` — the `"userid"` claim is the direct filter key |
| **Policy guard** | AnyAuthenticated (read own). AdminOnly (broadcast/write) |

---

### 23. Plan
| | |
|---|---|
| **Table** | `plans` |
| **Purpose** | Subscription tier definition (Free / Basic / Premium) |
| **Relationships** | 1:N → Subscription |
| **Phase 2 changes** | None |
| **Auth resolution** | Feature gates checked via `user → Subscription → Plan` |
| **Policy guard** | AdminOnly (CRUD). Any authenticated (read list) |

---

### 24. Subscription
| | |
|---|---|
| **Table** | `subscriptions` |
| **Purpose** | Links a User to a Plan over a billing period |
| **Relationships** | N:1 → User · N:1 → Plan · 1:N → Payment |
| **Phase 2 changes** | None |
| **Auth resolution** | `db.Subscriptions.Where(s => s.UserId == currentUserId && s.Status == "Active")` |
| **Policy guard** | AnyAuthenticated (own). AdminOnly (all) |

---

### 25. Payment
| | |
|---|---|
| **Table** | `payments` |
| **Purpose** | A payment transaction for a Subscription |
| **Relationships** | N:1 → Subscription |
| **Phase 2 changes** | None |
| **Auth resolution** | Accessed via Subscription.UserId == currentUserId |
| **Policy guard** | AnyAuthenticated (view own history). AdminOnly (all + refunds) |

---

### 26. RefreshToken ⭐ PHASE 2 NEW
| | |
|---|---|
| **Table** | `refresh_tokens` |
| **Purpose** | Persisted refresh token for JWT rotation and revocation |
| **Relationships** | N:1 → User |
| **Phase 2 changes** | Entire table is new |
| **Auth resolution** | Looked up by Token string on every `POST /api/auth/refresh` call |
| **Policy guard** | No public endpoint returns raw token data. Managed exclusively by AuthService |

**Key columns:**
```
Token           — unique, 64-byte Base64 random value
JwtId           — matches the paired access token's "jti" claim
IsUsed          — set true when this RT is rotated (one-time use)
IsRevoked       — set true on logout, revoke, or theft detection
ReplacedByToken — audit trail linking old→new token in a rotation chain
RevokedReason   — human-readable reason string
```

---

## Authorization Policy Map

| Policy | Roles included | Sample usage |
|---|---|---|
| `AdminOnly` | Admin | `/api/admin/*` |
| `TeacherOnly` | Teacher | `/api/teacher/*` |
| `StudentOnly` | Student | `/api/student/*` |
| `ParentOnly` | Parent | `/api/parent/*` |
| `AdminOrTeacher` | Admin, Teacher | Grading, analytics |
| `StudentOrParent` | Student, Parent | Progress viewing |
| `AnyAuthenticated` | All roles | Notifications, own profile |

---

## Full Project Structure

```
Masarak/
├── Controllers/
│   ├── AuthController.cs            # POST /api/auth/*  (7 endpoints)
│   └── SecuredControllers.cs        # Admin, Teacher, Student, Parent, Shared
│
├── Data/
│   └── Context.cs                   # DbContext — all 26 entities + RefreshToken
│
├── Models/                          # All domain entities (Phase 1 + Phase 2)
│   ├── Role.cs
│   ├── User.cs                      # ← Updated: 3 new columns
│   ├── Student.cs
│   ├── Teacher.cs
│   ├── Parent.cs
│   ├── ParentStudent.cs
│   ├── Grade.cs
│   ├── Class.cs
│   ├── StudentClass.cs
│   ├── Subject.cs
│   ├── TeachingAssignment.cs
│   ├── Session.cs
│   ├── Attendance.cs
│   ├── Assignment.cs
│   ├── Submission.cs
│   ├── Exam.cs
│   ├── Question.cs
│   ├── StudentExam.cs
│   ├── StudentAnswer.cs
│   ├── StudentPerformance.cs
│   ├── AiRecommendation.cs
│   ├── Notification.cs
│   ├── Plan.cs
│   ├── Subscription.cs
│   ├── Payment.cs
│   └── RefreshToken.cs              # ← Phase 2 NEW
│
├── DTOs/
│   └── AuthDTOs.cs                  # LoginRequest, RegisterRequest, AuthResponse, …
│
├── Services/
│   ├── IAuthService.cs
│   ├── AuthService.cs               # ← Updated: persisted lockout, Models namespace
│   ├── IJwtService.cs
│   ├── JwtService.cs
│   ├── IPasswordService.cs
│   └── PasswordService.cs           # PBKDF2-SHA512
│
├── Configurations/
│   └── JwtSettings.cs
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs
│
├── Policies/
│   └── AppPolicies.cs               # Role + Policy name constants
│
├── Seeders/
│   └── DatabaseSeeder.cs            # ← Updated: SeedGradesAsync added
│
├── Migrations/
│   ├── EfMigrations/
│   │   └── 20260604000000_Phase2_AuthIntegration.cs   # EF migration
│   ├── Phase2_AddRefreshTokens.sql  # Phase 1→2 SQL only (legacy)
│   └── Phase2_Auth_SqlChanges.sql   # Complete idempotent SQL (authoritative)
│
├── Program.cs                       # ← Updated: SeedGradesAsync call added
├── appsettings.json
├── appsettings.Development.json
├── Masarak.csproj
└── IntegrationMap.md                # This document
```

---

## Build Verification Checklist

### Entities
- [x] Role — unchanged, FK source for User.RoleId
- [x] User — updated: PasswordHash widened, 3 new auth columns, RefreshTokens nav
- [x] Student — unchanged, auto-created by AuthService on Student registration
- [x] Teacher — unchanged, auto-created by AuthService on Teacher registration
- [x] Parent — unchanged, auto-created by AuthService on Parent registration
- [x] ParentStudent — unchanged, composite PK intact
- [x] Grade — unchanged, seeded 1–12 on startup
- [x] Class — unchanged
- [x] StudentClass — unchanged, composite PK intact
- [x] Subject — unchanged
- [x] TeachingAssignment — unchanged, unique constraint intact
- [x] Session — unchanged
- [x] Attendance — unchanged, unique constraint intact
- [x] Assignment — unchanged, AssignmentRef FK preserved
- [x] Submission — unchanged, unique constraint intact
- [x] Exam — unchanged
- [x] Question — unchanged
- [x] StudentExam — unchanged, unique constraint intact
- [x] StudentAnswer — unchanged, unique constraint intact
- [x] StudentPerformance — unchanged, unique constraint intact
- [x] AiRecommendation — unchanged
- [x] Notification — unchanged
- [x] Plan — unchanged
- [x] Subscription — unchanged
- [x] Payment — unchanged
- [x] RefreshToken — NEW, FK to User, unique index on Token

### Context
- [x] All 26 DbSets present
- [x] All Fluent API configurations present
- [x] All unique indexes named and configured
- [x] All delete behaviors set to Restrict (no cascade)
- [x] Both constructors present (DI + parameterless for tooling)
- [x] OnConfiguring fallback for EF CLI tooling

### Relationships preserved
- [x] User 1:1 Student (UserId unique index)
- [x] User 1:1 Teacher (UserId unique index)
- [x] User 1:1 Parent (UserId unique index)
- [x] User 1:N RefreshToken (new)
- [x] User N:1 Role
- [x] Student N:M Parent (through ParentStudent)
- [x] Student N:M Class (through StudentClass with AcademicYear)
- [x] Teacher 1:N TeachingAssignment
- [x] TeachingAssignment 1:N Session
- [x] TeachingAssignment 1:N Assignment
- [x] TeachingAssignment 1:N Exam
- [x] Exam 1:N Question
- [x] Exam N:M Student (through StudentExam)
- [x] StudentExam 1:N StudentAnswer
- [x] Assignment N:M Student (through Submission)
- [x] Student N:M Subject (through StudentPerformance)

### Auth layer
- [x] JWT contains: sub, email, name, role, userid, jti
- [x] Refresh token rotation with one-time-use enforcement
- [x] Token theft detection → cascade revoke
- [x] Persisted lockout on User entity (works across restarts)
- [x] Password change → revokes all active refresh tokens
- [x] Role profiles auto-created at registration
- [x] Policies: AdminOnly, TeacherOnly, StudentOnly, ParentOnly, AdminOrTeacher, StudentOrParent
