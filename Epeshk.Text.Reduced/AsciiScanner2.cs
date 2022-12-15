
namespace Epeshk.Text
{
  using System;
  using System.Buffers.Text;
  using System.IO;

  public class AsciiScanner2
  {
    private const int MaxTokenLength = 1200;

    Stream? input = Console.OpenStandardInput();
    byte[] buffer = new byte[32768];
    Span<byte> Fragment => buffer.AsSpan(offset, length - offset);

    int offset;
    int length;

    public int ReadInt()
    {
      while (input != null && length - offset < MaxTokenLength)
      {
        if (offset != 0)
        {
          var remaining = Fragment.Length;
          Fragment.CopyTo(buffer);
          offset = 0;
          length = remaining;
        }

        var count = input.Read(buffer, length, buffer.Length - length);
        if (count <= 0)
        {
          input = null;
          break;
        }

        length += count;
        while (offset < length && buffer[offset] <= ' ') offset++;
      }
      while (offset < length && buffer[offset] <= ' ') offset++;

      var parsed = Utf8Parser.TryParse(Fragment, out int value, out int bytesConsumed);
      if (!parsed)
        Throw();
      offset += bytesConsumed;
      return value;
    }

    void Throw() => throw new Exception();
  }
}