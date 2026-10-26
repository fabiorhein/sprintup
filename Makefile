SHELL := /bin/bash
.DEFAULT_GOAL := build

DOTNET ?= dotnet
SOLUTION := SprintUp.sln
TEST_DB_CONNECTION ?=
PLAYWRIGHT ?= npx playwright

UNIT_PROJECT := tests/SprintUp.UnitTests/SprintUp.UnitTests.csproj
INTEGRATION_PROJECT := tests/SprintUp.IntegrationTests/SprintUp.IntegrationTests.csproj
UI_PROJECT := tests/SprintUp.UiTests/SprintUp.UiTests.csproj
E2E_PROJECT := tests/SprintUp.E2E/SprintUp.E2E.csproj
PERFORMANCE_PROJECT := tests/SprintUp.PerformanceTests/SprintUp.PerformanceTests.csproj

E2E_TESTS := $(shell find tests/SprintUp.E2E -name '*.cs' -not -path '*/obj/*' -not -path '*/bin/*' 2>/dev/null)
PERFORMANCE_TESTS := $(shell find tests/SprintUp.PerformanceTests -name '*.cs' -not -path '*/obj/*' -not -path '*/bin/*' 2>/dev/null)

export TEST_DB_CONNECTION

.PHONY: restore build lint test-unit test-integration test-ui test-e2e \
	harness-fast harness perf check-db check-playwright

restore:
	$(DOTNET) restore $(SOLUTION)

build: restore
	$(DOTNET) build $(SOLUTION) -warnaserror

lint:
	$(DOTNET) format $(SOLUTION) --verify-no-changes

test-unit: build
	$(DOTNET) test $(UNIT_PROJECT) --no-build --nologo

test-integration: build check-db
	$(DOTNET) test $(INTEGRATION_PROJECT) --no-build --nologo

test-ui: build
	$(DOTNET) test $(UI_PROJECT) --no-build --nologo

test-e2e:
	@if [ -z "$(strip $(E2E_TESTS))" ]; then \
		echo "No E2E tests yet; browser requirement not checked."; \
	else \
		$(MAKE) check-playwright; \
		$(DOTNET) test $(E2E_PROJECT) --nologo; \
	fi

harness-fast: restore build lint test-unit test-integration test-ui

harness: harness-fast test-e2e

perf:
	@if [ -z "$(strip $(PERFORMANCE_TESTS))" ]; then \
		echo "No performance tests yet."; \
	else \
		$(DOTNET) test $(PERFORMANCE_PROJECT) --nologo; \
	fi

check-db:
	@if [ -z "$(strip $(TEST_DB_CONNECTION))" ] && ! command -v docker >/dev/null 2>&1; then \
		echo "ERROR: integration tests need PostgreSQL."; \
		echo "Start Docker so Testcontainers can run, or set TEST_DB_CONNECTION to an empty disposable database:"; \
		echo "  export TEST_DB_CONNECTION='Host=localhost;Port=5432;Database=sprintup_test;Username=sprintup;Password=sprintup'"; \
		exit 1; \
	fi

check-playwright:
	@if ! command -v $(firstword $(PLAYWRIGHT)) >/dev/null 2>&1; then \
		echo "ERROR: Playwright CLI is not installed. Install Node.js 22 and run 'npx playwright install chromium'."; \
		exit 1; \
	fi
	@browsers="$${PLAYWRIGHT_BROWSERS_PATH:-}"; \
	if [ -z "$$browsers" ] || [ "$$browsers" = "0" ]; then browsers="$$HOME/.cache/ms-playwright"; fi; \
	if ! ls -d "$$browsers"/chromium-* >/dev/null 2>&1; then \
		echo "ERROR: Playwright browsers are not installed."; \
		echo "Run 'npx playwright install chromium' before the E2E gate."; \
		exit 1; \
	fi
