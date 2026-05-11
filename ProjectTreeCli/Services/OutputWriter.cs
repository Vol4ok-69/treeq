namespace ProjectTreeCli.Services;

public sealed class OutputWriter : IDisposable
{
    private readonly TextWriter _writer;

    public OutputWriter(string? outputPath = null)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            _writer = Console.Out;
        }
        else
        {
            _writer = new StreamWriter(
                outputPath,
                false,
                System.Text.Encoding.UTF8);
        }
    }

    public void WriteLine(string text = "")
    {
        _writer.WriteLine(text);
    }

    public void Dispose()
    {
        _writer.Flush();

        if (_writer != Console.Out)
        {
            _writer.Dispose();
        }
    }
}