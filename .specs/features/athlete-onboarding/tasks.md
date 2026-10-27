# Athlete Onboarding & Initial Strava Sync Tasks

## Execution Protocol (MANDATORY -- do not skip)

Implement these tasks with the `tlc-spec-driven` skill: **activate it by name and follow its Execute flow and Critical Rules.** Do not search for skill files by filesystem path. The skill is the source of truth for the full flow (per-task cycle, sub-agent delegation, the walk, check_proofs, the Verifier and the screens sub-agent).

**If the skill cannot be activated, STOP and tell the user - do not proceed without it.**

---

**Design**: `.specs/features/athlete-onboarding/design.md`
**Status**: Approved
**Approved**: 2026-10-26 - user: "Aprovar e executar T1 (Recomendado)"
**Mutations**: 3
**Build**: `make build; make lint; make harness`

---

## Test Coverage Matrix

> Generated from the approved design's Testing Strategy and the greenfield repository. Guidelines found: `.specs/features/athlete-onboarding/design.md` (Testing Strategy, Test rules, Tech Decisions). No existing source or test files exist, so location and command conventions below are the strong defaults chosen for this feature.

| Code Layer | Required Test Type | Coverage Expectation | Location Pattern | Run Command |
| ---------- | ------------------ | -------------------- | ---------------- | ----------- |
| Domain aggregate / value object / policy | unit | All branches; 1:1 to spec ACs; every listed edge case has a test | `tests/SprintUp.UnitTests/**/*.cs` | `make test-unit` |
| Handler / endpoint / EF configuration / worker | integration | Every handler and worker path: happy + edge + error; real PostgreSQL indexes, locks, and unique constraints | `tests/SprintUp.IntegrationTests/**/*.cs` | `make test-integration` |
| Blazor page / component | ui | Every named screen state, per-field error, empty/loading/error state, and read-only state | `tests/SprintUp.UiTests/**/*.cs` | `make test-ui` |
| End-to-end flow / accessibility / viewport | e2e | Register, verify, invite, profile, connect, sync, review, disconnect, delete; axe; 360 px and 1440 px | `tests/SprintUp.E2E/**/*.cs` | `make test-e2e` |
| Performance | performance | 12 months, 10,000 activities, 50 pages, p95 under 3 minutes against the local stub | `tests/SprintUp.PerformanceTests/**/*.cs` | `make perf` |
| Entity / configuration / schema / docs | none | Build + migration + documentation gate only | - | `make build` |

**Coverage note:** the integration suite uses PostgreSQL via Testcontainers. When Docker is unavailable, `TEST_DB_CONNECTION` points at a disposable local PostgreSQL; the fixture creates and drops a per-run schema and refuses a non-empty database. No test is silently skipped.

## Gate Check Commands

> Generated from the design's testing strategy. Targets are created in T5; later tasks consume them.

| Gate Level | When to Use | Command |
| ---------- | ----------- | ------- |
| Bootstrap | T1 only, before a solution exists | `git diff --check; test -f .editorconfig; test -f global.json; test -f Directory.Build.props` |
| Pre-Makefile build | T2-T3, before the Makefile exists | `dotnet build SprintUp.sln -warnaserror` |
| Pre-Makefile full | T4, before the Makefile exists | `TEST_DB_CONNECTION=<conn> dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj -warnaserror` |
| Quick | After tasks with unit tests only | `make test-unit` |
| Full | After tasks with integration or UI tests | `make test-unit; make test-integration; make test-ui` |
| Build | After phase completion or config/entity-only tasks | `make build; make lint; make harness` |
| Performance | Before release, after T78 | `make perf` |

---

## Execution Plan

Phases are ordered and run sequentially - each phase completes before the next begins, and tasks within a phase execute in order. Dependencies point backward only; a task may depend on tasks from earlier phases in addition to its predecessor in the same phase.

### Phase 1: Foundation

Bootstrap the repository, host, database, test stack, and shared infrastructure. No feature behavior yet.

```
T1 → T2 → T3 → T4 → T5 → T6 → T7 → T8
```

### Phase 2: Accounts Domain

Account, token, consent, and email-outbox domain models plus their first migration.

```
T9 → T10 → T11 → T12 → T13 → T14
```

### Phase 3: Coaching Core

Coach identities, the seeded AI coach, assignments, and operator-created coach accounts.

```
T15 → T16 → T17 → T18 → T19
```

### Phase 4: Account Flows and UI

Registration, verification, resend, login/logout, operator bootstrap, and the auth screens.

```
T20 → T21 → T22 → T23 → T24 → T25 → T26 → T27
```

### Phase 5: Profile and Onboarding

Required and optional profile data, consent gate, resume, preferences, and the profile screen.

```
T28 → T29 → T30 → T31 → T32 → T33
```

### Phase 6: Invitations

Invitation issuance, acceptance, mismatched-email approval, and the invitation entry screen.

```
T34 → T35 → T36 → T37 → T38
```

### Phase 7: Domain Contracts and Lifecycle Core

Provider contracts, subscription transitions, trial start, activity mapping, sync jobs, and initial-sync enqueue.

```
T39 → T40 → T41 → T42 → T43 → T44
```

### Phase 8: Strava Integration

The Strava adapter, OAuth state, token protection and refresh, connection lifecycle, and the Strava screen.

```
T45 → T46 → T47 → T48 → T49 → T50
```

### Phase 9: Sync Worker

Queue claim, pagination, idempotent upsert, retries, heartbeat, status, and the sync screen.

```
T51 → T52 → T53 → T54 → T55 → T56
```

### Phase 10: Review and Coach

History review, notes, hiding, coach read models, and the coach dashboard.

```
T57 → T58 → T59 → T60 → T61 → T62 → T63 → T64
```

### Phase 11: Lifecycle and Deletion

Trial expiry, retention, abandonment, read-only enforcement, deletion, purge, notifications, and lifecycle/admin screens.

```
T65 → T66 → T67 → T68 → T69 → T70 → T71 → T72
```

### Phase 12: Experience and Hardening

Complete localization, unit display, accessibility, responsive layout, end-to-end and performance proof, and the runbook.

```
T73 → T74 → T75 → T76 → T77 → T78 → T79
```

---

## Task Breakdown

### Phase 1: Foundation

### T1: Repository bootstrap and build conventions

**What**: Initialize the git repository, the cross-cutting build conventions, the public README, and the GitHub remote that every later task uses.
**Where**: `repository root`
**Depends on**: None
**Reuses**: None - greenfield repository
**Requirement**: AD-001, AD-002

**Done when**:

- [x] `git init` creates the repository on `main`; `.gitignore` excludes `bin/`, `obj/`, user secrets, Data Protection keys, `.env`, and Playwright output.
- [x] `.editorconfig`, `global.json` (SDK `10.0.113`), and `Directory.Build.props` (nullable, implicit usings, warnings as errors) are committed.
- [x] `README.md` states the product purpose, stack, repository layout, documentation links, prerequisites, quality gates, and license status, without claiming unimplemented features.
- [x] A public GitHub repository `sprintup` exists, `origin` points to it, and the bootstrap commit is pushed; the README records the repository URL.
- [x] `git diff --check` is clean and the bootstrap commit exists.
- [x] Test count: 0 (no code yet).

**Tests**: none
**Gate**: bootstrap

**Commit**: `chore(bootstrap): initialize repository, conventions, and README`

---

### T2: Create the .NET 10 solution and Blazor host

**What**: Create `SprintUp.sln` with the single Blazor Web App host, the interactive render mode, and the localization resource skeleton.
**Where**: `SprintUp.sln`
**Depends on**: T1
**Reuses**: `global.json` and `Directory.Build.props` from T1
**Requirement**: AD-002, AD-003, UX-03

**Done when**:

- [x] `SprintUp.sln` contains `src/SprintUp/SprintUp.csproj` targeting `net10.0` with the Blazor Web App template and InteractiveServer.
- [x] `src/SprintUp/Shared/Resources/SharedResources.resx` (English neutral) and `SharedResources.pt-BR.resx` exist with the shell keys.
- [x] `dotnet build SprintUp.sln -warnaserror` passes.
- [x] Test count: 0 (no test project yet).

**Tests**: none
**Gate**: pre-make-build

**Commit**: `feat(host): create .NET 10 Blazor solution and resource skeleton`

---

### T3: Add PostgreSQL Docker Compose and local configuration

**What**: Provide the local PostgreSQL 16 service and the documented configuration keys the host reads.
**Where**: `docker-compose.yml`
**Depends on**: T2
**Reuses**: `src/SprintUp/appsettings.json` from the template
**Requirement**: AD-002

**Done when**:

- [x] `docker-compose.yml` defines PostgreSQL 16 with a named volume and a `pg_isready` healthcheck.
- [x] The connection string, Strava settings, Data Protection path, admin bootstrap secret, and `TEST_DB_CONNECTION` fallback are documented as environment variables.
- [x] `docker compose config` validates on a Docker-enabled machine; the Docker prerequisite is called out for local runs.
- [x] Test count: 0 (configuration only).

**Tests**: none
**Gate**: pre-make-build

**Commit**: `chore(infra): add PostgreSQL compose service and configuration keys`

---

### T4: Create the test project skeleton and PostgreSQL fixture

**What**: Create the five test projects and the shared PostgreSQL fixture used by every integration and worker test.
**Where**: `tests/`
**Depends on**: T3
**Reuses**: `docker-compose.yml` from T3
**Requirement**: AD-002

**Done when**:

- [x] `SprintUp.UnitTests`, `SprintUp.IntegrationTests`, `SprintUp.UiTests`, `SprintUp.E2E`, and `SprintUp.PerformanceTests` reference the host and the agreed packages (xUnit, FluentAssertions, Moq, bUnit, Testcontainers, Playwright).
- [x] `PostgresFixture` creates a per-run schema, supports `TEST_DB_CONNECTION`, and refuses a non-empty database.
- [x] `DatabaseSmokeTests.CanConnectAndCreateSchema` passes against the compose database or `TEST_DB_CONNECTION`.
- [x] Test count: 1 integration test passes.

**Tests**: integration
**Gate**: pre-make-full

**Commit**: `test(harness): add test projects and PostgreSQL fixture`

---

### T5: Add the Makefile quality gates

**What**: Add the single source of truth for restore, build, format, test, E2E, and performance commands.
**Where**: `Makefile`
**Depends on**: T4
**Reuses**: `SprintUp.sln` and `tests/` from T2 and T4
**Requirement**: AD-002

**Done when**:

- [x] Targets exist for `restore`, `build`, `lint`, `test-unit`, `test-integration`, `test-ui`, `test-e2e`, `harness-fast`, `harness`, and `perf`.
- [x] `make build` runs `dotnet build SprintUp.sln -warnaserror`; `make lint` runs `dotnet format SprintUp.sln --verify-no-changes`.
- [x] `make harness` fails with a clear prerequisite message when Docker or Playwright browsers are missing; it never silently skips.
- [x] Test count: 0 (tooling only).

**Tests**: none
**Gate**: build

**Commit**: `chore(build): add Makefile quality gates`

---

### T6: Add SprintUpDbContext and module registration

**What**: Create the single EF Core context, the module registration seam, and the naming/concurrency conventions.
**Where**: `src/SprintUp/Shared/Infrastructure/SprintUpDbContext.cs`
**Depends on**: T5
**Reuses**: `Program.cs` host from T2; Npgsql provider from T3
**Requirement**: AD-002

**Done when**:

- [x] `SprintUpDbContext` exposes module-owned `DbSet` placeholders and applies `IEntityTypeConfiguration` classes from the assembly.
- [x] UTC timestamps, integer enums, `jsonb`, normalized-email siblings, and `xmin` concurrency are configured as conventions.
- [x] `SprintUpDbContextTests.Model_BuildsWithoutErrors` and `CanPersistAndRollback` pass against PostgreSQL.
- [x] Test count: 2 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(shared): add SprintUpDbContext and module registration`

---

### T7: Add ProblemDetails and validation error mapping

**What**: Add the RFC 7231 error pipeline with stable codes, correlation IDs, and no internal detail leakage.
**Where**: `src/SprintUp/Shared/Infrastructure/ProblemDetailsExceptionHandler.cs`
**Depends on**: T6
**Reuses**: `SprintUpDbContext` from T6
**Requirement**: UX-01, SYN-17

**Done when**:

- [x] `DomainException`, `ValidationException`, and `NotFoundException` map to `ProblemDetails` with stable `code` values and per-field errors.
- [x] Unexpected exceptions return `500` with a correlation ID and never a stack trace, SQL, token, or health value.
- [x] Tests cover domain conflict, validation fields, not found, and unexpected error.
- [x] Test count: 4 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(shared): add ProblemDetails error mapping`

---

### T8: Compose the host pipeline

**What**: Compose the single process: health endpoint, cultures, antiforgery, rate limiting, Data Protection, module registration, and worker seams.
**Where**: `src/SprintUp/Program.cs`
**Depends on**: T7
**Reuses**: `SprintUpDbContext`, `ProblemDetailsExceptionHandler`, and resource files
**Requirement**: AD-002, AD-003, UX-03

**Done when**:

- [x] `/health` reports database readiness; PT-BR is the default culture with EN available; antiforgery and fixed-window rate limits are active.
- [x] Data Protection is configured with a persistent key-ring path outside development; startup refuses ephemeral keys on a non-development environment.
- [x] Placeholder authentication and policy names (`AthleteOnly`, `ActiveAthlete`, `AssignedCoach`, `CoachReadOnly`, `AdminOnly`) are registered for later tasks.
- [x] Tests cover health, default culture, antiforgery failure, rate-limit `429`, and the error shape.
- [x] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: build

**Commit**: `feat(host): compose health, culture, antiforgery, and rate limiting`

---

### Phase 2: Accounts Domain

### T9: Implement the User aggregate

**What**: Implement the account aggregate with the approved status and onboarding transitions.
**Where**: `src/SprintUp/Modules/UserManagement/Domain/User.cs`
**Depends on**: T8
**Reuses**: `AggregateRoot` and UTC conventions from T6
**Requirement**: ONB-01, ONB-02, ONB-03

**Done when**:

- [ ] `User.RegisterAthlete`, `VerifyEmail`, `CompleteProfile`, `SetPreferences`, `TouchOnboarding`, `Deactivate`, `MarkDeleted`, and `RotateSecurityStamp` enforce the approved state model.
- [ ] Email normalization, role, status, language, unit preference, time zone, and onboarding status are invariant-protected.
- [ ] Tests cover registration state, verification transition, deletion transition, and security-stamp rotation.
- [ ] Test count: 4 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(accounts): add User aggregate and state transitions`

---

### T10: Implement the UserToken aggregate

**What**: Implement single-use verification and coach-activation tokens with hash-only storage and expiry.
**Where**: `src/SprintUp/Modules/UserManagement/Domain/UserToken.cs`
**Depends on**: T9
**Reuses**: `User` from T9
**Requirement**: ONB-03, ONB-04

**Done when**:

- [ ] Tokens store only a SHA-256 hash and expose `Consume` and `IsExpired` semantics for both purposes.
- [ ] A consumed or expired token cannot be consumed again; the expiry is 24 hours.
- [ ] Tests cover first consume, second consume, expired token, and purpose separation.
- [ ] Test count: 4 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(accounts): add single-use UserToken aggregate`

---

### T11: Implement the ConsentRecord aggregate

**What**: Implement append-only consent rows with type, document version, timestamp, and revocation.
**Where**: `src/SprintUp/Modules/UserManagement/Domain/ConsentRecord.cs`
**Depends on**: T10
**Reuses**: `User` from T9
**Requirement**: ONB-05

**Done when**:

- [ ] Accepting a consent records user, type, document version, and timestamp; revocation sets `RevokedAt` without deleting the row.
- [ ] Terms, Privacy, Health, and AiRag are separate consent types.
- [ ] Tests cover accept, revoke, and immutable history.
- [ ] Test count: 3 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(accounts): add append-only ConsentRecord aggregate`

---

### T12: Add account EF configurations and the initial migration

**What**: Persist the account model with unique email, unique token hash, consent history, and the first migration.
**Where**: `src/SprintUp/Modules/UserManagement/Infrastructure/AccountConfigurations.cs`
**Depends on**: T11
**Reuses**: `SprintUpDbContext` from T6; aggregates from T9-T11
**Requirement**: ONB-02, ONB-05

**Done when**:

- [ ] `Users(NormalizedEmail)` and `UserTokens(TokenHash)` are unique; consent history is indexed for lookup.
- [ ] The initial migration applies to an empty PostgreSQL database and rolls back cleanly.
- [ ] Tests cover duplicate email, duplicate token hash, and consent append behavior.
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(accounts): add EF configurations and initial migration`

---

### T13: Implement the EmailOutbox entity

**What**: Persist transactional emails outside the request transaction with retry metadata and recipient clearing.
**Where**: `src/SprintUp/Modules/UserManagement/Domain/EmailOutbox.cs`
**Depends on**: T12
**Reuses**: `SprintUpDbContext` from T6
**Requirement**: ONB-01

**Done when**:

- [ ] `EmailOutbox` stores recipient, template key, payload, culture, status, attempts, next attempt, and timestamps.
- [ ] The `(Status, NextAttemptAt)` index supports the send scan; a sent row clears the recipient.
- [ ] Tests cover pending defaults, due-row query, and recipient clearing.
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(accounts): add EmailOutbox persistence`

---

### T14: Implement the EmailOutboxWorker

**What**: Send pending email within the 60-second requirement with backoff, stale-sending recovery, and a console adapter.
**Where**: `src/SprintUp/Shared/Infrastructure/EmailOutboxWorker.cs`
**Depends on**: T13
**Reuses**: `EmailOutbox` from T13; `TimeProvider` from T8
**Requirement**: ONB-01

**Done when**:

- [ ] The worker polls every five seconds, claims `Sending`, sends through `IEmailService`, marks `Sent`, and retries transient provider failures with backoff.
- [ ] A `Sending` row older than five minutes is retried, so a crash cannot strand an email.
- [ ] Tests with `FakeTimeProvider` cover send-within-60-seconds, provider backoff, and stale-sending recovery.
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(accounts): add EmailOutbox worker and console sender`

---

### Phase 3: Coaching Core

### T15: Implement the Coach entity and seed the AI coach

**What**: Add human and AI coach identities and the seeded AI coach that every athlete receives.
**Where**: `src/SprintUp/Modules/Coaching/Domain/Coach.cs`
**Depends on**: T14, T9
**Reuses**: `User` from T9; migration pattern from T12
**Requirement**: ONB-15, ONB-17

**Done when**:

- [ ] `Coach` supports `Ai` and `Human`, optional `UserId`, display name, email, and status.
- [ ] A migration seeds one `Ai` coach with no login and `Active` status.
- [ ] Tests cover the seed row, one coach profile per login, and AI having no user.
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(coaching): add Coach entity and seeded AI coach`

---

### T16: Implement the CoachAssignment entity

**What**: Add coach-athlete assignments with one active AI and at most one active human coach.
**Where**: `src/SprintUp/Modules/Coaching/Domain/CoachAssignment.cs`
**Depends on**: T15
**Reuses**: `Coach` from T15; `User` from T9
**Requirement**: ONB-16, ONB-17

**Done when**:

- [ ] Assignments carry athlete user, coach, role, assigned time, and revoked time.
- [ ] Partial unique indexes enforce one active `AiPrimary` and one active `Human` per athlete.
- [ ] Tests cover duplicate AI rejection, duplicate human rejection, and revoked-assignment replacement.
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(coaching): add CoachAssignment with active-role indexes`

---

### T17: Implement the assignment service

**What**: Implement idempotent AI assignment, human assignment, and full revocation.
**Where**: `src/SprintUp/Modules/Coaching/Features/AssignmentService.cs`
**Depends on**: T16
**Reuses**: `CoachAssignment` from T16
**Requirement**: ONB-15, ONB-16, ONB-17

**Done when**:

- [ ] `AssignAiCoachAsync` is idempotent and always creates an `AiPrimary` assignment.
- [ ] `AssignHumanCoachAsync` rejects a second active human coach and never replaces the AI primary.
- [ ] `RevokeAllAsync` revokes every active assignment for deletion and retention end.
- [ ] Tests cover AI idempotency, second-human rejection, AI-stays-primary, and revoke-all.
- [ ] Test count: 4 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(coaching): add assignment service and invariants`

---

### T18: Implement CreateCoachHandler

**What**: Create an operator-issued coach login and profile in one transaction.
**Where**: `src/SprintUp/Modules/Coaching/Features/CreateCoachHandler.cs`
**Depends on**: T17, T10, T14
**Reuses**: `User.CreateCoach`, `UserToken`, `EmailOutbox`, `Coach`
**Requirement**: AD-005

**Done when**:

- [ ] The handler creates a `Coach` user in `PendingVerification`, a `Coach` profile in `PendingActivation`, and a 24-hour activation token.
- [ ] The activation email is enqueued in the same transaction; coach and admin users bypass the athlete onboarding guard.
- [ ] Tests cover the happy path, duplicate email, and outbox row creation.
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(coaching): add operator coach creation`

---

### T19: Implement ActivateCoachHandler

**What**: Consume the activation token, set the coach password, and activate the login and profile.
**Where**: `src/SprintUp/Modules/Coaching/Features/ActivateCoachHandler.cs`
**Depends on**: T18
**Reuses**: `UserToken` from T10; `Coach` from T15
**Requirement**: AD-005

**Done when**:

- [ ] A valid token sets the password, consumes the token, and activates both the user and coach profile in one transaction.
- [ ] Expired or consumed tokens return `coach_activation_invalid` and offer a new operator-issued link.
- [ ] Tests cover success, expired token, and reused token.
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(coaching): add coach activation flow`

---

### Phase 4: Account Flows and UI

### T20: Implement RegisterAthleteHandler

**What**: Register an athlete, enforce email uniqueness, record consent, assign the AI coach, and enqueue verification.
**Where**: `src/SprintUp/Modules/UserManagement/Features/Register/RegisterAthleteHandler.cs`
**Depends on**: T19, T17, T14
**Reuses**: `User`, `ConsentRecord`, `UserToken`, `EmailOutbox`, `AssignmentService`
**Requirement**: ONB-01, ONB-02, ONB-15

**Done when**:

- [ ] Email is trimmed and lowercased; password length 12-128 and the common-password block are enforced; the password is hashed with `PasswordHasher<User>`.
- [ ] A duplicate email returns `409 email_already_registered` with the exact ONB-02 message and creates no second account, including under a unique-index race.
- [ ] The transaction writes Terms and Privacy consent rows and calls `AssignAiCoachAsync`; a 32-byte verification token is stored as a hash and its email is enqueued.
- [ ] Tests cover the happy path, duplicate email, unique-index race, consent rows, and AI assignment.
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(accounts): add athlete registration handler`

---

### T21: Implement VerifyEmailHandler

**What**: Consume a verification token atomically, activate the account, and return the first incomplete onboarding step.
**Where**: `src/SprintUp/Modules/UserManagement/Features/VerifyEmail/VerifyEmailHandler.cs`
**Depends on**: T20, T10
**Reuses**: `User.VerifyEmail`, `UserToken`, `EmailOutbox`
**Requirement**: ONB-03, ONB-04

**Done when**:

- [ ] A valid token sets `EmailVerifiedAt`, moves the user to `Active`, sets onboarding to `ProfilePending`, and returns the resume target.
- [ ] Expired, consumed, or unknown tokens return `400 invalid_or_expired_token` and store no state change.
- [ ] Tests cover valid verification, expired token, reused token, and unknown token.
- [ ] Test count: 4 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(accounts): add email verification handler`

---

### T22: Implement ResendVerificationHandler

**What**: Reissue verification while invalidating every previous unconsumed token for that user.
**Where**: `src/SprintUp/Modules/UserManagement/Features/ResendVerification/ResendVerificationHandler.cs`
**Depends on**: T21
**Reuses**: `UserToken`, `EmailOutbox`, `TimeProvider`
**Requirement**: ONB-04

**Done when**:

- [ ] Every unconsumed verification token for the user is marked consumed before the new 24-hour token is created.
- [ ] The new token is enqueued in the outbox; resend is rate-limited per email and per IP.
- [ ] Tests cover invalidation, new token validity, and rate-limit response.
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(accounts): add verification resend with token invalidation`

---

### T23: Implement login and logout

**What**: Authenticate active users, issue the HTTP-only cookie, enforce the security stamp, and delete the cookie on logout.
**Where**: `src/SprintUp/Modules/UserManagement/Features/Login/LoginHandler.cs`
**Depends on**: T22
**Reuses**: `User`, `PasswordHasher<User>`, cookie scheme from T8
**Requirement**: AD-003, ONB-03

**Done when**:

- [ ] Unknown email and wrong password return the same generic error; only `Active` users receive a cookie.
- [ ] The cookie is HTTP-only, `SameSite=Lax`, `Secure` outside development, and has a 14-day sliding expiration.
- [ ] `OnValidatePrincipal` compares the claim with `SecurityStamp` and account status and signs out on mismatch; a successful login rehashes when required.
- [ ] Tests cover success, wrong password, pending verification, inactive account, and security-stamp invalidation.
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(accounts): add login, logout, and security-stamp validation`

---

### T24: Implement the admin bootstrapper

**What**: Create the operator account from a secret at startup and refuse unsafe defaults.
**Where**: `src/SprintUp/Shared/Infrastructure/AdminBootstrapper.cs`
**Depends on**: T23
**Reuses**: `User`, `PasswordHasher<User>`, configuration from T3
**Requirement**: AD-003

**Done when**:

- [ ] When no admin exists, startup creates an `Active` admin with onboarding `Completed` from configuration.
- [ ] Startup refuses a missing or default bootstrap secret outside development and writes an audit entry.
- [ ] Tests cover first boot, second boot idempotency, and missing/default secret refusal.
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(accounts): add admin bootstrap from secret`

---

### T25: Build the sign-up page

**What**: Build `/signup` with the required fields, the duplicate-email message, and server-side error rendering.
**Where**: `src/SprintUp/Components/Pages/Auth/SignUp.razor`
**Depends on**: T24
**Reuses**: `RegisterAthleteHandler`, `SharedResources`
**Requirement**: ONB-01, ONB-02

**Done when**:

- [ ] Name, email, password, and terms acceptance are collected; labels, error summary, and focus handling are accessible.
- [ ] The duplicate-email state shows exactly "This email is already registered. Sign in or recover your password."
- [ ] The email-sent state confirms the 60-second delivery expectation and offers resend.
- [ ] bUnit tests cover form, validation, duplicate email, and sent states (4 tests).
- [ ] Test count: 4 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add accessible sign-up page`

---

### T26: Build the verification page

**What**: Build `/verify-email` with valid, expired, used, and resend states plus the resume redirect.
**Where**: `src/SprintUp/Components/Pages/Auth/VerifyEmail.razor`
**Depends on**: T25
**Reuses**: `VerifyEmailHandler`, `ResendVerificationHandler`
**Requirement**: ONB-03, ONB-04

**Done when**:

- [ ] A valid token activates and redirects to the first incomplete onboarding step.
- [ ] Expired and used states reject activation and show the resend form; a successful resend confirms that previous links are invalid.
- [ ] bUnit tests cover valid, expired, used, and resend states (4 tests).
- [ ] Test count: 4 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add verification and resend page`

---

### T27: Build the login page

**What**: Build `/login` with generic credentials errors and account-status messages.
**Where**: `src/SprintUp/Components/Pages/Auth/Login.razor`
**Depends on**: T26
**Reuses**: `LoginHandler`, `SharedResources`
**Requirement**: AD-003

**Done when**:

- [ ] The form shows a generic credentials error for unknown email and wrong password.
- [ ] `PendingVerification` and `Inactive` accounts receive distinct localized messages and no cookie.
- [ ] bUnit tests cover success, generic error, pending, and inactive states (4 tests).
- [ ] Test count: 4 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add login page with status states`

---

### Phase 5: Profile and Onboarding

### T28: Implement AthleteProfile and validation rules

**What**: Implement the profile value object and the approved bounds for required and optional fields.
**Where**: `src/SprintUp/Modules/UserManagement/Domain/AthleteProfile.cs`
**Depends on**: T27, T9
**Reuses**: `User` from T9
**Requirement**: ONB-06, ONB-07, ONB-11

**Done when**:

- [ ] Required fields are full name, birth date, time zone, experience, primary goal, and weekly availability.
- [ ] Birth date must be in the past and age 18+; weight 30-250 kg; height 120-230 cm; availability 0-40 hours.
- [ ] Optional fields (weight, height, gender, injury, resting heart rate, medications) can be absent without blocking.
- [ ] Tests cover required fields, each bound, under-18, optional omission, and per-field error keys (6 tests).
- [ ] Test count: 6 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(profile): add AthleteProfile and bounds`

---

### T29: Implement CompleteProfileHandler

**What**: Validate and persist the profile, gate health fields on consent, and advance onboarding.
**Where**: `src/SprintUp/Modules/UserManagement/Features/CompleteProfile/CompleteProfileHandler.cs`
**Depends on**: T28, T11
**Reuses**: `AthleteProfile`, `ConsentRecord`, `User.CompleteProfile`
**Requirement**: ONB-05, ONB-06, ONB-07, ONB-10, ONB-11

**Done when**:

- [ ] Invalid data returns per-field errors while preserving every submitted value and stores nothing.
- [ ] Optional health fields are stored only when an active `Health` consent row exists.
- [ ] A valid save moves onboarding to `StravaPending`, updates `LastOnboardingActivityAt`, and returns the next step.
- [ ] Tests cover missing required data, invalid bounds, optional omission, health-consent gate, and successful advance (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(profile): add complete-profile handler`

---

### T30: Implement onboarding state and resume

**What**: Provide one source for the current onboarding step, route guards, and resume after logout or session expiry.
**Where**: `src/SprintUp/Modules/UserManagement/Features/OnboardingState/GetOnboardingStateHandler.cs`
**Depends on**: T29
**Reuses**: `User`, `OnboardingStatus`, `ICurrentUser`
**Requirement**: ONB-03, ONB-08, ONB-12, ONB-17

**Done when**:

- [ ] The handler returns account, onboarding status, first incomplete step, and whether the profile is complete.
- [ ] Resume after logout, session expiry, or browser close returns the same step with saved data.
- [ ] A missing human coach never blocks the next step; the AI primary is always present.
- [ ] Tests cover each transition, resume, and no-human-coach behavior (4 tests).
- [ ] Test count: 4 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(onboarding): add state query and resume`

---

### T31: Implement preferences and unit selection

**What**: Persist language and metric/imperial display preference without changing canonical stored values.
**Where**: `src/SprintUp/Modules/UserManagement/Features/Preferences/SetPreferencesHandler.cs`
**Depends on**: T30
**Reuses**: `User.SetPreferences`, `UnitPreference`
**Requirement**: UX-03, SYN-14

**Done when**:

- [ ] PT-BR and EN are the only accepted languages; Metric and Imperial are the only unit values.
- [ ] Changing units rewrites no activity or profile measurement.
- [ ] Tests cover language change, unit change, and canonical-value immutability (3 tests).
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(profile): add language and unit preferences`

---

### T32: Build the profile page

**What**: Build `/onboarding/profile` with required and optional fields, consent, per-field errors, and resume.
**Where**: `src/SprintUp/Components/Pages/Onboarding/Profile.razor`
**Depends on**: T31
**Reuses**: `CompleteProfileHandler`, `GetOnboardingStateHandler`, `ConsentRecord`
**Requirement**: ONB-06, ONB-07, ONB-08, ONB-11

**Done when**:

- [ ] Required fields block progress; optional fields show a skip path and are hidden or disabled until Health consent.
- [ ] Invalid submits show linked per-field errors and preserve all entered values.
- [ ] The page reloads saved data on return and announces saving/saved states via a live region.
- [ ] bUnit tests cover required, optional skip, invalid bounds, consent gate, resume, and saved states (6 tests).
- [ ] Test count: 6 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add profile page with resume and consent`

---

### T33: Implement continue-without-Strava and the onboarding guard

**What**: Allow completion with a pending sync, and guard onboarding routes to the owning athlete.
**Where**: `src/SprintUp/Modules/UserManagement/Features/ContinueWithoutStrava/ContinueWithoutStravaHandler.cs`
**Depends on**: T32
**Reuses**: `GetOnboardingStateHandler`, `User`
**Requirement**: ONB-09, ONB-10, ONB-12, ONB-17

**Done when**:

- [ ] The action is available from `StravaPending` and `SyncPending` and moves onboarding to `CompletedWithPendingSync`.
- [ ] A running job keeps running; the athlete sees its state and can retry.
- [ ] The route guard denies another athlete's onboarding and logs actor and target IDs without PII.
- [ ] Tests cover both source states, no-human-coach completion, and cross-athlete denial (4 tests).
- [ ] Test count: 4 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(onboarding): add continue-without-Strava and route guard`

---

### Phase 6: Invitations

### T34: Implement the Invitation entity and persistence

**What**: Persist single-use invitations bound to exactly one human coach.
**Where**: `src/SprintUp/Modules/Coaching/Domain/Invitation.cs`
**Depends on**: T33, T16
**Reuses**: `Coach`, `CoachAssignment`, `User`
**Requirement**: ONB-13, ONB-14, ONB-16

**Done when**:

- [ ] The invitation stores a token hash, normalized invited email, one coach, optional athlete user, status, and 7-day expiry.
- [ ] `Pending`, `PendingCoachApproval`, `Accepted`, `Expired`, and `Revoked` are persisted with explicit integer values.
- [ ] Unique token hash and `(NormalizedInvitedEmail, Status)` indexes exist.
- [ ] Tests cover token uniqueness, expiry, and coach binding (3 tests).
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(invitations): add Invitation entity and indexes`

---

### T35: Implement CreateInvitationHandler

**What**: Issue invitations from an operator or active coach, always bound to one human coach.
**Where**: `src/SprintUp/Modules/Coaching/Features/CreateInvitationHandler.cs`
**Depends on**: T34, T18
**Reuses**: `Invitation`, `EmailOutbox`, `Coach`
**Requirement**: ONB-13, ONB-14

**Done when**:

- [ ] An operator chooses the coach; a coach can invite only to self; the target coach must be `Active` and human.
- [ ] A pending invitation for the same normalized email and coach is returned instead of duplicated.
- [ ] The token is stored as a hash with a 7-day expiry and the invitation email is enqueued.
- [ ] Tests cover operator, coach-to-self, unauthorized coach, duplicate pending, and expired-coach rejection (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(invitations): add invitation issuance`

---

### T36: Implement AcceptInvitationHandler

**What**: Accept a valid invitation for the same email, link the coach, and start onboarding.
**Where**: `src/SprintUp/Modules/Coaching/Features/AcceptInvitationHandler.cs`
**Depends on**: T35, T17
**Reuses**: `Invitation`, `AssignmentService`, `GetOnboardingStateHandler`
**Requirement**: ONB-13, ONB-14

**Done when**:

- [ ] Acceptance loads the invitation `FOR UPDATE`, checks status and expiry, and commits the athlete link and `Human` assignment together.
- [ ] A consumed, expired, or revoked invitation returns `410 invitation_unavailable` and offers public sign-up or support.
- [ ] A second acceptance cannot create a second assignment; a pre-existing human coach returns `409 human_coach_already_assigned`.
- [ ] Tests cover new-account acceptance, existing-account acceptance, expired, revoked, and second-human conflict (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(invitations): add same-email invitation acceptance`

---

### T37: Implement mismatched-email confirmation and approval

**What**: Move a mismatched invitation to coach approval, then approve or reject it.
**Where**: `src/SprintUp/Modules/Coaching/Features/ApproveInvitationHandler.cs`
**Depends on**: T36
**Reuses**: `Invitation`, `AssignmentService`, `EmailOutbox`
**Requirement**: ONB-16

**Done when**:

- [ ] An email mismatch requires explicit confirmation, records `EmailConfirmedAt` and the athlete user, and moves to `PendingCoachApproval`.
- [ ] Only the inviting coach can approve; approval creates the `Human` assignment and marks `Accepted`; rejection marks `Revoked` and notifies the athlete.
- [ ] Approval is idempotent and guarded by the active-human unique index; an unassigned coach gets `403 coach_not_authorized` and a log entry.
- [ ] Tests cover mismatch confirmation, coach approval, rejection, unassigned coach, and duplicate approval (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(invitations): add mismatched-email approval`

---

### T38: Build the invitation entry page

**What**: Build `/invite` with valid, expired, revoked, used, mismatched-email, and public-sign-up states.
**Where**: `src/SprintUp/Components/Pages/Invite.razor`
**Depends on**: T37, T25
**Reuses**: `Invitation`, `AcceptInvitationHandler`, `SignUp.razor`
**Requirement**: ONB-13, ONB-14, ONB-16

**Done when**:

- [ ] Valid invitations show the prefilled locked email for a new account and a direct accept path for an existing account.
- [ ] Expired, revoked, and used invitations reject entry and offer public sign-up or support.
- [ ] A mismatched authenticated email shows the explicit confirmation and the "waiting for coach approval" state.
- [ ] bUnit tests cover valid-new, valid-existing, expired/revoked/used, mismatch confirmation, and waiting states (6 tests).
- [ ] Test count: 6 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add invitation entry and mismatch states`

---

### Phase 7: Domain Contracts and Lifecycle Core

### T39: Define the provider boundary and neutral DTOs

**What**: Define `IActivityProvider` and provider-neutral contracts so no Strava JSON name leaks into training code.
**Where**: `src/SprintUp/Modules/Integrations/Domain/IActivityProvider.cs`
**Depends on**: T38, T33
**Reuses**: `User`, `OnboardingStatus` from earlier phases
**Requirement**: STR-01, SYN-02, SYN-14

**Done when**:

- [ ] The interface covers authorization URL, code exchange, refresh, paged listing, and revocation.
- [ ] `ProviderActivity`, `ProviderPage`, `ProviderTokens`, and rate-limit metadata carry only fields the spec stores; Strava JSON names stay out.
- [ ] Unit tests cover DTO invariants, scope constants, and canonical-unit field names (3 tests).
- [ ] Test count: 3 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(integrations): define IActivityProvider boundary`

---

### T40: Implement the Subscription aggregate and deletion-scheduling contract

**What**: Implement trial, active, inactive, retention, and deleted transitions, plus the scheduler contract used by lifecycle and deletion handlers.
**Where**: `src/SprintUp/Modules/Subscriptions/Domain/Subscription.cs`
**Depends on**: T39, T38, T33
**Reuses**: `AggregateRoot`, `SprintUpDbContext`, `TimeProvider`
**Requirement**: LIFE-06, LIFE-07

**Done when**:

- [ ] `StartTrial` is idempotent and sets `TrialStartedAt`, `TrialEndsAt = now + 30 days`.
- [ ] `ExpireTrial` moves `Trial -> Inactive` and sets `RetentionEndsAt = now + 30 days`; `BeginRetentionEnd` and `MarkDeleted` follow the approved state model.
- [ ] Absence of a row is the not-started state; read models expose `NotStarted` without inventing a persisted enum value.
- [ ] `IDataDeletionScheduler.ScheduleAsync(userId, scope, dueAt, ct)` is the only way lifecycle and deletion handlers request a purge.
- [ ] `INotificationService.NotifyAsync(userId, kind, payload, ct)` is the contract used by reauthorization and lifecycle workers; the implementation arrives in T69.
- [ ] Tests cover trial start, idempotency, expiry, retention, activation, deletion, invalid transitions, and scheduler contract shape (5 tests).
- [ ] Test count: 5 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(subscriptions): add Subscription aggregate and deletion contract`

---

### T41: Implement StartTrialHandler and subscription queries

**What**: Start the trial at the first `SyncJob` transition to `Running` and expose one read snapshot to other modules.
**Where**: `src/SprintUp/Modules/Subscriptions/Features/StartTrialHandler.cs`
**Depends on**: T40
**Reuses**: `Subscription`, `SprintUpDbContext`, `TimeProvider`
**Requirement**: LIFE-06, LIFE-07

**Done when**:

- [ ] `StartTrialHandler` is idempotent and creates exactly one 30-day trial when no subscription exists; an existing row is untouched.
- [ ] `ISubscriptionQueries.GetSnapshotAsync` returns `NotStarted`, `Trial`, `Active`, `Inactive`, or `Deleted` with the relevant dates.
- [ ] `IEntitlementPolicy` methods for athlete feature use and coach read access are defined against the snapshot.
- [ ] Tests cover first start, repeated start, existing active subscription, and snapshot values (4 tests).
- [ ] Test count: 4 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(subscriptions): add trial-start handler and queries`

---

### T42: Implement the Activity aggregate and provider mapping

**What**: Persist imported activities in canonical units with UTC and local start, raw payload, manual marker, and incomplete metrics.
**Where**: `src/SprintUp/Modules/Training/Domain/Activity.cs`
**Depends on**: T41
**Reuses**: `ProviderActivity`, `SprintUpDbContext`
**Requirement**: SYN-02, SYN-14, SYN-15, SYN-16

**Done when**:

- [ ] Distance is meters, time is seconds, speed is meters per second, and start is stored in UTC plus the athlete's IANA local time.
- [ ] Run, TrailRun, and VirtualRun are running; recognized non-running types are skipped; unknown types are stored as `Unknown` and excluded from running metrics.
- [ ] Manual runs set `IsManual`; optional missing metrics are null, never discarded, and `HasIncompleteMetrics` lists the missing fields.
- [ ] Raw payload is `jsonb`, contains no tokens, and is deleted with the activity; `UpdateSummary` never overwrites `IsHidden` or the SprintUp note.
- [ ] Tests cover canonical units, UTC/local/DST, manual, unknown, non-running, missing metrics, and raw-payload lifecycle (6 tests).
- [ ] Test count: 6 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(training): add Activity aggregate and mapping`

---

### T43: Implement the SyncJob aggregate and persistence

**What**: Implement the sync job state machine, counters, lease, heartbeat, fixed window, and indexes.
**Where**: `src/SprintUp/Modules/Training/Domain/SyncJob.cs`
**Depends on**: T42
**Reuses**: `Activity`, `SprintUpDbContext`
**Requirement**: SYN-01, SYN-04, SYN-08, SYN-13, SYN-19, SYN-21

**Done when**:

- [ ] State transitions cover `Queued -> Running -> Completed | PartiallyCompleted | Failed | Interrupted`; `PartiallyCompleted`, `Failed`, and `Interrupted` allow a new attempt.
- [ ] `NextPage`, processed pages, imported/skipped/failed counts, attempts, lease owner/expiry, heartbeat, error code, and bounded error message are persisted.
- [ ] `WindowEndUtc` is fixed at creation; a partial unique index enforces one active job per athlete; the queue index supports the claim scan.
- [ ] Tests cover transitions, counters, active-job unique index, window immutability, and lease release (4 tests).
- [ ] Test count: 4 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(training): add SyncJob state machine and indexes`

---

### T44: Implement StartInitialSyncHandler

**What**: Enqueue one initial sync for the chosen window and return the existing active job instead of a second.
**Where**: `src/SprintUp/Modules/Training/Features/StartInitialSyncHandler.cs`
**Depends on**: T43
**Reuses**: `SyncJob`, `SprintUpDbContext`
**Requirement**: SYN-01, SYN-13, REV-03, SYN-21

**Done when**:

- [ ] A request while a queued or running job exists returns that job and creates no second job, including under two tabs.
- [ ] A new job fixes `WindowStartUtc` and `WindowEndUtc` from the chosen window (12 months default, 90 days option) at creation.
- [ ] Retry after `PartiallyCompleted` or `Failed` sets `RetryOfJobId` and copies `NextPage`; a fresh "Sync again" after `Completed` starts at page 1.
- [ ] Tests cover new job, existing job, two-tab race, retry-from-page, and completed-resync (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(training): add initial sync start and retry`

---

### Phase 8: Strava Integration

### T45: Implement StravaActivityProvider

**What**: Implement the Strava adapter with recorded fixtures for success and every failure mode, never calling live Strava.
**Where**: `src/SprintUp/Modules/Integrations/Infrastructure/StravaActivityProvider.cs`
**Depends on**: T44, T39
**Reuses**: typed `HttpClient`, `StravaOptions`, `ProviderActivity`
**Requirement**: STR-01, STR-11, SYN-02, SYN-06, SYN-14

**Done when**:

- [ ] The authorization URL requests only `read` and `activity:read_all`; exchange, refresh, list, and revoke use typed `HttpClient` and server-side secrets.
- [ ] `sport_type`, `manual`, `moving_time`, and `start_date_local` are mapped inside the adapter only; Run/TrailRun/VirtualRun are recognized, manual runs marked, recognized non-running skipped, unknown kept for raw storage.
- [ ] Fixtures cover success, 401, 429 with `Retry-After`, 5xx, timeout, short page, empty page, unknown type, and non-running type.
- [ ] Provider contract tests cover the mapping and failure fixtures (8 tests).
- [ ] Test count: 8 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(integrations): add Strava activity provider with fixtures`

---

### T46: Implement ConnectStravaHandler

**What**: Create a one-time OAuth state and return the Strava authorization redirect with the chosen history window.
**Where**: `src/SprintUp/Modules/Integrations/Features/ConnectStravaHandler.cs`
**Depends on**: T45, T30
**Reuses**: `IActivityProvider`, `StravaOAuthState`, `TimeProvider`
**Requirement**: STR-01, STR-14, SYN-01

**Done when**:

- [ ] Only an authenticated `Active` athlete with a complete profile can start; the page discloses that private activities are imported and requests no write scope.
- [ ] A 32-byte state is stored as a SHA-256 hash with a 10-minute expiry, the user ID, and the chosen window (12 months default, 90 days option).
- [ ] The window comes from the state row, so a tampered query string cannot widen it.
- [ ] Tests cover happy path, incomplete profile, state expiry, window options, and scope/disclosure payload (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(strava): add connect handler and OAuth state`

---

### T47: Implement StravaCallbackHandler and connection lifecycle

**What**: Consume state, exchange the code, encrypt tokens, enforce one connection and one Strava account, and enqueue exactly one job.
**Where**: `src/SprintUp/Modules/Integrations/Features/StravaCallbackHandler.cs`
**Depends on**: T46, T43, T44
**Reuses**: `StravaOAuthState`, `StravaConnection`, `IActivityProvider`, `StartInitialSyncHandler`
**Requirement**: STR-02, STR-03, STR-05, STR-06, STR-07, STR-13, SYN-01

**Done when**:

- [ ] A valid state is consumed with a conditional update; missing, invalid, expired, consumed, or other-user state returns `strava_state_invalid`, stores no token, and offers retry.
- [ ] Code exchange happens server-side; both tokens are encrypted; a new connection is `Active`; reconnecting the same Strava athlete refreshes tokens and preserves activities.
- [ ] A Strava athlete ID belonging to another user returns `409 strava_account_already_linked`; a different athlete for this user returns `409 strava_account_switch_blocked`; neither replaces or merges.
- [ ] Connection, tokens, one `Queued` job for the chosen window, and onboarding `SyncPending` commit in one transaction; the active-job unique index guarantees one job.
- [ ] Tests cover valid, reused state, denial, already linked, same-athlete reconnect, different-athlete block, second connection, and one-job enqueue (8 tests).
- [ ] Test count: 8 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(strava): add callback and connection lifecycle`

---

### T48: Implement StravaTokenService

**What**: Refresh rotating tokens under a row lock, mark reauthorization on failure, and keep tokens out of every read/log surface.
**Where**: `src/SprintUp/Modules/Integrations/Infrastructure/StravaTokenService.cs`
**Depends on**: T47
**Reuses**: `StravaConnection`, `IActivityProvider`, `IDataProtector`, `INotificationService`, `EmailOutbox`
**Requirement**: STR-08, STR-09

**Done when**:

- [ ] The service refreshes when the access token expires within five minutes, opens a transaction, loads the row `FOR UPDATE`, re-checks expiry, and persists the rotated refresh token atomically.
- [ ] A refresh `401`/failure is attempted once, marks the connection `NeedsReauthorization`, and notifies the athlete through `INotificationService` and the email outbox.
- [ ] Tokens are Data Protection ciphertext with purpose `SprintUp.StravaTokens.v1`; list queries never select them, responses never serialize them, and logs never contain them.
- [ ] Tests cover proactive refresh, concurrent refresh, failed refresh, notification, and token-absence assertions (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(strava): add token refresh under row lock`

---

### T49: Implement DisconnectStravaHandler

**What**: Revoke the Strava token, mark the connection revoked, interrupt active work, and record the retain-or-delete choice.
**Where**: `src/SprintUp/Modules/Integrations/Features/DisconnectStravaHandler.cs`
**Depends on**: T48
**Reuses**: `StravaConnection`, `IActivityProvider.RevokeAsync`, `SyncJob`, `IDataDeletionScheduler`
**Requirement**: STR-10, SYN-11, LIFE-01

**Done when**:

- [ ] Disconnect requires an explicit retain-or-delete choice; each option explains its effect before confirmation.
- [ ] The handler marks the connection `Revoked`, interrupts any active job, and calls `/oauth/revoke` with Basic Auth, falling back to `/oauth/deauthorize` until 2027-06-01.
- [ ] "Delete history" schedules an immediate `Activities` deletion through `IDataDeletionScheduler`; "retain" keeps activities and notes.
- [ ] Tests cover both choices, revoke fallback, job interruption, and already-revoked idempotency (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(strava): add disconnect, revoke, and history choice`

---

### T50: Build the Strava connection page

**What**: Build `/onboarding/strava` with disclosure, window choice, connection states, and attribution.
**Where**: `src/SprintUp/Components/Pages/Onboarding/Strava.razor`
**Depends on**: T49, T32, T27
**Reuses**: `ConnectStravaHandler`, `DisconnectStravaHandler`, `SharedResources`
**Requirement**: STR-03, STR-04, STR-11, STR-12, STR-14

**Done when**:

- [ ] Before authorization the page discloses private-activity import, shows the read-only scopes, and offers 12 months (default) or 90 days.
- [ ] States cover disconnected, redirecting, authorization denied, invalid state, connected, `NeedsReauthorization`, already linked, different account, and missing `activity:read_all` with an incomplete-history warning.
- [ ] "Powered by Strava" and a Strava link render wherever imported data is shown; imported text is rendered as text, never `MarkupString`.
- [ ] bUnit tests cover disclosure, window choice, denial, invalid state, connected, reauthorization, already linked, different account, and scope warning (7 tests).
- [ ] Test count: 7 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add Strava connection page and states`

---

### Phase 9: Sync Worker

### T51: Implement InitialSyncWorker

**What**: Claim queued jobs atomically, start the trial on first `Running`, and run each job in a fresh scope.
**Where**: `src/SprintUp/Modules/Training/Infrastructure/InitialSyncWorker.cs`
**Depends on**: T50, T44, T41
**Reuses**: `SyncJob`, `StartTrialHandler`, `InitialSyncRunner`
**Requirement**: SYN-01, SYN-13, LIFE-06

**Done when**:

- [ ] The worker polls every 10 seconds and claims the oldest queued job with `FOR UPDATE SKIP LOCKED` plus `UPDATE ... RETURNING`.
- [ ] The claim sets `Running`, lease owner, 2-minute lease, `StartedAt`, and increments `AttemptCount`; the first claim with no subscription calls `StartTrialHandler` exactly once.
- [ ] Per-athlete concurrency is 1 and global concurrency is 2; each job runs in a fresh DI scope.
- [ ] Tests cover claim, trial start at `Running` not enqueue, no double claim, retry does not restart trial, and concurrency limit (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(sync): add worker claim and trial start`

---

### T52: Implement InitialSyncRunner

**What**: Run one job page by page with mapping, idempotent upsert, counters, and page commits.
**Where**: `src/SprintUp/Modules/Training/Infrastructure/InitialSyncRunner.cs`
**Depends on**: T51, T48, T42
**Reuses**: `IActivityProvider`, `StravaTokenService`, `SyncJob`, `Activity`, `IStravaRateLimitGate`
**Requirement**: SYN-02, SYN-03, SYN-04, SYN-05, SYN-07, SYN-08, SYN-09, SYN-14, SYN-15, SYN-16, SYN-21

**Done when**:

- [ ] The runner obtains a valid token, requests `per_page = 200` with `after`/`before`, and continues until an empty page; a short page never stops pagination.
- [ ] Each page is mapped and upserted by `(UserId, Source, SourceActivityId)`; existing rows update Strava fields and preserve `IsHidden` and the SprintUp note.
- [ ] After each page it increments `NextPage`, `ProcessedPages`, and counters, writes the heartbeat, extends the lease, and commits; a crash loses at most the current page.
- [ ] A clean pass sets `Completed` with imported/skipped/failed counts and duration; zero eligible activities also set `Completed`; no full history is materialized.
- [ ] A page failure after retries preserves earlier pages, sets `PartiallyCompleted`, records the error code and `NextPage`, and leaves a retry path.
- [ ] Tests cover eligible/manual/unknown/non-running, short-page-then-full, empty page, duplicates, updates, counts, zero activities, page failure, and DST local start (9 tests).
- [ ] Test count: 9 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(sync): add paginated idempotent runner`

---

### T53: Implement the Strava retry policy and rate-limit gate

**What**: Honor `429` and `Retry-After`, retry `5xx`/timeouts with backoff, and cap each request at five attempts.
**Where**: `src/SprintUp/Modules/Training/Infrastructure/StravaRetryPolicy.cs`
**Depends on**: T52
**Reuses**: `IActivityProvider`, `IStravaRateLimitGate`, `TimeProvider`
**Requirement**: SYN-06, SYN-07, SYN-12

**Done when**:

- [ ] A `429` waits at least `Retry-After`; exponential backoff with jitter applies; no request exceeds five attempts.
- [ ] `5xx` and timeout use the same policy; a page that still fails preserves earlier pages, sets `PartiallyCompleted`, records `ErrorCode` and `NextPage`, and allows retry from that page.
- [ ] A failure that prevents any progress sets `Failed`; a new attempt never redoes committed pages.
- [ ] Tests cover `Retry-After`, backoff cap, page partial failure, total failure, and retry skip (5 tests).
- [ ] Test count: 5 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(sync): add rate-limit gate and retry policy`

---

### T54: Implement heartbeat, interruption, and the sweeper

**What**: Detect dead workers, stop jobs on disconnect or deletion, and preserve imported data.
**Where**: `src/SprintUp/Modules/Training/Infrastructure/SyncJobSweeper.cs`
**Depends on**: T53
**Reuses**: `SyncJob`, `StravaConnection`, `User`, `Subscription`, `TimeProvider`
**Requirement**: SYN-10, SYN-11, SYN-19, LIFE-04

**Done when**:

- [ ] A `Running` job with `LastHeartbeatAt` older than 10 minutes is marked `Failed` with `worker_heartbeat_lost` and its lease is released.
- [ ] Before each page and commit, the runner re-reads the connection and account state; a revoked connection, disconnect, or deletion marks the job `Interrupted` and commits nothing further.
- [ ] Already imported activities are preserved in every interruption path; expired OAuth states and consumed tokens older than 24 hours are removed.
- [ ] Tests cover stale heartbeat, lease release, disconnect interruption, deletion interruption, and cleanup (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(sync): add heartbeat sweeper and interruption`

---

### T55: Implement sync status and structured logging

**What**: Expose the job projection used by the athlete and coach and log only safe structured fields.
**Where**: `src/SprintUp/Modules/Training/Features/GetSyncStatusHandler.cs`
**Depends on**: T54
**Reuses**: `SyncJob`, `ILogger`, `ICurrentUser`
**Requirement**: SYN-08, SYN-10, SYN-11, SYN-12, SYN-17, SYN-22

**Done when**:

- [ ] The status DTO returns state, counters, current page, error code, retryability, and whether the job is still running.
- [ ] Structured logs include job ID, counts, duration, and error code, and never tokens, names, coordinates, or health metrics.
- [ ] A job state change is visible to the coach dashboard and sends no external notification in the MVP.
- [ ] `ISyncStatusQueries` exposes the job state to Coaching without exposing `DbSet`s.
- [ ] Tests cover each state projection, retryability, safe log fields, no outbound notification, and the query contract (6 tests).
- [ ] Test count: 6 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(sync): add status projection and safe logging`

---

### T56: Build the sync progress page

**What**: Build `/onboarding/sync` with every job state, live counters, retry, and continue-with-pending action.
**Where**: `src/SprintUp/Components/Pages/Onboarding/Sync.razor`
**Depends on**: T55, T50
**Reuses**: `GetSyncStatusHandler`, `StartInitialSyncHandler`, `SharedResources`
**Requirement**: SYN-09, SYN-10, SYN-11, SYN-12, SYN-20, SYN-22

**Done when**:

- [ ] States cover `Queued`, `Running`, `Completed`, `PartiallyCompleted`, `Failed`, `Interrupted`, zero activities, and unknown-type warnings.
- [ ] Running shows status and imported count in a live region; navigating away or closing the browser does not stop the job, and returning reattaches.
- [ ] Retry starts from `NextPage`; "continue with pending sync" moves onboarding to `CompletedWithPendingSync`; error text is localized and includes a job reference for support.
- [ ] Imported activity names/descriptions are encoded as text; no `MarkupString` is used for Strava content.
- [ ] bUnit tests cover every state, live count, retry, continue, empty, and escaping (8 tests).
- [ ] Test count: 8 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add sync progress states and retry`

---

### Phase 10: Review and Coach

### T57: Implement ReviewHistoryHandler

**What**: Aggregate visible activities into totals and the 20 most recent rows with Strava links.
**Where**: `src/SprintUp/Modules/Training/Features/ReviewHistoryHandler.cs`
**Depends on**: T56
**Reuses**: `Activity`, `SyncJob`, `SprintUpDbContext`, `ICurrentUser`
**Requirement**: REV-01, REV-02

**Done when**:

- [ ] The result returns activity count, covered date range, total distance (nulls ignored), and total time.
- [ ] The list returns the 20 most recent visible activities with date, type, distance, duration, incomplete marker, and `https://www.strava.com/activities/{id}` link.
- [ ] Hidden activities are excluded; no raw payload is materialized; zero activities returns an empty result without error.
- [ ] `ITrainingQueries` exposes visible activity summaries, counts, and health-data presence to Coaching without exposing `DbSet`s.
- [ ] Tests cover totals, 20-row cap, ordering, links, hidden exclusion, null metrics, empty history, and the query contract (6 tests).
- [ ] Test count: 6 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(review): add history review query`

---

### T58: Implement the ActivityNote entity and persistence

**What**: Store one SprintUp-owned note per activity, separate from every imported field.
**Where**: `src/SprintUp/Modules/Training/Domain/ActivityNote.cs`
**Depends on**: T57
**Reuses**: `Activity`, `SprintUpDbContext`
**Requirement**: REV-05

**Done when**:

- [ ] A note stores activity, owner, body, and timestamps; the body is limited to 2,000 characters.
- [ ] `ActivityNotes(ActivityId)` is unique; the note has no column on `Activity`.
- [ ] Tests cover one-note uniqueness, length bound, and separation from imported fields (3 tests).
- [ ] Test count: 3 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(review): add ActivityNote persistence`

---

### T59: Implement UpdateActivityNoteHandler

**What**: Allow the owner to create or edit the SprintUp note while imported metrics stay read-only.
**Where**: `src/SprintUp/Modules/Training/Features/UpdateActivityNoteHandler.cs`
**Depends on**: T58
**Reuses**: `ActivityNote`, `Activity`, `ICurrentUser`
**Requirement**: REV-05

**Done when**:

- [ ] Only the owning athlete can create or edit a note; a coach has no write path.
- [ ] Any attempt to change an imported metric is rejected and leaves the activity unchanged.
- [ ] Plain text up to 2,000 characters is stored; HTML is stored as text and never rendered as markup.
- [ ] Tests cover create, edit, owner denial, coach denial, metric rejection, and length bound (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(review): add note editing and read-only metrics`

---

### T60: Implement HideActivityHandler

**What**: Hide or show an activity for the owner without deleting the row or its payload.
**Where**: `src/SprintUp/Modules/Training/Features/HideActivityHandler.cs`
**Depends on**: T59
**Reuses**: `Activity`, `ICurrentUser`
**Requirement**: REV-06

**Done when**:

- [ ] Only the owner can toggle `IsHidden`; the row, note, and raw payload remain.
- [ ] Default athlete and coach queries exclude hidden rows; an explicit restore brings the row back.
- [ ] Tests cover hide, show, owner denial, coach denial, and default-view filtering (4 tests).
- [ ] Test count: 4 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(review): add activity hiding`

---

### T61: Build the review page

**What**: Build `/onboarding/review` with totals, recent activities, notes, hiding, empty state, and sync again.
**Where**: `src/SprintUp/Components/Pages/Onboarding/Review.razor`
**Depends on**: T60, T56
**Reuses**: `ReviewHistoryHandler`, `UpdateActivityNoteHandler`, `HideActivityHandler`, `StartInitialSyncHandler`, `SharedResources`
**Requirement**: REV-01, REV-02, REV-03, REV-05, REV-06, SYN-09, SYN-15

**Done when**:

- [ ] The page shows count, date range, total distance, total time, and the 20 most recent activities with Strava links.
- [ ] Imported metrics are displayed read-only; only the SprintUp note is editable; hide removes the row from the default view.
- [ ] Zero activities shows an explanatory empty state with "Sync again", not an error; incomplete metrics show a marker and an em dash per missing value.
- [ ] bUnit tests cover populated, empty, incomplete marker, read-only metrics, note edit, hide, and sync again (8 tests).
- [ ] Test count: 8 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add review page with notes and hiding`

---

### T62: Implement ListAssignedAthletesHandler

**What**: Return only the coach's assigned athletes with onboarding, sync, activity, subscription, and readiness state.
**Where**: `src/SprintUp/Modules/Coaching/Features/ListAssignedAthletesHandler.cs`
**Depends on**: T61, T41
**Reuses**: `ITrainingQueries`, `ISubscriptionQueries`, `ISyncStatusQueries`, `CoachAssignment`
**Requirement**: REV-04, SYN-22, LIFE-07

**Done when**:

- [ ] Rows come from active `CoachAssignment` rows and never from another module's tables directly.
- [ ] Each row shows onboarding state, sync state, imported count, last activity date, subscription state, and `Ready`.
- [ ] `Ready` is false while a job is queued or running, or while a partial/failed job has not been retried.
- [ ] Tests cover assigned-only, ready false during sync, ready true after completion, inactive subscription, and revoked assignment (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(coach): add assigned-athlete list and readiness`

---

### T63: Implement GetAthleteOverviewHandler

**What**: Return the coach's read-only athlete overview with training context, history, notes, and subscription state.
**Where**: `src/SprintUp/Modules/Coaching/Features/GetAthleteOverviewHandler.cs`
**Depends on**: T62
**Reuses**: `ITrainingQueries`, `ISubscriptionQueries`, `ISyncStatusQueries`, `CoachAssignment`, `ConsentRecord`
**Requirement**: REV-02, REV-05, REV-06, SYN-18, LIFE-07

**Done when**:

- [ ] The overview contains status, counts, sync state, subscription state, recent visible activities, and notes.
- [ ] Optional health fields appear only with active `Health` consent and an active assignment; tokens are never included.
- [ ] Hidden activities are excluded; every state is read-only; an unassigned coach or a deleted athlete gets `403` with an audit entry.
- [ ] Tests cover assigned read, health consent on/off, hidden exclusion, revoked assignment, and deleted athlete (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(coach): add read-only athlete overview`

---

### T64: Build the coach dashboard and athlete detail pages

**What**: Build `/coach` and `/coach/athletes/{id}` with status, readiness, read-only history, and incomplete markers.
**Where**: `src/SprintUp/Components/Pages/Coach/`
**Depends on**: T63
**Reuses**: `ListAssignedAthletesHandler`, `GetAthleteOverviewHandler`, `SharedResources`
**Requirement**: REV-02, REV-04, REV-05, REV-06, SYN-22, LIFE-07

**Done when**:

- [ ] The dashboard lists only assigned athletes and shows "Sync in progress" without marking them ready during an incomplete sync.
- [ ] The detail page is read-only in every subscription state, shows notes and incomplete-data markers, and never offers an edit or hide control.
- [ ] Revoked access renders the access-revoked state without leaking data.
- [ ] bUnit tests cover dashboard list, in-progress, ready, detail read-only, incomplete marker, and revoked state (6 tests).
- [ ] Test count: 6 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add coach dashboard and read-only detail`

---

### Phase 11: Lifecycle and Deletion

### T65: Implement SubscriptionLifecycleWorker

**What**: Run hourly trial expiry, retention end, and onboarding-abandonment notices and deletion.
**Where**: `src/SprintUp/Modules/Subscriptions/Infrastructure/SubscriptionLifecycleWorker.cs`
**Depends on**: T64, T41
**Reuses**: `Subscription`, `ISubscriptionQueries`, `INotificationService`, `IDataDeletionScheduler`, `AssignmentService`, `SyncJob`, `User`, `TimeProvider`
**Requirement**: LIFE-05, LIFE-06, LIFE-07

**Done when**:

- [ ] A `TrialExpiring` notice is sent when fewer than 48 hours remain; at `TrialEndsAt` the subscription becomes `Inactive`, `RetentionEndsAt = now + 30 days`, and `TrialExpired` is sent.
- [ ] Seven days before retention end a `RetentionEnding` notice is sent; at retention end the subscription becomes `Deleted`, the user becomes `Inactive`, the security stamp rotates, assignments are revoked, active jobs are interrupted, and an account purge is enqueued.
- [ ] Onboarding abandoned for 23 days gets one notice; at 30 days an account purge is enqueued; a return resets the timestamp and clears the reminder.
- [ ] Tests with `FakeTimeProvider` cover trial notice/expiry, retention notice/end, day-23/day-30 abandonment, and idempotency (6 tests).
- [ ] Test count: 6 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(lifecycle): add subscription and abandonment worker`

---

### T66: Implement entitlement and resource-access policies

**What**: Enforce inactive read-only access, coach read-only access, and cross-athlete/coach denial with audit logging.
**Where**: `src/SprintUp/Modules/Subscriptions/Domain/EntitlementPolicy.cs`
**Depends on**: T65
**Reuses**: `ISubscriptionQueries`, `CoachAssignment`, `ConsentRecord`, `ILogger`
**Requirement**: ONB-09, SYN-18, LIFE-06, LIFE-07

**Done when**:

- [ ] While `Inactive`, the athlete can read, disconnect Strava, and delete the account; sync, note edits, hiding, and new connections return `403 subscription_inactive`.
- [ ] A human coach can read only while assigned and the subscription is not `Deleted`; every write is denied in every state.
- [ ] A cross-athlete or unassigned-coach attempt is denied and logged with actor ID, target ID, and route, never PII.
- [ ] Activity visibility is limited to the athlete and the assigned coach, with health fields gated by consent.
- [ ] Tests cover every allowed/denied action, inactive coach read, deletion revocation, cross-athlete denial, and audit fields (6 tests).
- [ ] Test count: 6 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(lifecycle): add entitlement and access policies`

---

### T67: Implement RequestAccountDeletionHandler

**What**: Revoke access immediately, stop active work, and schedule the account purge without inactivity retention.
**Where**: `src/SprintUp/Modules/Subscriptions/Features/RequestAccountDeletionHandler.cs`
**Depends on**: T66
**Reuses**: `User`, `Subscription`, `SyncJob`, `AssignmentService`, `IDataDeletionScheduler`
**Requirement**: LIFE-03, LIFE-04, LIFE-07

**Done when**:

- [ ] A typed confirmation is required; on success the user becomes `Inactive`, the security stamp rotates, all coach assignments are revoked, and active jobs are marked `Interrupted`.
- [ ] The subscription moves to `Deleted` and an `Account` deletion job is enqueued due immediately, bypassing the 30-day retention.
- [ ] The response confirms deletion; no athlete or coach session remains valid.
- [ ] Tests cover confirmation required, immediate revocation, job interruption, purge scheduling, and coach access denial (5 tests).
- [ ] Test count: 5 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(lifecycle): add explicit account deletion request`

---

### T68: Implement DataDeletionJob and the purge worker

**What**: Process activity and account deletions, including the PII-free tombstone and the 30-day guarantee.
**Where**: `src/SprintUp/Modules/Subscriptions/Infrastructure/DataDeletionWorker.cs`
**Depends on**: T67
**Reuses**: `DataDeletionJob`, `Activity`, `ActivityNote`, `StravaConnection`, `UserToken`, `ConsentRecord`, `AthleteProfile`, `UserNotification`, `EmailOutbox`, `IDataDeletionScheduler`
**Requirement**: LIFE-02, LIFE-03, LIFE-05

**Done when**:

- [ ] The worker processes due jobs every 15 minutes; `Activities` deletes activities, raw payloads, and notes; `Account` deletes activities, notes, tokens, connections, consent rows, profile, notifications, and jobs.
- [ ] Account purge replaces the email with `deleted+{id}@invalid`, clears the password hash, sets `Status = Deleted`, and keeps only the ID and timestamps as a tombstone.
- [ ] Deletion completes well inside 30 days, is idempotent, and records confirmation; a failed attempt increments `AttemptCount` and retries.
- [ ] Tests cover activity deletion, account purge, tombstone shape, confirmation, idempotency, and retry (6 tests).
- [ ] Test count: 6 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(lifecycle): add deletion jobs and tombstone purge`

---

### T69: Implement the notification service

**What**: Deliver in-app notifications and the transactional emails required by lifecycle and reauthorization events.
**Where**: `src/SprintUp/Shared/Infrastructure/NotificationService.cs`
**Depends on**: T68
**Reuses**: `INotificationService`, `UserNotification`, `EmailOutbox`, `SharedResources`
**Requirement**: LIFE-05, LIFE-06, STR-08

**Done when**:

- [ ] In-app notifications store kind, payload, read state, and timestamp; the authenticated layout exposes an unread badge.
- [ ] Trial expiry, retention ending, onboarding abandonment, and Strava reauthorization send both in-app and email notices through the outbox.
- [ ] Coach notifications stay in-app; no external coach notification is sent in the MVP; templates are localized in PT-BR and EN.
- [ ] Tests cover in-app creation, email enqueue, unread badge data, and no external coach send (4 tests).
- [ ] Test count: 4 integration tests pass.

**Tests**: integration
**Gate**: full

**Commit**: `feat(notifications): add in-app and lifecycle email delivery`

---

### T70: Build the lifecycle page

**What**: Build `/account/lifecycle` with subscription state, retention notice, disconnect choice, and account deletion.
**Where**: `src/SprintUp/Components/Pages/Account/Lifecycle.razor`
**Depends on**: T69, T49
**Reuses**: `RequestAccountDeletionHandler`, `DisconnectStravaHandler`, `ISubscriptionQueries`
**Requirement**: LIFE-01, LIFE-02, LIFE-03, LIFE-06, STR-10

**Done when**:

- [ ] States cover active trial, expired trial, `Inactive`, retention notice, explicit deletion, and read-only coach access.
- [ ] Disconnect offers "retain history" or "delete history" with the effect of each explained before confirmation.
- [ ] Account deletion requires typed confirmation and states the immediate access revocation and 30-day purge guarantee.
- [ ] Inactive state explains what remains available (read, disconnect, delete) and what is blocked (sync, notes).
- [ ] bUnit tests cover trial, expired, inactive, retention, disconnect choice, and delete confirmation (6 tests).
- [ ] Test count: 6 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add lifecycle, retention, and deletion page`

---

### T71: Build the coach invitations page

**What**: Build `/coach/invitations` for creating invitations and approving mismatched emails.
**Where**: `src/SprintUp/Components/Pages/Coach/Invitations.razor`
**Depends on**: T70, T37
**Reuses**: `CreateInvitationHandler`, `ApproveInvitationHandler`
**Requirement**: ONB-16, LIFE-07

**Done when**:

- [ ] An active coach can invite to self and see pending invitations; an operator can choose the coach.
- [ ] Mismatched-email invitations show the athlete and offer approve/reject; approval updates the row and rejection notifies the athlete.
- [ ] An unassigned coach sees no approval controls and receives the unauthorized state.
- [ ] bUnit tests cover create, pending list, approve, reject, and unauthorized (4 tests).
- [ ] Test count: 4 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add coach invitation management`

---

### T72: Build the admin support page

**What**: Build `/admin/sync-jobs` with job metadata, counts, and error codes only.
**Where**: `src/SprintUp/Components/Pages/Admin/SyncJobs.razor`
**Depends on**: T71, T55
**Reuses**: `ISyncStatusQueries`, `ICurrentUser`
**Requirement**: SYN-17

**Done when**:

- [ ] Admin-only page lists job ID, athlete reference, state, counts, duration, and error code.
- [ ] No name, email, coordinate, token, or health value appears; support sees metadata only.
- [ ] Access by a non-admin is denied and logged.
- [ ] bUnit tests cover populated, failed job, no-PII fields, and non-admin denial (4 tests).
- [ ] Test count: 4 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(ui): add admin sync-job support view`

---

### Phase 12: Experience and Hardening

### T73: Complete PT-BR and EN resources

**What**: Complete every onboarding, validation, email, and error key in both languages with parity checks.
**Where**: `src/SprintUp/Shared/Resources/SharedResources.pt-BR.resx`
**Depends on**: T72
**Reuses**: `SharedResources.resx`, `SharedResources.pt-BR.resx`, `EmailOutbox` templates
**Requirement**: UX-03, ONB-02

**Done when**:

- [ ] Every onboarding screen, validation message, email subject/body, and stable error code has PT-BR and EN values.
- [ ] PT-BR is the default; EN is complete for onboarding; a missing key fails a parity test.
- [ ] The duplicate-email message keeps the exact required wording in both languages.
- [ ] Tests cover resource parity and PT-BR default selection (3 tests).
- [ ] Test count: 3 UI tests pass.

**Tests**: ui
**Gate**: full

**Commit**: `feat(i18n): complete PT-BR and EN onboarding resources`

---

### T74: Implement canonical metric and imperial display

**What**: Format canonical metric values for display without ever rewriting stored values.
**Where**: `src/SprintUp/Shared/Units/UnitFormatter.cs`
**Depends on**: T73, T31
**Reuses**: `UnitPreference`, `Activity`, `AthleteProfile`
**Requirement**: UX-03, SYN-14

**Done when**:

- [ ] Distance, pace/speed, elevation, weight, and height convert for `Imperial`; `Metric` renders canonical values.
- [ ] Switching units re-displays from storage and changes no activity or profile row.
- [ ] Activity start renders in the athlete's time zone across a DST change.
- [ ] Tests cover metric, imperial, round-trip, unit switch immutability, and DST (5 tests).
- [ ] Test count: 5 unit tests pass.

**Tests**: unit
**Gate**: quick

**Commit**: `feat(units): add metric and imperial display formatting`

---

### T75: Add the accessibility proof suite

**What**: Prove WCAG 2.1 AA on every onboarding screen with keyboard, focus, labels, live regions, and axe.
**Where**: `tests/SprintUp.E2E/Accessibility/OnboardingAccessibilityTests.cs`
**Depends on**: T74
**Reuses**: `SprintUp.E2E` Playwright fixture, onboarding pages from Phases 4-10
**Requirement**: UX-01

**Done when**:

- [ ] A keyboard-only run completes entry, sign-up, verification, profile, Strava, sync, and review.
- [ ] Visible focus, input labels, error-summary links, semantic headings, live-region status, and no color-only meaning are asserted.
- [ ] Axe reports zero violations on every onboarding screen; a manual screen-reader checklist is recorded.
- [ ] Test count: 1 end-to-end accessibility test plus the axe screen sweep passes.

**Tests**: e2e
**Gate**: full

**Commit**: `test(a11y): add keyboard and axe onboarding proof`

---

### T76: Add the responsive viewport proof suite

**What**: Prove every onboarding screen works at 360 px and 1440 px without horizontal scrolling.
**Where**: `tests/SprintUp.E2E/Viewports/OnboardingViewportTests.cs`
**Depends on**: T75
**Reuses**: `SprintUp.E2E` Playwright fixture, `OnboardingAccessibilityTests` page helpers
**Requirement**: UX-02

**Done when**:

- [ ] Every onboarding screen is rendered at 360 px and 1440 px and asserts `scrollWidth <= clientWidth`.
- [ ] Tables become stacked cards at 360 px and controls remain reachable.
- [ ] Screenshots for both widths are attached to the test report.
- [ ] Test count: 2 viewport tests (360 and 1440) pass.

**Tests**: e2e
**Gate**: full

**Commit**: `test(ui): add 360 and 1440 viewport proof`

---

### T77: Add the end-to-end onboarding and security suite

**What**: Prove the complete athlete journey and the security properties that unit tests cannot.
**Where**: `tests/SprintUp.E2E/Onboarding/OnboardingFlowTests.cs`
**Depends on**: T76
**Reuses**: `SprintUp.E2E` Playwright fixture, `StravaActivityProvider` fixtures, onboarding pages
**Requirement**: SYN-09, SYN-10, SYN-20, STR-12, UX-03

**Done when**:

- [ ] The full path runs: invitation or public sign-up, verification, profile, Strava connect, sync, review, disconnect, and delete.
- [ ] Zero activities shows the empty state; leaving the sync screen keeps the job running; imported screens show "Powered by Strava" and links.
- [ ] PT-BR/EN and metric/imperial switches keep canonical values; a script payload in Strava text renders escaped; no token appears in a response or log.
- [ ] Test count: 8 end-to-end tests pass.

**Tests**: e2e
**Gate**: full

**Commit**: `test(e2e): add onboarding and security flow`

---

### T78: Add the initial-sync performance proof

**What**: Prove 12 months and 10,000 activities import within the p95 budget with zero duplicates.
**Where**: `tests/SprintUp.PerformanceTests/InitialSyncPerformanceTests.cs`
**Depends on**: T77
**Reuses**: `InitialSyncRunner`, stub `IActivityProvider`, `SprintUp.PerformanceTests` fixture
**Requirement**: SYN-08, Q-25

**Done when**:

- [ ] The stub serves 50 pages / 10,000 activities and the runner completes with p95 under 3 minutes.
- [ ] Three consecutive syncs of the same window produce zero duplicate activities.
- [ ] The run records elapsed time, page count, imported/skipped/failed counts, and memory in the test artifact.
- [ ] Test count: 1 performance test passes.

**Tests**: performance
**Gate**: performance

**Commit**: `test(perf): add 10k-activity sync proof`

---

### T79: Write the runbook and prerequisite guide

**What**: Document local setup, secrets, migrations, workers, Data Protection backup, retention operations, and the quality gates in the operations runbook linked from the README.
**Where**: `docs/runbook.md`
**Depends on**: T78
**Reuses**: `Makefile`, `docker-compose.yml`, `src/SprintUp/appsettings.json`
**Requirement**: AD-002, AD-004

**Done when**:

- [ ] Prerequisites name Docker, .NET SDK `10.0.113`, Node/Playwright browsers, and the `TEST_DB_CONNECTION` fallback.
- [ ] Environment variables cover the connection string, Strava client ID/secret, Data Protection path, and admin bootstrap secret; no secret is committed.
- [ ] The runbook covers migrations, workers, backup/restore of the key ring, retention/deletion operations, and `make` gates.
- [ ] `README.md` links to the runbook and keeps only the product overview, stack, layout, and documentation links.
- [ ] Test count: 0 (documentation only).

**Tests**: none
**Gate**: build

**Commit**: `docs: add operations runbook and prerequisites`

---

## Requirement Coverage

Every acceptance criterion in `spec.md` maps to at least one implementation task and one check below.

| Requirement | Tasks | Check |
| ----------- | ----- | ----- |
| ONB-01 | T14, T20, T25 | C1 |
| ONB-02 | T12, T20, T25 | C2 |
| ONB-03 | T21, T26, T30 | C3 |
| ONB-04 | T21, T22, T26 | C4 |
| ONB-05 | T11, T12, T29 | C5 |
| ONB-06 | T28, T29, T32 | C6 |
| ONB-07 | T28, T29, T32 | C7 |
| ONB-08 | T30, T32 | C8 |
| ONB-09 | T33, T66 | C9 |
| ONB-10 | T29, T33 | C10 |
| ONB-11 | T28, T29, T32 | C11 |
| ONB-12 | T30, T33 | C12 |
| ONB-13 | T34, T35, T36, T38 | C13 |
| ONB-14 | T34, T35, T36, T38 | C14 |
| ONB-15 | T15, T17, T20 | C15 |
| ONB-16 | T16, T17, T34, T37, T38, T71 | C16 |
| ONB-17 | T15, T16, T17, T30, T33 | C17 |
| STR-01 | T39, T45, T46 | C18 |
| STR-02 | T47 | C19 |
| STR-03 | T47, T50 | C20 |
| STR-04 | T50 | C21 |
| STR-05 | T47 | C22 |
| STR-06 | T47 | C23 |
| STR-07 | T47 | C24 |
| STR-08 | T48, T69 | C25 |
| STR-09 | T48 | C26 |
| STR-10 | T49, T70 | C27 |
| STR-11 | T45, T50 | C28 |
| STR-12 | T50, T77 | C29 |
| STR-13 | T47 | C30 |
| STR-14 | T46, T50 | C31 |
| SYN-01 | T43, T44, T46, T47, T51 | C32 |
| SYN-02 | T39, T42, T45, T52 | C33 |
| SYN-03 | T52 | C34 |
| SYN-04 | T43, T52 | C35 |
| SYN-05 | T52 | C36 |
| SYN-06 | T45, T53 | C37 |
| SYN-07 | T52, T53 | C38 |
| SYN-08 | T43, T52, T55, T78 | C39 |
| SYN-09 | T52, T56, T61, T77 | C40 |
| SYN-10 | T54, T55, T56, T77 | C41 |
| SYN-11 | T49, T54, T55, T56 | C42 |
| SYN-12 | T53, T55, T56 | C43 |
| SYN-13 | T43, T44, T51 | C44 |
| SYN-14 | T39, T42, T52, T74 | C45 |
| SYN-15 | T42, T52, T61 | C46 |
| SYN-16 | T42, T52 | C47 |
| SYN-17 | T55, T72 | C48 |
| SYN-18 | T63, T66 | C49 |
| SYN-19 | T43, T54 | C50 |
| SYN-20 | T56, T77 | C51 |
| SYN-21 | T43, T44, T52 | C52 |
| SYN-22 | T55, T62, T64 | C53 |
| REV-01 | T57, T61 | C54 |
| REV-02 | T57, T61, T63 | C55 |
| REV-03 | T44, T61 | C56 |
| REV-04 | T62, T64 | C57 |
| REV-05 | T58, T59, T61, T63 | C58 |
| REV-06 | T60, T61, T63 | C59 |
| LIFE-01 | T49, T70 | C60 |
| LIFE-02 | T68, T70 | C61 |
| LIFE-03 | T67, T68 | C62 |
| LIFE-04 | T54, T67 | C63 |
| LIFE-05 | T65, T68, T69 | C64 |
| LIFE-06 | T40, T41, T51, T65, T66, T70 | C65 |
| LIFE-07 | T40, T41, T62, T63, T64, T65, T66, T67 | C66 |
| UX-01 | T75 | C67 |
| UX-02 | T76 | C68 |
| UX-03 | T31, T73, T74, T77 | C69 |

---

## Checks

**C1** - A new athlete is created in `PendingVerification` and the verification email is sent within 60 seconds (ONB-01, AC 1)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~RegisterAthleteHandlerTests.Register_EnqueuesVerification_ThatWorkerSendsWithin60Seconds"`

**C2** - A duplicate email returns `409 email_already_registered` with the exact required message and leaves one account (ONB-02, AC 2)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~RegisterAthleteHandlerTests.Register_DuplicateEmail_Returns409AndKeepsSingleAccount"`

**C3** - A valid verification token activates the account and resumes at the first incomplete step (ONB-03, AC 3)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~VerifyEmailHandlerTests.Verify_ValidToken_ActivatesAndResumesAtFirstIncompleteStep"`

**C4** - An expired or used token is rejected and a resend invalidates previous links (ONB-04, AC 4)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~VerifyEmailHandlerTests.ExpiredOrUsedToken_Rejected_AndResendInvalidatesPrevious"`

**C5** - Accepting a consent records user, type, document version, and timestamp (ONB-05, AC 5)
Proof: `dotnet test tests/SprintUp.UnitTests/SprintUp.UnitTests.csproj --no-build --filter "FullyQualifiedName~ConsentRecordTests.Accept_RecordsUserTypeDocumentVersionAndTimestamp"`

**C6** - Missing any required profile field blocks the Strava connection step (ONB-06, AC 1)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~CompleteProfileHandlerTests.Complete_MissingRequiredField_BlocksStravaConnection"`

**C7** - Optional profile fields can be skipped without blocking onboarding (ONB-07, AC 2)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~CompleteProfileHandlerTests.Complete_WithoutOptionalFields_DoesNotBlockOnboarding"`

**C8** - After logout, session expiry, or browser close, resume returns the saved step and data (ONB-08, AC 3)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~GetOnboardingStateHandlerTests.Resume_AfterLogout_ReturnsSavedStepAndData"`

**C9** - A cross-athlete onboarding or data access is denied and logged (ONB-09, AC 4)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ResourceAccessGuardTests.CrossAthleteAccess_DeniedAndLogged"`

**C10** - A completed required profile advances onboarding to the Strava step (ONB-10, AC 5)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~CompleteProfileHandlerTests.Complete_RequiredFields_AdvancesToStravaPending"`

**C11** - Out-of-bounds profile data returns per-field errors and preserves the entered values (ONB-11, AC 6)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~CompleteProfileHandlerTests.InvalidBounds_ReturnsPerFieldErrors_AndPreservesValues"`

**C12** - Continuing without Strava sets `CompletedWithPendingSync` without requiring a human coach (ONB-12, AC 7)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ContinueWithoutStravaHandlerTests.Continue_SetsCompletedWithPendingSync_WithoutHumanCoach"`

**C13** - A valid invitation links the new athlete to the inviting coach and starts onboarding (ONB-13, AC 6)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~AcceptInvitationHandlerTests.Accept_ValidInvitation_LinksCoachAndStartsOnboarding"`

**C14** - An expired, revoked, or used invitation rejects entry and offers public sign-up or support (ONB-14, AC 7)
Proof: `dotnet test tests/SprintUp.UiTests/SprintUp.UiTests.csproj --no-build --filter "FullyQualifiedName~InvitePageTests.UnavailableInvitation_RejectsEntry_AndOffersPublicSignupOrSupport"`

**C15** - Public sign-up assigns AI as primary and completes without a human coach (ONB-15, AC 8)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~RegisterAthleteHandlerTests.Register_WithoutInvitation_AssignsAiCoach_AndCompletes"`

**C16** - A mismatched invitation requires explicit confirmation and stays pending until the coach approves it (ONB-16, AC 9)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ApproveInvitationHandlerTests.MismatchedEmail_RequiresConfirmation_AndCoachApproval"`

**C17** - AI is primary for every athlete, a second human coach is rejected, and onboarding never blocks on a human coach (ONB-17, AC 10)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~AssignmentServiceTests.SecondHumanCoach_Rejected_AiRemainsPrimary"`

**C18** - Connect creates a one-time `state` and redirects with only `read` and `activity:read_all` (STR-01, AC 1)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ConnectStravaHandlerTests.Connect_CreatesOneTimeState_AndRequestsReadScopes"`

**C19** - A valid callback exchanges the code server-side, stores encrypted tokens, and marks the connection `Active` (STR-02, AC 2)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StravaCallbackHandlerTests.Callback_ValidState_ExchangesCodeAndStoresEncryptedActiveConnection"`

**C20** - A missing, invalid, expired, or used `state` is rejected and stores no token (STR-03, AC 3)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StravaCallbackHandlerTests.Callback_ReusedState_Rejected_AndNoTokensStored"`

**C21** - Authorization denial creates no connection and explains the required data (STR-04, AC 4)
Proof: `dotnet test tests/SprintUp.UiTests/SprintUp.UiTests.csproj --no-build --filter "FullyQualifiedName~StravaPageTests.AuthorizationDenied_ShowsDataExplanation_AndCreatesNoConnection"`

**C22** - A Strava athlete already linked elsewhere is rejected with no merge and a support path (STR-05, AC 5)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StravaCallbackHandlerTests.Callback_AthleteLinkedToAnotherUser_Returns409AndRollsBack"`

**C23** - Reconnecting the same Strava account refreshes tokens and preserves imported activities (STR-06, AC 6)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StravaCallbackHandlerTests.Callback_SameAthlete_RefreshesTokens_AndPreservesActivities"`

**C24** - Connecting a different Strava account is blocked without automatic replacement (STR-07, AC 7)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StravaCallbackHandlerTests.Callback_DifferentAthlete_Blocked_WithoutReplacement"`

**C25** - A failed refresh marks `NeedsReauthorization` after one attempt and notifies the athlete (STR-08, AC 8)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StravaTokenServiceTests.RefreshFailsOnce_MarksNeedsReauthorization_AndNotifiesAthlete"`

**C26** - Tokens are ciphertext at rest and absent from browser responses and logs (STR-09, AC 9)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StravaTokenSecurityTests.Tokens_NotInBrowserResponsesOrLogs_AndEncryptedAtRest"`

**C27** - Disconnect revokes the token and offers retain-or-delete with the effect of each (STR-10, AC 10)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~DisconnectStravaHandlerTests.Disconnect_RevokesAtStrava_AndOffersRetainOrDelete"`

**C28** - Missing `activity:read_all` imports only public activities and raises the incomplete-history warning (STR-11, AC 11)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~InitialSyncRunnerTests.MissingReadAllScope_ImportsPublicOnly_AndReportsIncompleteHistory"`

**C29** - Every screen showing imported data displays "Powered by Strava" and a Strava link (STR-12, AC 12)
Proof: `dotnet test tests/SprintUp.E2E/SprintUp.E2E.csproj --no-build --filter "FullyQualifiedName~OnboardingFlowTests.ImportedDataScreens_ShowPoweredByStravaLink"`

**C30** - A second active Strava connection is rejected by the unique index (STR-13, AC 13)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StravaCallbackHandlerTests.SecondActiveConnection_BlockedByUniqueIndex"`

**C31** - Before authorization the athlete is told private activities are imported and only read scopes are requested (STR-14, AC 14)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ConnectStravaHandlerTests.Connect_DisclosesPrivateImport_AndRequestsNoWriteScope"`

**C32** - An `Active` connection enqueues exactly one job for the chosen window (SYN-01, AC 1)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StravaCallbackHandlerTests.Callback_EnqueuesExactlyOneJob_WithChosenWindow"`

**C33** - Running, trail, virtual, manual, non-running, and unknown types follow the mapping rules (SYN-02, AC 2)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~InitialSyncRunnerTests.EligibleManualUnknownAndNonRunningTypes_FollowMappingRules"`

**C34** - Pagination continues through a short page until an empty page (SYN-03, AC 3)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~InitialSyncRunnerTests.ShortPage_ContinuesUntilEmptyPage"`

**C35** - Each activity is persisted once per athlete by Strava activity ID (SYN-04, AC 4)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~InitialSyncRunnerTests.RepeatedSync_PersistsEachActivityOnce"`

**C36** - A repeated sync updates Strava fields and preserves SprintUp-owned fields (SYN-05, AC 5)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~InitialSyncRunnerTests.ExistingActivity_UpdatesStravaFields_AndPreservesSprintUpFields"`

**C37** - A `429` waits at least `Retry-After` and never exceeds five attempts (SYN-06, AC 6)
Proof: `dotnet test tests/SprintUp.UnitTests/SprintUp.UnitTests.csproj --no-build --filter "FullyQualifiedName~StravaRetryPolicyTests.RateLimit429_WaitsRetryAfter_AndCapsAtFiveAttempts"`

**C38** - A page failure preserves good pages, sets `PartiallyCompleted`, and retries only the failed page (SYN-07, AC 7)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~InitialSyncRunnerTests.PageFailure_PreservesPages_SetsPartial_AndRetriesFromNextPage"`

**C39** - A clean job records imported, skipped, and failed counts plus duration (SYN-08, AC 8)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~InitialSyncRunnerTests.Completed_RecordsImportedSkippedFailedAndDuration"`

**C40** - Zero eligible activities completes the job and shows an explanatory empty state (SYN-09, AC 9)
Proof: `dotnet test tests/SprintUp.E2E/SprintUp.E2E.csproj --no-build --filter "FullyQualifiedName~OnboardingFlowTests.ZeroActivities_CompletesJobAndShowsEmptyState"`

**C41** - Running shows status and count, and leaving the screen does not interrupt the job (SYN-10, AC 10)
Proof: `dotnet test tests/SprintUp.E2E/SprintUp.E2E.csproj --no-build --filter "FullyQualifiedName~OnboardingFlowTests.LeavingSyncScreen_JobContinues_AndCountsRefresh"`

**C42** - Disconnecting or revoking during a job interrupts it and preserves imported activities (SYN-11, AC 11)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~InitialSyncRunnerTests.DisconnectDuringRun_InterruptsJob_AndPreservesImportedActivities"`

**C43** - Strava `5xx` or timeout ends `Failed` after retries and a retry does not redo committed pages (SYN-12, AC 12)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~InitialSyncRunnerTests.Strava5xx_FailsAfterRetries_AndRetrySkipsCommittedPages"`

**C44** - A new sync while a job is active returns the existing job and creates no second job (SYN-13, AC 13)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StartInitialSyncHandlerTests.ActiveJob_ReturnsExisting_AndCreatesNoSecondJob"`

**C45** - Metrics are stored in canonical units and start is stored in UTC and local time (SYN-14, AC 14)
Proof: `dotnet test tests/SprintUp.UnitTests/SprintUp.UnitTests.csproj --no-build --filter "FullyQualifiedName~ActivityTests.CanonicalUnitsAndUtcLocalStart_AreStored"`

**C46** - An activity with missing optional metrics is persisted and shows the incomplete-data marker (SYN-15, AC 15)
Proof: `dotnet test tests/SprintUp.UiTests/SprintUp.UiTests.csproj --no-build --filter "FullyQualifiedName~ReviewPageTests.IncompleteMetrics_ShowsMarkerAndEmDash_WithoutDiscardingActivity"`

**C47** - The raw payload is retained without tokens and deleted with the activity (SYN-16, AC 16)
Proof: `dotnet test tests/SprintUp.UnitTests/SprintUp.UnitTests.csproj --no-build --filter "FullyQualifiedName~ActivityTests.RawPayload_RetainedWithoutTokens_AndDeletedWithActivity"`

**C48** - Logs carry job ID, counts, duration, and error code, and no tokens, names, coordinates, or health metrics (SYN-17, AC 17)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~SyncLoggingTests.CompletedJob_LogsJobIdCountsDurationAndErrorCode_NoPii"`

**C49** - Imported activities are visible only to the athlete and the assigned coach under consent (SYN-18, AC 18)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ResourceAccessGuardTests.ActivitiesVisibleOnlyToAthleteAndAssignedCoach"`

**C50** - A `Running` job without a heartbeat for 10 minutes is marked `Failed` and its lease released (SYN-19, AC 19)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~SyncJobSweeperTests.StaleHeartbeat_MarksFailed_AndReleasesLease"`

**C51** - Strava names and descriptions containing script are escaped when rendered (SYN-20, AC 20)
Proof: `dotnet test tests/SprintUp.UiTests/SprintUp.UiTests.csproj --no-build --filter "FullyQualifiedName~SyncProgressPageTests.StravaTextWithScript_IsEncodedAsText"`

**C52** - The job window end is fixed at creation, so later activities are excluded (SYN-21, AC 21)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StartInitialSyncHandlerTests.WindowEnd_FixedAtJobStart_ExcludesLaterActivities"`

**C53** - A job state change reaches the coach dashboard and sends no external notification (SYN-22, AC 22)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ListAssignedAthletesHandlerTests.JobStateChange_ReflectsInDashboard_WithoutExternalNotification"`

**C54** - The review shows count, date range, total distance, and total time (REV-01, AC 1)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ReviewHistoryHandlerTests.CompletedSync_ReturnsCountDateRangeDistanceAndTime"`

**C55** - The review lists the 20 most recent activities with date, type, distance, duration, and Strava link (REV-02, AC 2)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ReviewHistoryHandlerTests.RecentActivities_ReturnsTwentyWithRequiredFieldsAndStravaLinks"`

**C56** - "Sync again" enqueues the same window and attaches to progress (REV-03, AC 3)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StartInitialSyncHandlerTests.SyncAgain_EnqueuesSameWindow_AndReturnsProgress"`

**C57** - During an incomplete sync the coach sees "Sync in progress" and `Ready = false` (REV-04, AC 4)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ListAssignedAthletesHandlerTests.IncompleteSync_ShowsSyncInProgress_AndReadyFalse"`

**C58** - Imported metrics reject edits while the SprintUp note remains editable (REV-05, AC 5)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~UpdateActivityNoteHandlerTests.ImportedMetricsReadOnly_NoteEditable"`

**C59** - Hiding an activity removes it from default athlete and coach views without deleting data (REV-06, AC 6)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~HideActivityHandlerTests.Hide_RemovesFromDefaultAthleteAndCoachViews_KeepsData"`

**C60** - Disconnect offers retain or delete history with each effect explained (LIFE-01, AC 1)
Proof: `dotnet test tests/SprintUp.UiTests/SprintUp.UiTests.csproj --no-build --filter "FullyQualifiedName~LifecyclePageTests.Disconnect_OffersRetainOrDelete_WithExplanations"`

**C61** - Choosing delete history removes activities and raw payloads within 30 days and confirms (LIFE-02, AC 2)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~DataDeletionWorkerTests.DeleteHistory_RemovesActivitiesAndPayload_AndConfirms"`

**C62** - Explicit deletion ends access immediately, purges personal and health data within 30 days, and skips inactivity retention (LIFE-03, AC 3)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~RequestAccountDeletionHandlerTests.Delete_RevokesAccessImmediately_AndBypassesInactivity"`

**C63** - Deletion requested during a sync stops the job before purge (LIFE-04, AC 4)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~RequestAccountDeletionHandlerTests.DeleteDuringSync_InterruptsJobBeforePurge"`

**C64** - Abandoned onboarding gets a notice at day 23 and deletion at day 30 (LIFE-05, AC 5)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~SubscriptionLifecycleWorkerTests.AbandonedOnboarding_NotifiesAtDay23_DeletesAtDay30"`

**C65** - The first sync start begins a 30-day trial; expiry sets `Inactive`, notifies, and retains data for 30 days (LIFE-06, AC 6)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~SubscriptionLifecycleWorkerTests.TrialExpiry_SetsInactive_Notifies_AndStartsRetention"`

**C66** - During `Inactive` the coach has read-only access, and deletion revokes all coach access (LIFE-07, AC 7)
Proof: `dotnet test tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~ResourceAccessGuardTests.InactiveSubscription_CoachReadOnly_DeletionRevokesAll"`

**C67** - The full onboarding path is operable by keyboard with visible focus and zero axe violations (UX-01, AC 1)
Proof: `dotnet test tests/SprintUp.E2E/SprintUp.E2E.csproj --no-build --filter "FullyQualifiedName~OnboardingAccessibilityTests.KeyboardPathFocusAndLabels_HaveNoAxeViolations"`

**C68** - Every onboarding screen at 360 px and 1440 px has no horizontal scrolling (UX-02, AC 2)
Proof: `dotnet test tests/SprintUp.E2E/SprintUp.E2E.csproj --no-build --filter "FullyQualifiedName~OnboardingViewportTests.At360And1440_NoHorizontalScroll"`

**C69** - PT-BR is the default, EN is complete, and switching units re-displays without changing canonical values (UX-03, AC 3)
Proof: `dotnet test tests/SprintUp.E2E/SprintUp.E2E.csproj --no-build --filter "FullyQualifiedName~OnboardingFlowTests.SwitchPtBrEnAndUnits_KeepsCanonicalValues"`

---

## Handoff

The feature's live state, written as it happens - a compaction gives no warning.

**Settled mid-build:** the GitHub repository is public at `https://github.com/fabiorhein/sprintup`; `gh` is installed at `~/.local/bin/gh` because `sudo` requires an interactive password; the local container runtime is Podman (rootless) because Docker is not installed, with PostgreSQL 16 at `localhost:5432`, database `sprintup_test`, user and password `sprintup`, reached through `TEST_DB_CONNECTION`.
**Dispatched:** 2026-10-26 - Phase 1 batch (T2-T8) to a general sub-agent; base commit 1014f9f; returned: complete.
**Phase 1:** complete 2026-10-27 - T2-T8 implemented, gated, and committed (`631319e`, `05b61c6`, `18b0780`, `44533b8`, `adaa06c`, `4438c33`, `c47a11c`).
**Host probes:** the Testing-only diagnostics endpoints are mapped only when `ASPNETCORE_ENVIRONMENT=Testing` and `Diagnostics:Enabled=true`; they are never mapped in Development, Staging, or Production.
**Push policy:** per task - every task commit is pushed to `origin/main` after its gate is green, using SSH (`git@github.com:fabiorhein/sprintup.git`). T1-T8 are published; `origin/main` was at `9bc0582` after the Phase 1 review.
**Phase 1 verification:** independent review of T5-T8 plus `make build`, `make lint`, and `make harness` with `TEST_DB_CONNECTION` green; 12 integration tests, 0 unit, 0 UI, 0 E2E, 0 performance. Deviations reviewed and accepted: test-only diagnostics endpoints, combined Data Protection/500 test, placeholder `account_status` claim, and the no-op worker seam.
**Dispatched:** 2026-10-27 - Phase 2 batch (T9-T14, accounts domain) to a general sub-agent; base commit `9bc0582`; returned: pending.
**Walked:** none

---

## Phase Execution Map

```
Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6
Phase 7 → Phase 8 → Phase 9 → Phase 10 → Phase 11 → Phase 12

Phase 1:  T1 → T2 → T3 → T4 → T5 → T6 → T7 → T8
Phase 2:  T9 → T10 → T11 → T12 → T13 → T14
Phase 3:  T15 → T16 → T17 → T18 → T19
Phase 4:  T20 → T21 → T22 → T23 → T24 → T25 → T26 → T27
Phase 5:  T28 → T29 → T30 → T31 → T32 → T33
Phase 6:  T34 → T35 → T36 → T37 → T38
Phase 7:  T39 → T40 → T41 → T42 → T43 → T44
Phase 8:  T45 → T46 → T47 → T48 → T49 → T50
Phase 9:  T51 → T52 → T53 → T54 → T55 → T56
Phase 10: T57 → T58 → T59 → T60 → T61 → T62 → T63 → T64
Phase 11: T65 → T66 → T67 → T68 → T69 → T70 → T71 → T72
Phase 12: T73 → T74 → T75 → T76 → T77 → T78 → T79
```

Execution is strictly sequential - there is no intra-phase parallelism. A single agent (or batch worker) works one task at a time, in order.

---

## Task Granularity Check

| Task | Scope | Status |
| ---- | ----- | ------ |
| T1-T8 | Repository, host, database, tests, shared infrastructure | ✅ Bootstrap layer, one concern per task |
| T9-T14 | One aggregate, token, consent, migration, or outbox concern each | ✅ Granular |
| T15-T19 | One coach, assignment, or coach-account handler each | ✅ Granular |
| T20-T27 | One account handler or auth page each | ✅ Granular |
| T28-T33 | One profile rule, handler, or page each | ✅ Granular |
| T34-T38 | One invitation model, handler, or page each | ✅ Granular |
| T39-T44 | One contract, subscription, trial start, activity, job, or enqueue concern each | ✅ Granular |
| T45-T50 | One adapter, OAuth, token, disconnect, or page concern each | ✅ Granular |
| T51-T56 | One worker, runner, retry, heartbeat, status, or page concern each | ✅ Granular |
| T57-T64 | One review query, note, hide, list, overview, or page concern each | ✅ Granular |
| T65-T72 | One lifecycle worker, policy, deletion, notification, or page concern each | ✅ Granular |
| T73-T79 | One localization, unit, proof suite, or documentation concern each | ✅ Granular |

## Diagram-Definition Cross-Check

| Phase | Diagram | Definition | Status |
| ----- | ------- | ---------- | ------ |
| 1 | T1 → ... → T8 | Each task depends on its predecessor; no other intra-phase dependency | ✅ Match |
| 2 | T9 → ... → T14 | Each task depends on its predecessor | ✅ Match |
| 3 | T15 → ... → T19 | Each task depends on its predecessor | ✅ Match |
| 4 | T20 → ... → T27 | Each task depends on its predecessor | ✅ Match |
| 5 | T28 → ... → T33 | Each task depends on its predecessor | ✅ Match |
| 6 | T34 → ... → T38 | Each task depends on its predecessor | ✅ Match |
| 7 | T39 → ... → T44 | Each task depends on its predecessor | ✅ Match |
| 8 | T45 → ... → T50 | Each task depends on its predecessor | ✅ Match |
| 9 | T51 → ... → T56 | Each task depends on its predecessor | ✅ Match |
| 10 | T57 → ... → T64 | Each task depends on its predecessor | ✅ Match |
| 11 | T65 → ... → T72 | Each task depends on its predecessor | ✅ Match |
| 12 | T73 → ... → T79 | Each task depends on its predecessor | ✅ Match |

## Test Co-location Validation

| Task group | Code Layer Created/Modified | Matrix Requires | Task Says | Status |
| ---------- | --------------------------- | --------------- | --------- | ------ |
| T1-T3, T5, T79 | Repository/config/docs | none | none | ✅ OK |
| T9-T11, T28, T39-T40, T42, T45, T53, T74 | Domain/policy/formatter | unit | unit | ✅ OK |
| T4, T6-T8, T12-T24, T29-T31, T33-T37, T41, T43-T44, T46-T49, T51-T52, T54-T55, T57-T60, T62-T63, T65-T69 | Handler/endpoint/EF/worker | integration | integration | ✅ OK |
| T25-T27, T32, T38, T50, T56, T61, T64, T70-T73 | Blazor page/component | ui | ui | ✅ OK |
| T75-T77 | E2E, accessibility, viewport | e2e | e2e | ✅ OK |
| T78 | Performance | performance | performance | ✅ OK |
