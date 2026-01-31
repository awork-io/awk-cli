# awork CLI (awk)

Token-only, swagger-driven awork CLI. Built for agents: stable command names, strict param validation, JSON output envelope.

## Quick start
```
echo "AWORK_TOKEN=..." > .env
dotnet run --project src/Awk.Cli -- doctor
```

## Configuration
Tokens (required):
- `AWORK_TOKEN=...` or `BEARER_TOKEN=...`

Base URL (optional):
- `AWORK_BASE_URL=https://api.awork.com/api/v1`

Overrides:
- `--env <PATH>` to load a different env file
- `--base-url <URL>` to override base URL

## Command structure
- Commands are grouped by Swagger tags (e.g., `users`, `tasks`, `projects`).
- Path params are positional, in path order.
- Options are kebab-case.
- Naming uses verbs (`list`, `get`, `create`, `update`, `delete`) + action verbs (`set-`, `add-`, `remove-`, `assign-`).

List commands:
```
dotnet run --project src/Awk.Cli -- --help
dotnet run --project src/Awk.Cli -- users --help
```

## Examples
List users:
```
dotnet run --project src/Awk.Cli -- users list
```

Get user by id (positional path param):
```
dotnet run --project src/Awk.Cli -- users get <user-id>
```

Search with query params (bool/int):
```
dotnet run --project src/Awk.Cli -- search get-search \
  --search-term "agent" \
  --search-types "user" \
  --top 3 \
  --include-closed-and-stuck true
```

Create a private task (params):
```
dotnet run --project src/Awk.Cli -- tasks create \
  --name "Welcome" \
  --base-type private \
  --entity-id <user-id>
```

Create a task from JSON:
```
dotnet run --project src/Awk.Cli -- tasks create --body @samples/private-task.json
```

Merge JSON body + overrides:
```
dotnet run --project src/Awk.Cli -- tasks create \
  --body @payload.json \
  --set name="Override"
```

Array body via `--set-json`:
```
dotnet run --project src/Awk.Cli -- absence-regions users-assign \
  --set regionId=<region-id> \
  --set-json userIds='["u1","u2"]'
```

Array body via file:
```
dotnet run --project src/Awk.Cli -- absence-regions users-assign \
  --set regionId=<region-id> \
  --set-json userIds=@/tmp/users.json
```

Nested body properties:
```
dotnet run --project src/Awk.Cli -- task-tags tasks-update-tags \
  --set newTag.name=Priority
```

Invite user (skip email) + accept:
```
dotnet run --project src/Awk.Cli -- invitations create --body @samples/invite.json
dotnet run --project src/Awk.Cli -- invitations accept --body @samples/accept.json
```

## Output contract
Every command prints JSON:
- `statusCode`
- `traceId` (best effort, from response headers)
- `response` (JSON when possible, otherwise raw text)

## Code generation
Source generator reads `swagger.json` and emits:
- DTOs in `Awk.Generated`
- full API client (one method per operationId)
- CLI commands grouped by Swagger tags

If swagger changes, rebuild. No manual DTOs.

## Repo layout
```
src/Awk.CodeGen      # source generator
src/Awk.Cli          # CLI app
tests/Awk.CodeGen.Tests
tests/Awk.Cli.Tests
scripts/             # bash test helpers
```

## Tests
```
./scripts/test-build.sh
./scripts/test-cli-names.sh
./scripts/test-example.sh
./scripts/test-params.sh
./scripts/test-unit.sh
```

## Publishing
CLI binary:
```
dotnet publish src/Awk.Cli -c Release -r osx-x64 --self-contained false
```
Output: `src/Awk.Cli/bin/Release/net10.0/<rid>/publish`.

Source generator package (NuGet):
```
dotnet pack src/Awk.CodeGen -c Release
```
