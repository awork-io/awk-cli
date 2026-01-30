# Plan

## Goal
Generic awork CLI, close to API surface, generated from `swagger.json`. Token-only auth via `.env` bearer token. Output always includes `statusCode`, `traceId`, `response`. Commands grouped by API areas; path params as positional args; body/query as options. Tests via bash scripts.

## Steps
1) Swagger + backend alignment: confirm auth headers, trace id headers, required fields, and any path params missing in swagger.
2) Codegen: source generator emits DTOs + client + CLI commands from swagger (no `Async` suffix). Keep manual code minimal.
3) CLI UX: Spectre.Console.Cli, tag branches, positional path args, validated options for query/body, `--body`/`--set`/`--set-json` escape hatch.
4) HTTP + output: bearer token only, trace id extraction, response envelope JSON for agents.
5) Docs + scripts: README, `.env` usage, bash scripts for build + sample flows.
6) Verify: `scripts/test-build.sh` + flow scripts (no full test suite).

## Done When
- `dotnet build` clean.
- `scripts/test-*.sh` green using `.env` token.
- CLI covers all swagger operations with ergonomic commands and validation.

## TODO
- Review swagger + awork-backend for endpoints: user import (skip invite), create private tasks from provided template, working hours/profile, holiday region, team assignment.
- Add sample scripts for the initial use case (generic commands, no onboarding defaults).
- Update README examples to show positional path args + body options.
- Confirm trace-id header precedence and document it.
