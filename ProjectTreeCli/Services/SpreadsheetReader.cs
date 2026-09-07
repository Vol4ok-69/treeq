using ClosedXML.Excel;
using System.Text;

namespace ProjectTreeCli.Services;

public sealed class SpreadsheetReader
{
    public string ReadCsv(string path)
    {
        var encoding = DetectEncoding(path);

        var text = File.ReadAllText(path, encoding);

        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var lines = text
            .Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None);

        var delimiter = DetectDelimiter(lines);

        var rows = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => ParseCsvLine(line, delimiter))
            .ToList();

        return BuildMarkdownTable(rows);
    }

    public string ReadXlsx(string path)
    {
        using var workbook = new XLWorkbook(path);

        var builder = new StringBuilder();

        foreach (var worksheet in workbook.Worksheets)
        {
            var usedRange = worksheet.RangeUsed();

            if (usedRange is null)
            {
                continue;
            }

            builder.AppendLine($"### {worksheet.Name}");
            builder.AppendLine();

            var rows = usedRange
                .Rows()
                .Select(row => row
                    .Cells()
                    .Select(cell => cell.GetFormattedString())
                    .ToList())
                .ToList();

            builder.AppendLine(
                BuildMarkdownTable(rows));

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private Encoding DetectEncoding(string path)
    {
        Encoding.RegisterProvider(
            CodePagesEncodingProvider.Instance);

        var bytes = File.ReadAllBytes(path);

        if (bytes.Length >= 3 &&
            bytes[0] == 0xEF &&
            bytes[1] == 0xBB &&
            bytes[2] == 0xBF)
        {
            return new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: true);
        }

        if (IsUtf8(bytes))
        {
            return new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false);
        }

        return Encoding.GetEncoding(1251);
    }

    private bool IsUtf8(byte[] bytes)
    {
        var i = 0;

        while (i < bytes.Length)
        {
            if (bytes[i] <= 0x7F)
            {
                i++;
                continue;
            }

            if (bytes[i] >= 0xC2 &&
                bytes[i] <= 0xDF)
            {
                if (i + 1 >= bytes.Length ||
                    !IsContinuationByte(bytes[i + 1]))
                {
                    return false;
                }

                i += 2;
                continue;
            }

            if (bytes[i] >= 0xE0 &&
                bytes[i] <= 0xEF)
            {
                if (i + 2 >= bytes.Length ||
                    !IsContinuationByte(bytes[i + 1]) ||
                    !IsContinuationByte(bytes[i + 2]))
                {
                    return false;
                }

                i += 3;
                continue;
            }

            if (bytes[i] >= 0xF0 &&
                bytes[i] <= 0xF4)
            {
                if (i + 3 >= bytes.Length ||
                    !IsContinuationByte(bytes[i + 1]) ||
                    !IsContinuationByte(bytes[i + 2]) ||
                    !IsContinuationByte(bytes[i + 3]))
                {
                    return false;
                }

                i += 4;
                continue;
            }

            return false;
        }

        return true;
    }

    private bool IsContinuationByte(byte value)
    {
        return value >= 0x80 && value <= 0xBF;
    }

    private char DetectDelimiter(string[] lines)
    {
        var firstLine = lines
            .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));

        if (string.IsNullOrEmpty(firstLine))
        {
            return ',';
        }

        var candidates = new[]
        {
            ',',
            ';',
            '\t'
        };

        return candidates
            .OrderByDescending(
                delimiter => CountDelimiter(
                    firstLine,
                    delimiter))
            .First();
    }

    private int CountDelimiter(
        string line,
        char delimiter)
    {
        var count = 0;
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];

            if (character == '"')
            {
                if (insideQuotes &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    i++;
                    continue;
                }

                insideQuotes = !insideQuotes;
                continue;
            }

            if (character == delimiter &&
                !insideQuotes)
            {
                count++;
            }
        }

        return count;
    }

    private List<string> ParseCsvLine(
        string line,
        char delimiter)
    {
        var result = new List<string>();
        var value = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];

            if (character == '"')
            {
                if (insideQuotes &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    value.Append('"');
                    i++;
                    continue;
                }

                insideQuotes = !insideQuotes;
                continue;
            }

            if (character == delimiter &&
                !insideQuotes)
            {
                result.Add(value.ToString());
                value.Clear();
                continue;
            }

            value.Append(character);
        }

        result.Add(value.ToString());

        return result;
    }

    private string BuildMarkdownTable(
        List<List<string>> rows)
    {
        if (rows.Count == 0)
        {
            return string.Empty;
        }

        var columnCount = rows.Max(row => row.Count);

        foreach (var row in rows)
        {
            while (row.Count < columnCount)
            {
                row.Add(string.Empty);
            }
        }

        var builder = new StringBuilder();

        builder.AppendLine(
            "| " +
            string.Join(
                " | ",
                rows[0].Select(EscapeMarkdown)) +
            " |");

        builder.AppendLine(
            "| " +
            string.Join(
                " | ",
                Enumerable.Repeat(
                    "---",
                    columnCount)) +
            " |");

        foreach (var row in rows.Skip(1))
        {
            builder.AppendLine(
                "| " +
                string.Join(
                    " | ",
                    row.Select(EscapeMarkdown)) +
                " |");
        }

        return builder.ToString().TrimEnd();
    }

    private string EscapeMarkdown(string value)
    {
        return value
            .Replace("|", "\\|")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }
}