# STATE

## Decisions

### AD-001
- **Decision**: Feature artifacts (`spec.md`, `design.md`, `tasks.md`) are written in English; chat and user-facing product copy remain in PT-BR unless a feature says otherwise.
- **Reason**: English matches the EARS keywords, the codebase, and most technical vocabulary; one language across artifacts avoids translation drift and uses fewer tokens per artifact.
- **Trade-off**: PT-BR stakeholders review English artifacts, so nuance can be slower to validate; the chat stays in PT-BR to keep decisions accessible.
- **Scope**: All features under `.specs/`.
- **Date**: 2026-10-26
- **Status**: active

### AD-002
- **Decision**: The project is a .NET 10 single-project Blazor modular monolith with PostgreSQL 16, EF Core 10, Minimal APIs under `/api/v1`, and database-backed background workers.
- **Reason**: One deploy, one runtime, and one test stack fit a solo product and the reference project's proven shape; PostgreSQL provides `jsonb`, partial indexes, and `SKIP LOCKED` without an external broker.
- **Trade-off**: A single process couples UI and workers at deploy time; scaling requires care with the workers and the in-process Strava rate gate.
- **Scope**: All features.
- **Date**: 2026-10-26
- **Status**: active

### AD-003
- **Decision**: Authentication uses a custom `User` aggregate, `PasswordHasher<User>`, and an HTTP-only cookie. ASP.NET Core Identity EF stores are not used. Roles are `Athlete`, `Coach`, and `Admin`.
- **Reason**: The approved account and onboarding state model is the source of truth; Identity's parallel tables and lifecycle would compete with it.
- **Trade-off**: Password reset, lockout, and external login are not provided by Identity and must be built or added later.
- **Scope**: All authenticated features.
- **Date**: 2026-10-26
- **Status**: active

### AD-004
- **Decision**: Activity providers sit behind `IActivityProvider`. Strava tokens are encrypted with ASP.NET Core Data Protection, and the rotating refresh token is persisted under a row lock.
- **Reason**: The boundary allows future providers without touching training code; rotation makes lost refresh updates fatal to the connection.
- **Trade-off**: Data Protection key loss makes stored tokens unreadable, so the key ring needs a persistent volume and backup.
- **Scope**: All integrations and ingestion.
- **Date**: 2026-10-26
- **Status**: active

### AD-005
- **Decision**: The MVP models coach-athlete only. AI is the seeded primary coach, at most one human coach is allowed, and minimal coach access (login, read-only dashboard, invitation approval) is part of onboarding. No `Team` entity exists.
- **Reason**: The spec requires coach visibility and invitation approval, while Q-33 keeps public coach registration out of scope.
- **Trade-off**: The operator bootstraps coaches, and team-level grouping is deferred.
- **Scope**: Coaching and onboarding features.
- **Date**: 2026-10-26
- **Status**: active

### AD-006
- **Decision**: PT-BR is the default language and English is complete for onboarding; Spanish is deferred. Canonical storage is metric, with a metric or imperial display preference.
- **Reason**: Q-05 and Q-35; canonical values keep analytics and AI inputs consistent.
- **Trade-off**: Spanish-speaking athletes are not served in the MVP.
- **Scope**: All user-facing features.
- **Date**: 2026-10-26
- **Status**: active

## Handoff
