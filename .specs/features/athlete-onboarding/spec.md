# Athlete Onboarding & Initial Strava Sync Specification

**Status**: Approved. All 38 product decisions resolved on 2026-10-26.
**Approved**: 2026-10-26 - user: "colocar toda a documentação em inglês e seguir para aprovado dentro do contexto que estamos trabalhando"
**Date**: 2026-10-26
**Product**: SprintUp
**Feature**: Athlete onboarding and initial Strava workout sync

---

## Context & Constraints

- SprintUp is a data-driven running coaching SaaS.
- The solution will be a modular monolith in .NET 10 with Domain-Driven Design. This spec defines behavior; modules, contracts, and persistence belong to Design.
- The external integration is the Strava API via OAuth 2.0 and activity endpoints. Official documentation will be consulted during Design. This spec does not fix unverified endpoints, limits, or fields.
- AI is the primary coach for every athlete (Q-32). This feature prepares the data but does not generate plans, analysis, performed-vs-prescribed comparison, or AI responses; that belongs to a separate feature after AI/RAG consent (Q-26, Q-38).
- The human coach is optional, limited to one per athlete, and does not block onboarding completion (Q-28, Q-32).
- Business model: the 30-day trial starts at the first sync. Without a purchase, the subscription becomes `Inactive` and data is retained for 30 days; explicit deletion removes the data (Q-36, Q-37).
- No screenshots or visual prototypes were provided. Screens are specified by states and outcomes, not layout.
- Language: PT-BR is the default and EN is complete in onboarding (Q-35). Canonical metrics are metric, with a metric or imperial display preference (Q-05).

---

## Problem Statement

A data-driven running coaching service depends on reliable history to prescribe workouts, track load, and feed analysis. The athlete already has part of that history in Strava, but creating an account, filling in a profile, connecting Strava, and importing workouts is manual, slow, and easy to abandon. Without initial history, the coach starts blind and the AI/RAG engine starts empty.

This feature delivers the first complete value flow: account, running profile, Strava connection, and idempotent initial sync. The athlete goes from sign-up to available history without manual upload. The coach receives enough data for the first conversation. The platform gains a normalized base for the next features.

---

## Goals

- [ ] An athlete completes sign-up, profile, Strava connection, and the first sync in one session, with resume after interruption.
- [ ] At least 95% of eligible activities in the chosen window are imported on the first successful run.
- [ ] No duplicate activity after repeated syncs; every activity is traceable to its Strava ID.
- [ ] Strava failures do not block the account: the athlete resumes the sync without losing what was already imported.
- [ ] Imported history is normalized and queryable by the athlete and the coach, ready for future AI/RAG indexing.

These targets were confirmed in Q-25 and Q-29.

---

## Out of Scope

| Item | Reason |
| --- | --- |
| Continuous sync, webhooks, and daily automatic updates | Q-20 confirmed this spec covers only the initial snapshot. |
| Workout prescription, plans, automatic periodization, Agent prescription, and performed-vs-prescribed comparison | Core coaching work, outside onboarding (Q-06). |
| Embeddings, vector indexing, chat, summaries, analysis, and AI/RAG recommendations | AI is the primary coach (Q-32), but this feature covers only the data base. AI use in onboarding was ruled out in Q-26 and Q-38. |
| Human coach public sign-up, approval, and management | Q-33 confirmed this flow belongs to a separate feature, with a `PendingApproval` state. |
| Google and Apple sign-in | Q-34 confirmed email/password for the MVP; social providers are fast follow. |
| Checkout, billing, and subscription management implementation | The business rule is in LIFE-06 and LIFE-07; billing implementation is outside this feature (Q-36). |
| Strava upload or write, segments, kudos, comments, and social features | Not needed to import history (Q-13, Q-16). |
| Garmin, Polar, Apple Health, TrainingPeaks, and other providers | Strava only in the MVP. |
| Manual workout upload and editing imported metrics | Strava is the source of truth in this feature (Q-11). |
| Native mobile apps | Web first. |
| Password recovery and full account management | Separate authentication feature. |
| Athlete data export | Outside this feature; data deletion is covered by LIFE-02, LIFE-03, and LIFE-06. |

---

## Assumptions & Open Questions

All product decisions were confirmed on 2026-10-26. The rows below record the adopted default and the source decision.

| Assumption / decision | Chosen default | Rationale | Confirmed? |
| --- | --- | --- | --- |
| Athlete entry | Invitation as the primary path and public sign-up allowed | Q-01 confirmed both; invitation reduces friction and public sign-up supports direct acquisition. | y (Q-01) |
| Authentication | Separate SprintUp account from Strava; email/password with verification in the MVP; Google/Apple as fast follow | Q-03 and Q-34 confirmed; Strava is never identity. | y (Q-03, Q-34) |
| Email verification | Required before connecting Strava | Proves email ownership and reduces fake accounts before calling an external API. | y (Q-03) |
| Invitation | Bound to the invited email; a different email requires explicit confirmation and coach approval | Q-30 confirmed; prevents improper transfer of the link. | y (Q-30) |
| Required profile fields | Full name, date of birth, time zone, running experience, primary goal, and weekly availability; weight, height, sex/gender, and injury optional | Q-07 confirmed the list. These are the minimum for the coach to interpret history. | y (Q-07) |
| Sensitive data | Weight, height, sex/gender, injury history, resting heart rate, and medications are optional, with a specific purpose and consent | Q-08 confirmed all are optional in the MVP. | y (Q-08) |
| Consent | Terms and privacy at sign-up; health data in the profile; AI/RAG consent before any AI feature | Q-09 confirmed separate consents with specific purpose and revocation. | y (Q-09) |
| Minimum age | 18 or older in the MVP | Q-04 confirmed 18+; minors are out of scope. | y (Q-04) |
| Strava scopes | `read` and `activity:read_all`, read-only | Q-13 confirmed private activities; Q-16 confirms there is no write. | y (Q-13, Q-16) |
| History window | 12 months by default; 90 days as an option | Q-14 confirmed; balances coaching value, sync time, and API limits. | y (Q-14) |
| Activity types | Run, TrailRun, and VirtualRun; manual runs included and marked; unknown types keep the raw payload and stay out of running metrics | Q-15 and Q-31 confirmed. | y (Q-15, Q-31) |
| Data depth | Activity summary and raw payload; no streams or laps | Q-16 confirmed; delivers immediate value at lower storage cost. | y (Q-16) |
| Onboarding completion | May complete with a pending sync (`CompletedWithPendingSync`) | Q-23 confirmed; a Strava outage must not block entry. | y (Q-23) |
| Duplicate Strava account | Reject the link and direct to support; no automatic merge | Q-17 confirmed; prevents history takeover and duplicate athletes. | y (Q-17) |
| Strava account switch | Block; support replaces it after explicit athlete confirmation | Q-18 confirmed; protects history integrity. | y (Q-18) |
| Disconnection | Revoke the token; the athlete chooses to retain or delete history; deletion within 30 days | Q-19 confirmed; gives the athlete control and supports minimization. | y (Q-19) |
| Incomplete onboarding retention | 30 days after the last activity, with notice at least 7 days in advance | Q-10 confirmed 30 days and prior notice; 7 days is the notice default. | y (Q-10) |
| Concurrent sync | One active job per athlete; new requests return the existing job | Prevents duplicates, rate-limit waste, and inconsistent states. | y (spec default) |
| Performance | p95 up to 3 minutes for 12 months and up to 10,000 activities | Q-25 confirmed the target to guide Design and sync cost. | y (Q-25) |
| Primary coach | AI as the primary coach for every athlete; human coach optional and limited to one | Q-02, Q-28, and Q-32 confirmed; removes manual assignment as a dependency. | y (Q-02, Q-28, Q-32) |
| Workout definition | Activity is the performed workout; PlannedWorkout is the prescribed one; prescription and comparison are outside this feature | Q-06 confirmed the distinction. | y (Q-06) |
| Imported activities | Metrics read-only; the athlete may hide an activity and edit only SprintUp notes | Q-11 confirmed. | y (Q-11) |
| Business model | 30-day trial from the first sync; without purchase, subscription `Inactive` with 30-day retention; explicit deletion removes data | Q-36 confirmed. | y (Q-36) |
| Coach access across the lifecycle | Read-only during inactivity; no access after deletion | Q-37 confirmed. | y (Q-37) |
| Coach notification | Dashboard state only in the MVP, with no external notification | Q-24 confirmed. | y (Q-24) |
| Strava updates and deletions | Re-sync updates summaries; deletions belong to continuous sync | Q-21 confirmed. | y (Q-21) |
| AI/RAG in this feature | No embeddings or analysis; sync only persists normalized data | Q-26 and Q-38 confirmed; analysis and prescription belong to a separate feature after consent. | y (Q-26, Q-38) |
| Observability | Support sees job metadata, counts, and error codes; never health data | Q-27 confirmed; enables diagnosis without expanding exposure. | y (Q-27) |
| Language and display units | PT-BR default, complete EN in onboarding; canonical metric and metric or imperial display | Q-05 and Q-35 confirmed. | y (Q-05, Q-35) |

**Open questions:** none - all 38 product decisions resolved on 2026-10-26.

### Implicit-Requirement Dimensions

| Dimension | Resolution |
| --- | --- |
| Input validation & bounds | ONB-11 defines profile bounds; ONB-16 defines email-bound invitations; Q-07 and Q-08 decide fields and obligation. |
| Failure / partial-failure states | STR-08, SYN-07, SYN-12, and SYN-19 define failure, resume, and partial state. |
| Idempotency / retry / duplicate handling | SYN-04, SYN-05, SYN-13, and STR-05 define keys and safe repetition. |
| Auth boundaries & rate limits | ONB-09, STR-01 through STR-14, SYN-06, and SYN-18 define access and limits. |
| Concurrency / ordering | SYN-13 and SYN-19 prevent duplicate and orphaned jobs. |
| Data lifecycle / expiry | LIFE-01 through LIFE-07, Q-19, and Q-36 define retention, inactivity, deletion, and disconnection. |
| Observability | SYN-17 and Q-27 define what is logged and what support may see. |
| External-dependency failure | STR-08, SYN-06, and SYN-12 handle Strava unavailability, expiry, and rate limits. |
| State-transition integrity | The state model below and Q-22 define valid transitions. |
| AI/RAG data readiness | Generating embeddings is out of scope; imported data stays normalized and traceable. |
| Billing lifecycle | LIFE-06 and LIFE-07 define trial, inactivity, retention, and coach access. |

---

## Domain Language

| Term | Definition |
| --- | --- |
| Athlete | Person who receives running coaching and owns their workout data. |
| Coach | Professional who follows the athlete and has access to authorized data. May be human or AI (Q-02). |
| AI Coach (Primary Coach) | AI agent assigned to every athlete as the primary coach. It does not analyze or prescribe in this feature (Q-32, Q-38). |
| Human Coach | Optional human coach, limited to one per athlete. Public sign-up and approval belong to a separate feature (Q-28, Q-33). |
| CoachApplication | Human coach application with a `PendingApproval` state. Outside this feature (Q-33). |
| Coaching service / Team | Organization that groups coaches and athletes. The link model belongs to Design. |
| Activity (Performed Workout) | Run executed and imported from Strava. Not a prescribed workout. |
| PlannedWorkout (Prescribed Workout) | Workout planned by the coach. Outside this feature (Q-06). |
| StravaConnection | OAuth link between an athlete and a Strava account, with tokens and state. |
| InitialSync | One-time load of eligible Strava history into SprintUp. |
| SyncJob | Traceable sync run with state, counters, and errors. |
| Subscription | Commercial athlete state: `Trial`, `Active`, `Inactive`, or `Deleted`. |
| Eligible activity | Strava activity inside the chosen window and of an accepted running type. |

### State Model

| Entity | States | Rule |
| --- | --- | --- |
| Account | `PendingVerification` -> `Active` -> `Inactive` -> `Deleted` | Only an `Active` account may connect Strava. `Inactive` precedes deletion after retention; `Deleted` removes personal data. |
| Onboarding | `AccountPending` -> `ProfilePending` -> `StravaPending` -> `SyncPending` -> `Completed` / `CompletedWithPendingSync` | Every athlete gets AI as the primary coach; there is no waiting state for a human coach. |
| Subscription | `Trial` -> `Active` / `Inactive` -> `Deleted` | The trial starts at the first sync and lasts 30 days. Without purchase it becomes `Inactive` and retains data for 30 days. Explicit deletion moves it to `Deleted`. |
| StravaConnection | `Active`, `NeedsReauthorization`, `Revoked` | `Revoked` requires new authorization; it does not reactivate on its own. |
| SyncJob | `Queued` -> `Running` -> `Completed` / `PartiallyCompleted` / `Failed` / `Interrupted` | `PartiallyCompleted`, `Failed`, and `Interrupted` allow a new attempt. |
| CoachApplication | `PendingApproval` -> `Approved` / `Rejected` | Outside this feature; recorded so it is not confused with athlete onboarding (Q-33). |

---

## User Stories

### P1: Entry and account ⭐ MVP

**User Story**: As an athlete, I want to enter SprintUp through an invitation or public sign-up and create my account so I can start onboarding with the AI coach and, when present, the right human coach.

**Why P1**: Without the right account and link, there is no data owner and no controlled coach access.

**Acceptance Criteria**:
1. WHEN the athlete submits name, email, password, and acceptance of the terms THEN the system SHALL create an account in `PendingVerification` and SHALL send a verification email within 60 seconds. (**ONB-01**)
2. IF the email already belongs to an account THEN the system SHALL reject the sign-up, SHALL not create a second account, and SHALL show "This email is already registered. Sign in or recover your password." (**ONB-02**)
3. WHEN the athlete opens a valid verification link within 24 hours THEN the system SHALL activate the account and SHALL resume onboarding at the first incomplete step. (**ONB-03**)
4. IF the verification link is expired or already used THEN the system SHALL reject activation and SHALL offer a resend; a new resend SHALL invalidate previous links. (**ONB-04**)
5. WHEN the athlete accepts a consent, whether terms, privacy, or health data, THEN the system SHALL record user, consent type, document version, and timestamp. (**ONB-05**)
6. WHEN the athlete opens a valid invitation link THEN the system SHALL link the new account to the coach or team that sent the invitation and SHALL start onboarding. (**ONB-13**)
7. IF the invitation is expired, revoked, or already used THEN the system SHALL reject entry and SHALL offer public sign-up or support contact. (**ONB-14**)
8. WHEN the athlete signs up without an invitation THEN the system SHALL create the account with AI as the primary coach and SHALL allow onboarding to complete without a human coach assignment. (**ONB-15**)
9. WHEN the athlete opens an invitation issued to an email different from the account email THEN the system SHALL require explicit confirmation and SHALL keep the link pending until the inviting coach approves it. (**ONB-16**)
10. The system SHALL assign AI as the primary coach to every athlete at sign-up, SHALL allow at most one additional human coach, and SHALL not block onboarding completion when no human coach is present. (**ONB-17**)

**Independent Test**: Open a valid invitation, create an account, verify the email, and confirm the coach link. Repeat with an expired invitation, an invitation for another email, and public sign-up without a human coach.

---

### P1: Profile and resume ⭐ MVP

**User Story**: As an athlete, I want to provide my running context and resume where I stopped so the coach has the minimum needed to guide me.

**Why P1**: Strava history without goal, experience, and availability context does not support responsible prescription.

**Acceptance Criteria**:
1. The system SHALL require full name, date of birth, time zone, running experience, primary goal, and weekly availability before unlocking the Strava connection. (**ONB-06**)
2. WHERE a profile field is optional, such as weight, height, sex/gender, injury history, resting heart rate, or medications, the system SHALL allow skipping it without blocking onboarding. (**ONB-07**)
3. WHEN the athlete saves a step THEN the system SHALL persist the data and SHALL resume from the same point after logout, session expiry, or browser close. (**ONB-08**)
4. IF an athlete tries to access another athlete's onboarding or imported data THEN the system SHALL deny the operation and SHALL log the attempt. (**ONB-09**)
5. WHEN the athlete completes the required profile THEN the system SHALL advance to the Strava connection step. (**ONB-10**)
6. IF profile data is outside accepted bounds THEN the system SHALL reject the step with per-field errors and SHALL preserve entered values; initial bounds are a birth date in the past with a minimum age of 18, weight between 30 and 250 kg, height between 120 and 230 cm, and availability between 0 and 40 hours per week. (**ONB-11**)
7. WHEN the athlete completes the initial sync or chooses to continue without Strava THEN the system SHALL mark onboarding as `Completed` or `CompletedWithPendingSync`, respectively, without requiring a human coach assignment. (**ONB-12**)

**Independent Test**: Fill half the profile, leave, return, and confirm resume. Submit invalid weight and a birth date under 18 and confirm errors without losing the other fields.

---

### P1: Strava connection ⭐ MVP

**User Story**: As an athlete, I want to connect my Strava account so SprintUp imports my running history without manual upload.

**Why P1**: Strava is the data source that gives immediate value to onboarding.

**Acceptance Criteria**:
1. WHEN the athlete selects "Connect Strava" THEN the system SHALL redirect to Strava OAuth authorization with a unique `state` parameter and the `read` and `activity:read_all` scopes. (**STR-01**)
2. WHEN Strava returns a valid code and a `state` matching an active session THEN the system SHALL exchange the code on the server, SHALL store tokens encrypted, and SHALL mark the connection `Active`. (**STR-02**)
3. IF `state` is missing, invalid, expired, or already used THEN the system SHALL reject the callback, SHALL not store tokens, and SHALL show a retry message. (**STR-03**)
4. IF the athlete denies authorization at Strava THEN the system SHALL not create a connection and SHALL explain which data is needed and why. (**STR-04**)
5. IF the Strava athlete identifier is already linked to another SprintUp athlete THEN the system SHALL reject the link, SHALL not merge accounts automatically, and SHALL direct the athlete to support. (**STR-05**)
6. WHEN the athlete reconnects the same Strava account THEN the system SHALL refresh the tokens and SHALL preserve already imported activities. (**STR-06**)
7. IF the athlete tries to connect a Strava account different from the current one THEN the system SHALL block the switch and SHALL direct the athlete to support, without automatic replacement. (**STR-07**)
8. WHEN the access token expires or the API returns 401 THEN the system SHALL try to refresh the token once; IF the refresh fails THEN the system SHALL mark the connection `NeedsReauthorization` and SHALL notify the athlete. (**STR-08**)
9. The system SHALL encrypt Strava tokens at rest and SHALL never send them to the browser or write them to logs. (**STR-09**)
10. WHEN the athlete disconnects Strava THEN the system SHALL revoke the token at the Strava API, SHALL mark the connection `Revoked`, and SHALL offer the choice to retain or delete imported activities. (**STR-10**)
11. IF the granted scope does not include `activity:read_all` THEN the system SHALL import only public activities and SHALL show an incomplete-history warning. (**STR-11**)
12. The system SHALL display "Powered by Strava" attribution and a link to Strava on every screen that shows imported data. (**STR-12**)
13. The system SHALL allow at most one active Strava connection per athlete. (**STR-13**)
14. The system SHALL disclose, before authorization, that private activities will also be imported and SHALL request only the `read` and `activity:read_all` scopes, with no write permission. (**STR-14**)

**Independent Test**: Connect a test account, confirm stored tokens and `Active` state. Simulate denial, invalid `state`, an already linked account, and refusal of the private-activity scope.

---

### P1: Initial sync ⭐ MVP

**User Story**: As an athlete, I want SprintUp to import my running history automatically so my coach sees my workouts from day one.

**Why P1**: The first session only delivers value when history is available.

**Acceptance Criteria**:
1. WHEN the Strava connection becomes `Active` THEN the system SHALL enqueue exactly one initial sync job for the chosen window, with 12 months as the default and 90 days as an option. (**SYN-01**)
2. WHEN the initial sync job runs THEN the system SHALL fetch all eligible activities in the window, including running, trail running, and virtual running, SHALL include manual runs with an origin marker, and, IF the type is unrecognized, SHALL persist the raw payload without including it in running metrics. (**SYN-02**)
3. WHEN Strava returns an activity page THEN the system SHALL persist the page and SHALL continue pagination until there are no more results or the window start date is reached. (**SYN-03**)
4. The system SHALL persist each activity once per athlete, using the Strava activity identifier as the idempotency key. (**SYN-04**)
5. WHEN a repeated sync finds an existing activity THEN the system SHALL update summary fields with current Strava values and SHALL preserve SprintUp-owned fields. (**SYN-05**)
6. IF Strava returns 429 THEN the system SHALL wait at least the `Retry-After` and SHALL retry with exponential backoff, capped at 5 attempts per request. (**SYN-06**)
7. IF a page fails after retries THEN the system SHALL preserve already imported pages, SHALL mark the job `PartiallyCompleted`, and SHALL retry only the failed pages on a new attempt. (**SYN-07**)
8. WHEN the job finishes without failures and has processed all eligible activities THEN the system SHALL mark the job `Completed` and SHALL record the number of imported, skipped, and failed activities plus duration. (**SYN-08**)
9. IF no eligible activity is found THEN the system SHALL complete the job as `Completed` and SHALL show an explanatory empty state, not an error. (**SYN-09**)
10. WHILE the job is running THEN the system SHALL show status and imported-activity count and SHALL let the athlete leave the screen without interrupting the job. (**SYN-10**)
11. IF the athlete revokes Strava or disconnects during the job THEN the system SHALL stop the job, SHALL mark it `Interrupted`, and SHALL preserve already imported activities. (**SYN-11**)
12. IF Strava is unavailable, returns 5xx, or exceeds the timeout THEN the system SHALL retry under the retry policy and, after the final failure, SHALL mark the job `Failed` and SHALL allow a new attempt without redoing successful pages. (**SYN-12**)
13. WHEN the athlete requests a new sync while a job is active THEN the system SHALL return the existing job and SHALL not start a second job. (**SYN-13**)
14. The system SHALL store metrics in canonical units, such as meters, seconds, and meters per second, and SHALL store activity start in UTC and in the athlete's time zone. (**SYN-14**)
15. IF an activity lacks an optional metric, such as heart rate, power, distance, or elevation, THEN the system SHALL persist the activity and SHALL display it with an "incomplete data" marker, without discarding it. (**SYN-15**)
16. The system SHALL retain the raw Strava activity payload for audit and reprocessing, SHALL not store tokens in that payload, and SHALL apply the defined retention policy. (**SYN-16**)
17. The system SHALL log structured fields with job ID, counts, duration, and error code, and SHALL not log tokens, names, coordinates, or health metrics. (**SYN-17**)
18. The system SHALL limit visibility of imported activities to the athlete and the assigned coach, according to recorded consent. (**SYN-18**)
19. IF a job remains `Running` without a heartbeat for 10 minutes THEN the system SHALL mark it `Failed` and SHALL allow a new attempt. (**SYN-19**)
20. The system SHALL treat all text received from Strava as untrusted and SHALL escape it before rendering. (**SYN-20**)
21. The system SHALL limit the initial sync to activities that exist at job start and SHALL leave later activities to continuous sync. (**SYN-21**)
22. WHEN the sync job changes state THEN the system SHALL reflect the new state in the coach dashboard and SHALL not send an external notification in the MVP. (**SYN-22**)

**Independent Test**: Connect a test account with known activities, confirm counts, pagination, and no duplicates after repetition. Simulate 429, 5xx, partial failure, an interrupted worker, an unknown type, and a workout created after job start.

---

### P2: Imported history review

**User Story**: As an athlete, I want to review what was imported so I can confirm my history is complete before working with the coach.

**Why P2**: Review increases confidence and reduces support, but the main flow can complete without it.

**Acceptance Criteria**:
1. WHEN the initial sync ends THEN the system SHALL show activity count, covered date range, total distance, and total time. (**REV-01**)
2. WHEN the athlete opens the review THEN the system SHALL list the 20 most recent activities with date, type, distance, duration, and a link to the Strava activity. (**REV-02**)
3. WHEN the athlete selects "Sync again" THEN the system SHALL enqueue a job for the same window and SHALL show progress. (**REV-03**)
4. WHILE a sync is incomplete, the system SHALL show the assigned coach the status "Sync in progress" and SHALL not mark the athlete ready. (**REV-04**)
5. The system SHALL keep imported metrics read-only and SHALL allow editing only SprintUp-owned notes. (**REV-05**)
6. WHEN the athlete hides an activity THEN the system SHALL remove it from the default athlete and coach views without deleting the underlying data. (**REV-06**)

**Independent Test**: Open the review after a sync with 3 activities, check totals and links. Repeat with zero activities and with a partial sync. Confirm imported metrics are not editable, notes are, and hiding removes the activity from default views.

---

### P2: Lifecycle and disconnection

**User Story**: As an athlete, I want to control my Strava connection, my subscription, and my imported data so I can revoke access, become inactive, or delete my history.

**Why P2**: Necessary for trust and compliance, but it does not block the first sync.

**Acceptance Criteria**:
1. WHEN the athlete disconnects Strava THEN the system SHALL revoke the token and SHALL offer "retain history" or "delete history", with the effect of each option explained. (**LIFE-01**)
2. WHEN the athlete chooses to delete history THEN the system SHALL delete activities and raw payloads within 30 days and SHALL show confirmation. (**LIFE-02**)
3. WHEN the athlete requests explicit account deletion THEN the system SHALL immediately end athlete and coach access, SHALL delete personal and health data within 30 days, except what law requires retaining, and SHALL not apply the inactivity retention period. (**LIFE-03**)
4. IF deletion is requested during a sync THEN the system SHALL stop the job before completing deletion. (**LIFE-04**)
5. IF the athlete is inactive in onboarding for 30 days THEN the system SHALL send a notice at least 7 days in advance and SHALL delete incomplete onboarding data after the deadline if there is no resumption. (**LIFE-05**)
6. WHEN the first sync starts THEN the system SHALL start the 30-day trial; IF the trial ends without a purchase THEN the system SHALL mark the subscription `Inactive`, SHALL notify the athlete, and SHALL retain data for 30 days before automatic deletion. (**LIFE-06**)
7. WHILE the subscription is `Inactive` and within the retention period, the system SHALL give the human coach read-only access to history; WHEN the account is deleted, explicitly or after retention, THEN the system SHALL revoke all coach access. (**LIFE-07**)

**Independent Test**: Disconnect, choose delete, and confirm removal. Request account deletion during a simulated sync. Simulate trial expiry without purchase and confirm inactivity, retention, read-only coach access, and revocation after deletion.

---

### P1: Experience and accessibility ⭐ MVP

**User Story**: As an athlete, I want to use onboarding on any device, in my language, and with a screen reader so I can complete the flow without barriers.

**Why P1**: The flow is the product's entry point. An accessibility, language, or viewport barrier excludes athletes.

**Acceptance Criteria**:
1. The system SHALL meet WCAG 2.1 AA on onboarding screens, including keyboard navigation, visible focus, and screen-reader labels. (**UX-01**)
2. The system SHALL work at 360 px and 1440 px widths without horizontal scrolling. (**UX-02**)
3. The system SHALL use PT-BR as the default language, SHALL offer complete EN in onboarding, and SHALL allow metric or imperial display while keeping canonical storage. (**UX-03**)

**Independent Test**: Navigate the entire onboarding with keyboard and screen reader only. Open at 360 px and 1440 px and confirm no horizontal scrolling. Switch PT-BR/EN and metric/imperial and confirm canonical values do not change.

---

## Screen Outcomes

No captures were provided. Screens are defined by states and outcomes, not element placement.

| Screen | Required states | Requirements |
| --- | --- | --- |
| Entry / invitation | Valid, expired, revoked, used invitation; invitation for another email; public sign-up; loading; network error | ONB-13, ONB-14, ONB-15, ONB-16 |
| Sign-up and verification | Form, email already registered, email sent, invalid link, expired link, resend, verified account | ONB-01 through ONB-05 |
| Profile | Required step, optional fields, per-field error, under 18, saving, saved, resume, consent | ONB-06 through ONB-12 |
| Strava connection | Disconnected, redirecting, authorization denied, invalid `state`, connected, `NeedsReauthorization`, already linked account, different account, private scope denied | STR-01 through STR-14 |
| Sync | `Queued`, `Running`, `Completed`, `PartiallyCompleted`, `Failed`, `Interrupted`, zero activities, unknown type | SYN-01 through SYN-22 |
| Review | Loading, populated, empty, incomplete metrics, list with more than 20 activities, hidden activity, read-only metrics | REV-01 through REV-06 |
| Lifecycle | Active trial, expired trial, `Inactive`, retention notice, explicit deletion, read-only coach access | LIFE-01 through LIFE-07 |
| Errors | Actionable message, retry button, job reference for support, no tokens or internal details | SYN-12, SYN-17, UX-01 |

---

## Edge Cases

| Scenario | Expected behavior | Requirement / Question |
| --- | --- | --- |
| Strava account with no eligible runs | Complete with an explanatory empty state, not an error | SYN-09 |
| Athlete refuses the `activity:read_all` scope | Import only public activities and warn that history is incomplete | STR-11, STR-14 |
| Token revoked by the athlete at Strava before sync | Detect 401, attempt refresh, and mark `NeedsReauthorization` | STR-08 |
| Rate limit 429 | Wait for `Retry-After`, exponential backoff, maximum 5 attempts | SYN-06 |
| Strava unavailability, 5xx, or timeout | Retry; after final failure, `Failed` with resume without redoing good pages | SYN-12 |
| Failure on a specific page | `PartiallyCompleted`; preserve good pages; retry only failures | SYN-07 |
| Worker crashes and job stays `Running` | Mark `Failed` after 10 minutes without heartbeat and allow retry | SYN-19 |
| Two tabs request a sync | Return the existing job, with no second job | SYN-13 |
| Activity without heart rate, power, distance, or elevation | Persist and show "incomplete data" | SYN-15 |
| Manual run in Strava | Include with a manual-origin marker | SYN-02, Q-15 |
| Activity from another sport, such as cycling or swimming | Exclude from running history | SYN-02 |
| Unrecognized Strava activity type | Persist the raw payload and exclude it from running metrics | SYN-02, Q-31 |
| Activity updated in Strava | Update the summary with current Strava values | SYN-05 |
| Activity deleted in Strava after import | Do not remove it in this feature; handle it in continuous sync | Q-21 |
| New workout recorded after job start | Do not include it in the initial snapshot; handle it in continuous sync | SYN-21, Q-20 |
| Athlete revokes Strava during sync | Stop the job, mark `Interrupted`, preserve imported data | SYN-11 |
| Account deletion during sync | Stop the job before completing deletion | LIFE-04 |
| Strava account already linked to another athlete | Reject, do not merge, direct to support | STR-05, Q-17 |
| Reconnection with another Strava account | Block the automatic switch and direct to support | STR-07, Q-18 |
| Invitation expired, revoked, or already used | Reject entry and offer public sign-up or support | ONB-14 |
| Invitation issued to another email | Require explicit confirmation and approval by the inviting coach | ONB-16, Q-30 |
| Athlete under 18 | Block profile progress with a clear message and create no health data | ONB-11, Q-04 |
| Activity crosses a daylight-saving change | Store UTC and the local time zone; display in the athlete's time zone | SYN-14 |
| Activity name or description contains HTML or script | Escape all Strava text before rendering | SYN-20 |
| Athlete changes unit preference after import | Re-display from the canonical value; do not recreate the activity | SYN-14, UX-03 |
| Athlete tries to edit an imported metric | Reject the edit and keep the metric read-only | REV-05 |
| Athlete edits a SprintUp note | Store it separately from Strava fields | REV-05 |
| Athlete hides an activity | Remove it from default athlete and coach views without deleting the data | REV-06 |
| Athlete switches language or units | Re-display canonical values in the new format without changing storage | UX-03 |
| Coach opens the dashboard before sync ends | Show "Sync in progress", do not mark the athlete ready | REV-04, SYN-22 |
| Athlete signs up without an invitation | Create the account with AI as the primary coach, with no human coach required | ONB-15, ONB-17 |
| Onboarding abandoned midway | Retain for 30 days, notify 7 days in advance, then delete | LIFE-05 |
| Trial ends without purchase | Subscription `Inactive`, athlete notified, data retained for 30 days | LIFE-06 |
| Human coach opens the dashboard during inactivity | Show history in read-only mode | LIFE-07 |
| Human coach tries to access after deletion | Deny all access and log the attempt | LIFE-03, LIFE-07 |
| Repeated OAuth callback | Reject an already used `state` and store no token | STR-03 |
| Network drop during callback | Do not create a partial connection; allow a new attempt | STR-02, STR-03 |
| No human coach assigned | AI remains the primary coach; onboarding does not block | ONB-17 |

---

## Requirement Traceability

| Requirement ID | Story | Phase | Status |
| --- | --- | --- | --- |
| ONB-01 | P1: Entry | - | Pending |
| ONB-02 | P1: Entry | - | Pending |
| ONB-03 | P1: Entry | - | Pending |
| ONB-04 | P1: Entry | - | Pending |
| ONB-05 | P1: Entry | - | Pending |
| ONB-06 | P1: Profile | - | Pending |
| ONB-07 | P1: Profile | - | Pending |
| ONB-08 | P1: Profile | - | Pending |
| ONB-09 | P1: Profile | - | Pending |
| ONB-10 | P1: Profile | - | Pending |
| ONB-11 | P1: Profile | - | Pending |
| ONB-12 | P1: Profile | - | Pending |
| ONB-13 | P1: Entry | - | Pending |
| ONB-14 | P1: Entry | - | Pending |
| ONB-15 | P1: Entry | - | Pending |
| ONB-16 | P1: Entry | - | Pending |
| ONB-17 | P1: Entry | - | Pending |
| STR-01 | P1: Strava | - | Pending |
| STR-02 | P1: Strava | - | Pending |
| STR-03 | P1: Strava | - | Pending |
| STR-04 | P1: Strava | - | Pending |
| STR-05 | P1: Strava | - | Pending |
| STR-06 | P1: Strava | - | Pending |
| STR-07 | P1: Strava | - | Pending |
| STR-08 | P1: Strava | - | Pending |
| STR-09 | P1: Strava | - | Pending |
| STR-10 | P1: Strava | - | Pending |
| STR-11 | P1: Strava | - | Pending |
| STR-12 | P1: Strava | - | Pending |
| STR-13 | P1: Strava | - | Pending |
| STR-14 | P1: Strava | - | Pending |
| SYN-01 | P1: Sync | - | Pending |
| SYN-02 | P1: Sync | - | Pending |
| SYN-03 | P1: Sync | - | Pending |
| SYN-04 | P1: Sync | - | Pending |
| SYN-05 | P1: Sync | - | Pending |
| SYN-06 | P1: Sync | - | Pending |
| SYN-07 | P1: Sync | - | Pending |
| SYN-08 | P1: Sync | - | Pending |
| SYN-09 | P1: Sync | - | Pending |
| SYN-10 | P1: Sync | - | Pending |
| SYN-11 | P1: Sync | - | Pending |
| SYN-12 | P1: Sync | - | Pending |
| SYN-13 | P1: Sync | - | Pending |
| SYN-14 | P1: Sync | - | Pending |
| SYN-15 | P1: Sync | - | Pending |
| SYN-16 | P1: Sync | - | Pending |
| SYN-17 | P1: Sync | - | Pending |
| SYN-18 | P1: Sync | - | Pending |
| SYN-19 | P1: Sync | - | Pending |
| SYN-20 | P1: Sync | - | Pending |
| SYN-21 | P1: Sync | - | Pending |
| SYN-22 | P1: Sync | - | Pending |
| REV-01 | P2: Review | - | Pending |
| REV-02 | P2: Review | - | Pending |
| REV-03 | P2: Review | - | Pending |
| REV-04 | P2: Review | - | Pending |
| REV-05 | P2: Review | - | Pending |
| REV-06 | P2: Review | - | Pending |
| LIFE-01 | P2: Lifecycle | - | Pending |
| LIFE-02 | P2: Lifecycle | - | Pending |
| LIFE-03 | P2: Lifecycle | - | Pending |
| LIFE-04 | P2: Lifecycle | - | Pending |
| LIFE-05 | P2: Lifecycle | - | Pending |
| LIFE-06 | P2: Lifecycle | - | Pending |
| LIFE-07 | P2: Lifecycle | - | Pending |
| UX-01 | P1: UX | - | Pending |
| UX-02 | P1: UX | - | Pending |
| UX-03 | P1: UX | - | Pending |

**Coverage:** 69 total, 0 mapped to tasks, 69 unmapped. Tasks have not been created.

---

## Decisions

All 38 product decisions were answered by the user on 2026-10-26. No question remains open.

| ID | Decision |
| --- | --- |
| Q-01 | Entry by invitation and public sign-up, with invitation as the primary path. |
| Q-02 | AI as the primary coach; public human coach sign-up with administrator or moderator approval. Details in Q-32 and Q-33. |
| Q-03 | Email/password with verification in the MVP; Google/Apple as fast follow; Strava is never identity but is required to link workouts. |
| Q-04 | 18+ in the MVP. |
| Q-05 | PT-BR and EN; canonical metric and a metric or imperial display preference. |
| Q-06 | Activity is the performed workout; PlannedWorkout is the prescribed one. Agent prescription and performed-vs-prescribed comparison are outside this feature. |
| Q-07 | Required: name, birth date, time zone, experience, goal, and availability. Optional: weight, height, sex/gender, and injury. |
| Q-08 | Weight, height, sex/gender, injury, resting heart rate, and medications are optional, with a specific purpose and consent. |
| Q-09 | Separate consents: terms/privacy at sign-up, health in the profile, and AI/RAG before any AI feature. |
| Q-10 | 30 days after the last activity, with notice before deletion. |
| Q-11 | Imported metrics are read-only; the athlete may hide the activity and edit only SprintUp notes. |
| Q-12 | 30-day free trial, then inactivity without purchase; AI workouts are paid; coach has read-only access. |
| Q-13 | Include private activities with the `read` and `activity:read_all` scopes. |
| Q-14 | 12 months by default; 90 days as an option. |
| Q-15 | Run, TrailRun, and VirtualRun; manual runs included and marked. |
| Q-16 | Activity summary and raw payload; no streams or laps. |
| Q-17 | Strava already linked to another athlete: reject, no merge, direct to support. |
| Q-18 | Block Strava account switching; support replaces it after explicit confirmation. |
| Q-19 | Disconnecting revokes the token; the athlete chooses to retain or delete history. |
| Q-20 | Initial snapshot; continuous sync belongs to a separate feature. |
| Q-21 | Re-sync updates summaries; deletions belong to continuous sync. |
| Q-22 | `Completed` only when all eligible activities are processed; partial success requires a new attempt. |
| Q-23 | Allow onboarding completion with `CompletedWithPendingSync`. |
| Q-24 | Dashboard state only in the MVP, with no external coach notification. |
| Q-25 | p95 up to 3 minutes for 12 months and up to 10,000 activities. |
| Q-26 | No embeddings or indexing in this feature; separate feature. |
| Q-27 | Support sees job metadata, counts, and error codes, without health data. |
| Q-28 | One primary human coach in the MVP, in addition to AI. |
| Q-29 | 80% completion within 10 minutes, 95% import rate, and zero duplicates. |
| Q-30 | Invitation bound to the email; a different email requires explicit confirmation and coach approval. |
| Q-31 | Unknown type keeps the raw payload and stays out of running metrics. |
| Q-32 | AI default for every athlete; human coach optional; analysis and prescription in a separate feature. |
| Q-33 | Public human coach sign-up outside this feature, with a `PendingApproval` state. |
| Q-34 | Email/password with verification in the MVP; Google/Apple as fast follow. |
| Q-35 | PT-BR default; complete EN in onboarding. |
| Q-36 | Trial starts at the first sync; inactivity retains data for 30 days; explicit deletion removes data. |
| Q-37 | Coach has read-only access during inactivity; no access after deletion. |
| Q-38 | Sync only persists data; analysis and prescription belong to a separate feature after consent. |

---

## Success Criteria

- [ ] 80% of athletes who start onboarding complete account, profile, and Strava connection within 10 minutes.
- [ ] 95% of eligible activities in the chosen window are imported on the first successful run.
- [ ] Zero duplicate activities after three consecutive syncs of the same window.
- [ ] p95 up to 3 minutes to import 12 months of history for an athlete with up to 10,000 activities.
- [ ] Zero Strava tokens in browser responses, logs, or activity payloads.
- [ ] Zero edits to imported metrics; SprintUp notes are stored separately.
- [ ] 100% of hidden activities removed from default views without deleting the underlying data.
- [ ] 100% of consents recorded with type, document version, and timestamp.
- [ ] 100% of athletes receive AI as the primary coach without onboarding blocked by a missing human coach.
- [ ] 100% of subscriptions without a purchase become `Inactive` at trial end and retain data for 30 days.
- [ ] Zero human coach access after explicit account deletion.
- [ ] Fewer than 2% of syncs end in a definitive failure with no resume path.
- [ ] 100% of onboarding screens pass WCAG 2.1 AA verification and work at 360 px and 1440 px widths.

---

## Sources

- Product briefing provided by the user on 2026-10-26: running coaching SaaS, modular monolith in .NET 10, DDD, Strava API, and AI/RAG capabilities.
- Clarification answers provided by the user on 2026-10-26: Q-01 through Q-38.
- No screenshot, prototype, or external document was provided. There are no visual sources.
- Official Strava API documentation will be consulted during Design. This spec does not fix unverified endpoints, fields, or limits.
