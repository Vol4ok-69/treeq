using ProjectTreeCli.Configuration;

namespace ProjectTreeCli.Services;

public sealed class BinaryDetector
{
    public bool IsBinary(string path)
    {
        var extension = Path.GetExtension(path);

        if (DefaultBinaryExtensions.Items.Contains(extension))
        {
            return true;
        }

        try
        {
            const int sampleSize = 8000;

            using var stream = File.OpenRead(path);

            var buffer = new byte[sampleSize];

            var bytesRead = stream.Read(
                buffer,
                0,
                buffer.Length);

            for (var i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == 0)
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return true;
        }
    }
}