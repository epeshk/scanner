# Scanner

Tired of writing something like this:
```csharp
var n = int.Parse(Console.ReadLine());
var input = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
```

Just replace it with:
```csharp
var sc = new AsciiScanner();
var input = new int[sc.ReadInt32()];
for (int i = 0; i < input.Length; ++i)
  input[i] = sc.ReadInt32();
```

And not forget to use `StreamWriter.Write` instead of `Console.Write` to avoid excessive flushing!

```csharp
using var output = new StreamWriter(Console.OpenStandardOutput());

output.WriteLine(Process(line));
```

## Embeddable version

```csharp
class Scanner
{
  StreamReader input = new(Console.OpenStandardInput(), bufferSize: 16384);
  char[] buffer = new char[4096];

  public int ReadInt()
  {
    var length = PrepareToken();
    return int.Parse(buffer.AsSpan(0, length));
  }

  public long ReadLong()
  {
    var length = PrepareToken();
    return long.Parse(buffer.AsSpan(0, length));
  }

  public double ReadDouble()
  {
    var length = PrepareToken();
    return double.Parse(buffer.AsSpan(0, length));
  }

  public string ReadString()
  {
    var length = PrepareToken();
    return new string(buffer, 0, length);
  }

  private int PrepareToken()
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

    return length;
  }
}
```