# SprintUp

SprintUp is a data-driven running coaching SaaS. The AI coach is the primary coach; a human coach is optional and limited to one per athlete. The product is designed for adults (18+) and defaults to Brazilian Portuguese, with complete English onboarding.

**Repository**: https://github.com/fabiorhein/sprintup

## Status

Specification, design, and task plan are approved. The repository is in the bootstrap phase; application code starts at task T2 of the execution plan. The current milestone is athlete onboarding and the initial Strava sync.

## Stack

- .NET 10, ASP.NET Core, Blazor Web App with InteractiveServer
- PostgreSQL 16 with EF Core 10 and Npgsql
- Strava API for read-only activity import
- Modular monolith with one deployable host

## Repository layout

| Path | Purpose |
| --- | --- |
| `.specs/` | Approved specification, design, task plan, and project decisions |
| `src/` | Application host and modules (created in T2) |
| `tests/` | Unit, integration, UI, end-to-end, and performance tests (created in T4) |
| `docs/` | Operations runbook (created in T79) |

## Documentation

- [Feature specification](.specs/features/athlete-onboarding/spec.md)
- [Design](.specs/features/athlete-onboarding/design.md)
- [Task plan](.specs/features/athlete-onboarding/tasks.md)
- [Project decisions](.specs/STATE.md)

## Prerequisites

- .NET SDK `10.0.113`
- PostgreSQL 16, locally or through Docker Compose (added in T3)
- Node.js 22 and Playwright browsers for end-to-end tests (added in T4)
- `make` for the quality gates (added in T5)

When Docker is unavailable, integration tests use `TEST_DB_CONNECTION` against a disposable local PostgreSQL instance.

## Local development

`docker-compose.yml` is the supported local path. It starts PostgreSQL 16 with a named volume and a `pg_isready` healthcheck:

```bash
docker compose up -d postgres
docker compose ps
```

Validate the compose file with `docker compose config` on a machine with Docker Compose installed. This bootstrap environment does not have Docker, so the YAML was checked with a parser instead.

This environment uses rootless Podman as the local fallback. Start or reuse a PostgreSQL 16 container, then export `TEST_DB_CONNECTION` before running integration tests:

```bash
export TEST_DB_CONNECTION='Host=localhost;Port=5432;Database=sprintup_test;Username=sprintup;Password=sprintup'
```

The test fixture creates and drops its own per-run schema. It refuses to run against a non-empty database, so keep the `public` schema empty.

Copy `.env.example` to `.env` and fill only what you need. The documented keys are `ConnectionStrings__SprintUp`, `Strava__ClientId`, `Strava__ClientSecret`, `Strava__RedirectUri`, `DataProtection__KeysPath`, `Admin__BootstrapEmail`, `Admin__BootstrapPassword`, and the `TEST_DB_CONNECTION` fallback. Do not commit `.env`.

## Quality gates

The `Makefile` targets are introduced in T5:

```bash
make build
make lint
make harness
```

## Security

Do not commit secrets. Local configuration uses environment variables or user secrets; Strava tokens are encrypted at rest with ASP.NET Core Data Protection.

## License

No license has been selected yet. The repository is public for transparency; reuse rights are not granted until a license is added.

## Strava

Strava is a trademark of Strava, Inc. SprintUp is not affiliated with or endorsed by Strava.
