using Spectre.Console.Cli;

namespace Awk.Commands;

internal sealed class SkillCommand : AsyncCommand<SkillCommand.Settings>
{
    internal sealed class Settings : CommandSettings;

    protected override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        Console.WriteLine(SkillContent.Text);
        return Task.FromResult(0);
    }
}

internal static class SkillContent
{
    internal const string Text = """
# awk-cli Guide

> AI-friendly reference for the awork CLI. Pipe to your agent: `awk-cli skill`

## Overview

`awk-cli` is a CLI for the awork API. Commands are auto-generated from `swagger.json` — always in sync with the API.

## Output Contract

Every command returns a JSON envelope:

```json
{
  "statusCode": 200,
  "traceId": "00-abc123...",
  "response": { ... }
}
```

- `statusCode`: HTTP status (200-299 = success)
- `traceId`: Correlation ID for debugging
- `response`: API response body (array or object)

## Command Structure

```
awk-cli <domain> [resource] <action> [positional-args] [--options]
```

**Domains**: `users`, `tasks`, `projects`, `times`, `workspace`, `documents`, `files`, `search`, `integrations`, `automation`

**Common actions**: `list`, `get`, `create`, `update`, `delete`

## Global Options

| Option | Description |
|--------|-------------|
| `--select <FIELDS>` | Filter response fields (client-side). Example: `--select "id,name"` |
| `--output <FORMAT>` | Output format: `json` (default) or `table` |
| `--page <N>` | Page number (default: 1) |
| `--page-size <N>` | Items per page |
| `--env <PATH>` | Custom `.env` file |
| `--token <TOKEN>` | Override API token |
| `--auth-mode <MODE>` | `auto`, `token`, or `oauth` |

## Authentication

```bash
# Check auth status
awk-cli auth status

# Login with OAuth (opens browser)
awk-cli auth login

# Login with API token
awk-cli auth login --token "$AWORK_TOKEN"

# Or set env var
export AWORK_TOKEN=your-token
```

## Common Patterns

### List with pagination and field selection

```bash
awk-cli users list --page-size 10 --select "id,firstName,lastName,email"

# Table output for quick inspection
awk-cli users list --output table --select "firstName,lastName"
```

### Get by ID (positional argument)

```bash
awk-cli users get <user-id>
awk-cli tasks get <task-id>
awk-cli projects get <project-id>
```

### Create with inline options

```bash
awk-cli tasks create \
  --name "Task name" \
  --base-type private \
  --entity-id <user-id>
```

### Create from JSON file

```bash
awk-cli tasks create --body @payload.json
```

### Create with JSON + overrides

```bash
awk-cli tasks create --body @payload.json --set name="Override"
```

### Set nested properties

```bash
awk-cli tasks tags tasks-update-tags --set newTag.name=Priority
```

### Set JSON arrays

```bash
awk-cli workspace absence-regions users-assign \
  --set regionId=<region-id> \
  --set-json userIds='["user-1","user-2"]'
```

## jq Integration

```bash
# Get first user ID
awk-cli users list --page-size 1 | jq -r '.response[0].id'

# List all project names
awk-cli projects list | jq -r '.response[].name'

# Check success
awk-cli users me | jq -e '.statusCode == 200' > /dev/null && echo "OK"

# Chain: create task for first user
USER_ID=$(awk-cli users list --page-size 1 | jq -r '.response[0].id')
awk-cli tasks create --name "Welcome" --base-type private --entity-id "$USER_ID"
```

## Discovering Commands

```bash
# List domains
awk-cli --help

# List actions in domain
awk-cli users --help
awk-cli tasks --help

# Get help for specific command
awk-cli users list --help
awk-cli tasks create --help
```

## Key Endpoints Reference

### Users

```bash
awk-cli users list                      # List all users
awk-cli users get <id>                  # Get user by ID
awk-cli users me                        # Get current user
awk-cli users update <id> --position X  # Update user
awk-cli users delete <id>               # Delete user
```

### Tasks

```bash
awk-cli tasks list                      # List tasks
awk-cli tasks get <id>                  # Get task
awk-cli tasks create --name X --base-type private --entity-id <user-id>
awk-cli tasks update <id> --name "New name"
awk-cli tasks delete <id>
```

### Projects

```bash
awk-cli projects list                   # List projects
awk-cli projects get <id>               # Get project
awk-cli projects create --name X        # Create project
```

### Time Entries

```bash
awk-cli times list                      # List time entries
awk-cli times create --task-id X --duration 3600
```

### Search

```bash
awk-cli search get-search \
  --search-term "query" \
  --search-types "user,task,project" \
  --top 10
```

### Workspace

```bash
awk-cli workspace teams list
awk-cli workspace roles list
awk-cli workspace absence-regions list
```

## Error Handling

Non-2xx responses still return the envelope:

```json
{
  "statusCode": 400,
  "traceId": "...",
  "response": {"error": "Bad Request", "message": "..."}
}
```

Check `statusCode` to determine success:

```bash
result=$(awk-cli users get invalid-id)
status=$(echo "$result" | jq '.statusCode')
if [[ "$status" -ge 200 && "$status" -lt 300 ]]; then
  echo "Success"
else
  echo "Error: $status"
fi
```

## Tips for AI Agents

1. **Always check `statusCode`** — don't assume success
2. **Use `--select`** to reduce response size when you only need specific fields
3. **Use `--page-size`** for large lists to avoid timeouts
4. **Use `jq -r`** for raw string output (no quotes)
5. **Use `jq -e`** for exit code based on expression result
6. **Discover with `--help`** — commands match the API spec exactly
""";
}
