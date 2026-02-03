using System.Text.Json;
using System.Text.Json.Nodes;
using Awk.Models;
using Spectre.Console;

namespace Awk.Cli;

internal static class TableConsole
{
    internal static int Write(ResponseEnvelope<object?> envelope)
    {
        if (envelope.StatusCode < 200 || envelope.StatusCode >= 300)
        {
            AnsiConsole.MarkupLine($"[red]Error {envelope.StatusCode}[/]");
            if (envelope.Response is JsonElement errorJson)
            {
                AnsiConsole.WriteLine(errorJson.ToString());
            }
            else if (envelope.Response is not null)
            {
                AnsiConsole.WriteLine(envelope.Response.ToString() ?? "");
            }
            return 1;
        }

        switch (envelope.Response)
        {
            case JsonElement json:
                WriteJsonElement(json);
                break;
            case JsonArray jsonArray:
                WriteJsonArray(jsonArray);
                break;
            case JsonObject jsonObject:
                WriteJsonObject(jsonObject);
                break;
            default:
                AnsiConsole.WriteLine(envelope.Response?.ToString() ?? "");
                break;
        }

        return 0;
    }

    private static void WriteJsonElement(JsonElement json)
    {
        if (json.ValueKind == JsonValueKind.Array)
        {
            WriteArray(json);
        }
        else if (json.ValueKind == JsonValueKind.Object)
        {
            WriteObject(json);
        }
        else
        {
            AnsiConsole.WriteLine(json.ToString());
        }
    }

    private static void WriteJsonArray(JsonArray array)
    {
        var items = array.ToList();
        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No results[/]");
            return;
        }

        var columns = CollectColumnsFromNodes(items);
        if (columns.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No columns to display[/]");
            return;
        }

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.ShowRowSeparators();

        foreach (var col in columns)
        {
            table.AddColumn(new TableColumn(col).NoWrap());
        }

        foreach (var item in items)
        {
            var row = new List<string>();
            foreach (var col in columns)
            {
                row.Add(GetCellValueFromNode(item, col));
            }
            table.AddRow(row.Select(Markup.Escape).ToArray());
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{items.Count} row(s)[/]");
    }

    private static void WriteJsonObject(JsonObject obj)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.ShowRowSeparators();
        table.AddColumn("Property");
        table.AddColumn("Value");

        foreach (var prop in obj)
        {
            var value = FormatNodeValue(prop.Value);
            table.AddRow(Markup.Escape(prop.Key), Markup.Escape(value));
        }

        AnsiConsole.Write(table);
    }

    private static List<string> CollectColumnsFromNodes(List<JsonNode?> items)
    {
        var columns = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            if (item is not JsonObject obj) continue;

            foreach (var prop in obj)
            {
                if (seen.Add(prop.Key))
                {
                    columns.Add(prop.Key);
                }
            }
        }

        return columns;
    }

    private static string GetCellValueFromNode(JsonNode? item, string column)
    {
        if (item is not JsonObject obj) return "";
        if (!obj.TryGetPropertyValue(column, out var value) || value is null) return "";
        return FormatNodeValue(value);
    }

    private static string FormatNodeValue(JsonNode? value)
    {
        if (value is null) return "";

        return value switch
        {
            JsonValue jv => jv.ToString(),
            JsonArray ja => $"[{ja.Count} items]",
            JsonObject => "{...}",
            _ => value.ToJsonString()
        };
    }

    private static void WriteArray(JsonElement array)
    {
        var items = array.EnumerateArray().ToList();
        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No results[/]");
            return;
        }

        var columns = CollectColumns(items);
        if (columns.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No columns to display[/]");
            return;
        }

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.ShowRowSeparators();

        foreach (var col in columns)
        {
            table.AddColumn(new TableColumn(col).NoWrap());
        }

        foreach (var item in items)
        {
            var row = new List<string>();
            foreach (var col in columns)
            {
                row.Add(GetCellValue(item, col));
            }
            table.AddRow(row.Select(Markup.Escape).ToArray());
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{items.Count} row(s)[/]");
    }

    private static void WriteObject(JsonElement obj)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.ShowRowSeparators();
        table.AddColumn("Property");
        table.AddColumn("Value");

        foreach (var prop in obj.EnumerateObject())
        {
            var value = FormatValue(prop.Value);
            table.AddRow(Markup.Escape(prop.Name), Markup.Escape(value));
        }

        AnsiConsole.Write(table);
    }

    private static List<string> CollectColumns(List<JsonElement> items)
    {
        var columns = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            if (item.ValueKind != JsonValueKind.Object) continue;

            foreach (var prop in item.EnumerateObject())
            {
                if (seen.Add(prop.Name))
                {
                    columns.Add(prop.Name);
                }
            }
        }

        return columns;
    }

    private static string GetCellValue(JsonElement item, string column)
    {
        if (item.ValueKind != JsonValueKind.Object) return "";
        if (!item.TryGetProperty(column, out var value)) return "";
        return FormatValue(value);
    }

    private static string FormatValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null => "",
            JsonValueKind.String => value.GetString() ?? "",
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Array => $"[{value.GetArrayLength()} items]",
            JsonValueKind.Object => "{...}",
            _ => value.GetRawText()
        };
    }
}
