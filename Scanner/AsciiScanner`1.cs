using System.Runtime.CompilerServices;
using System.Text;

namespace Scanner;

public partial class AsciiScanner<TConfig> : IDisposable
  where TConfig : struct, IAsciiScannerConfig
{
  private static TConfig config;

  private byte[] buffer;
  private int length;
  private int offset;
  private readonly Stream stream;
  private readonly bool leaveOpen;

  protected AsciiScanner(Stream? stream=null, int initialBufferSize=32768, bool leaveOpen=false)
  {
    if (initialBufferSize <= 0)
      throw new ArgumentException(nameof(initialBufferSize));

    this.leaveOpen = leaveOpen;
    this.stream = stream ?? Console.OpenStandardInput();
    buffer = new byte[initialBufferSize];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private bool TryRead<T, TParser>(out T value, char format='\0')
    where TParser : struct, IParser<T>
  {
    value = default!;

    SkipDelimiters();
    bool result = TParser.TryParse(Fragment, out value, out int bytesConsumed, format);
    if (result && offset + bytesConsumed < length)
    {
      offset += bytesConsumed;
      return true;
    }
    return TryReadRare<T, TParser>(out value, format);
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  private bool TryReadRare<T, TParser>(out T value, char format)
    where TParser : struct, IParser<T>
  {
    bool result;
    int bytesConsumed;
    if (!Fragment.IsEmpty)
    {
      result = TParser.TryParse(Fragment, out value, out bytesConsumed, format);
      if (!result && !IsReadIncomplete<T>())
      {
        value = default!;
        return false;
      }
    }

    while (FetchData())
    {
      SkipDelimiters();
      result = TParser.TryParse(Fragment, out value, out bytesConsumed, format);
      if (result)
      {
        if (offset + bytesConsumed < length)
        {
          offset += bytesConsumed;
          return true;
        }
      }
      else if (!IsReadIncomplete<T>())
        return false;
    }

    result = TParser.TryParse(Fragment, out value, out var bc, format);
    if (result)
      offset += bc;
    return result;
  }

  private Span<byte> Fragment
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => buffer.AsSpan(offset, length - offset);
  }

  private bool IsReadIncomplete<T>()
  {
    if (typeof(T) == typeof(double) ||
        typeof(T) == typeof(float) ||
        typeof(T) == typeof(TimeSpan) ||
        typeof(T) == typeof(DateTime) ||
        typeof(T) == typeof(DateTimeOffset))
      return IsReadIncompleteUnbounded();

    if (typeof(T) == typeof(Guid))
      return Fragment.Length < 42;

    return Fragment.Length < 32;
  }

  private bool IsReadIncompleteUnbounded()
  {
    var span = Fragment;
    if (span.Length < 10)
      return false;
    var c = span[span.Length - 1];
    return c - '+' <= '/' - '+' || (c & ~0x20u) is 'E' or 'T' or 'Z';
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SkipDelimiters()
  {
    while (offset < length && IsDelimiter(buffer[offset]))
      offset++;
  }

  private bool FetchData()
  {
    MoveDataToBufferStart();

    if (length == buffer.Length)
      GrowBuffer();

    int count = stream.Read(buffer, length, buffer.Length - length);

    if (count > 0)
      length += count;

    return count > 0;
  }

  private void MoveDataToBufferStart()
  {
    // calculate the length of unused data span
    int remaining = length - offset;

    // shift unused data to the beginning
    buffer.AsSpan(offset, remaining).CopyTo(buffer);
    offset = 0;
    length = remaining;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool IsDelimiter(byte c) => TConfig.IsDelimiter(c);

  private void GrowBuffer()
  {
    var newBuffer = new byte[buffer.Length * 2];
    buffer.CopyTo(newBuffer, 0);
    buffer = newBuffer;
  }

  public void Dispose()
  {
    if (!leaveOpen)
      stream.Dispose();
  }

  private static void ThrowFormatException() => throw new FormatException();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private T Read<T, TParser>(char format='\0') where TParser : struct,IParser<T> {
    if (!TryRead<T, TParser>(out T value, format))
      ThrowFormatException();
    return value;
  }

  private interface IParser<T>
  {
    static abstract bool TryParse(
      ReadOnlySpan<byte> source,
      out T value,
      out int bytesConsumed,
      char standardFormat = default);
  }

  private struct CharParser : IParser<char>
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<byte> source, out char value, out int bytesConsumed, char standardFormat = default)
    {
      if (source.IsEmpty)
      {
        value = default;
        bytesConsumed = 0;
        return false;
      }

      value = (char)source[0];
      bytesConsumed = 1;
      return true;
    }
  }

  private struct StringParser : IParser<string>
  {
    public static bool TryParse(ReadOnlySpan<byte> source, out string value, out int bytesConsumed, char standardFormat = default)
    {
      if (source.IsEmpty)
      {
        value = default!;
        bytesConsumed = 0;
        return false;
      }

      var sb = new StringBuilder();
      foreach (var b in source)
      {
        if (IsDelimiter(b))
          break;
        sb.Append((char)b);
      }

      value = sb.ToString();
      bytesConsumed = value.Length;
      return true;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryRead(out string value, char format = default) => TryRead<string, StringParser>(out value, format);
  public string ReadString(char format = default) => Read<string, StringParser>(format);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryRead(out char value, char format = default) => TryRead<char, CharParser>(out value, format);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public char ReadChar(char format = default) => Read<char, CharParser>(format);
}