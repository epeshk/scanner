
using System;
using System.Globalization;
using System.IO;

namespace Epeshk.Text
{
  using System;
  using System.Globalization;
  using System.IO;

  public class TextScanner2
  {
    StreamReader input = new StreamReader(Console.OpenStandardInput(), bufferSize: 16384);
    char[] buffer = new char[4096];

    public int ReadInt()
    {
      int length = 0;
      bool readStart = false;
      while (true)
      {
        int ch = input.Read();
        if (ch == -1)
          break;

        if (char.IsWhiteSpace((char)ch))
        {
          if (readStart) break;
          continue;
        }

        readStart = true;
        buffer[length++] = (char)ch;
      }

      return int.Parse(buffer.AsSpan(0, length));
    }
  }
}