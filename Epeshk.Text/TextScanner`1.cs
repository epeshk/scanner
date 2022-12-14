#if NET7_0_OR_GREATER
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Epeshk.Text;

public class TextScanner<TConfig> : IDisposable where TConfig : struct, ITextScannerConfig
{
  private char[] buffer;
  private int length;
  private int offset;
  private readonly TextReader reader;
  private readonly bool leaveOpen;

  public TextScanner(TextReader? reader=null, int initialBufferSize=1024, bool leaveOpen=false)
  {
    this.leaveOpen = leaveOpen;
    this.reader = reader ?? new StreamReader(Console.OpenStandardInput());
    buffer = new char[initialBufferSize];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryRead<T>(out T value, IFormatProvider? formatProvider)
    where T : ISpanParsable<T>
  {
    if (typeof(T) == typeof(char))
      return TryReadChar(out value, formatProvider);
    
    ReadOnlySpan<char> span = GetNextToken();
    if (!T.TryParse(span, formatProvider, out value))
      return false;

    Consume(span.Length);
    return true;
  }

  public bool TryReadString(out string value)
  {
    value = GetNextToken().ToString();
    Consume(value.Length);
    return value.Length > 0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private bool TryReadChar<T>(out T value, IFormatProvider? formatProvider) where T : ISpanParsable<T>
  {
    if (TryReadCharFastPath(out value))
      return true;

    var span = ReadSingleCharSpan();
    if (!T.TryParse(span, formatProvider, out value))
      return false;

    Consume(1);
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private bool TryReadCharFastPath<T>(out T value) where T : ISpanParsable<T>
  {
    value = default!;
    if (offset + 2 > length)
      return false;

    // fast path for case when char is available intermediately or there is one separator
    // additional IF for `\r\n` case is not worth it by benchmark 

    var c = buffer[offset++];
    if (IsDelimiter(c))
    {
      c = buffer[offset++];
      if (IsDelimiter(c))
        return false;
    }

    value = Unsafe.As<char, T>(ref c);
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private bool TrySkipDelimitersFastPath()
  {
    if (offset + 2 > length)
      return false;

    // fast path for case when char is available intermediately or there is one separator
    // additional IF for `\r\n` case is not worth it by benchmark 

    if (!IsDelimiter(buffer[offset])  ||
        !IsDelimiter(buffer[++offset]) )
      return true;

    offset++;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private ReadOnlySpan<char> ReadSingleCharSpan()
  {
    SkipDelimiters();
    if (offset < length)
      return buffer.AsSpan(offset, 1);
    return ReadSingleCharSpanRare();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void Consume(int bytes) => offset += bytes;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private ReadOnlySpan<char> GetNextToken()
  {
    if (!TrySkipDelimitersFastPath())
      SkipDelimiters();

    int end = FindDelimiter(offset);

    if (end < length)
      return buffer.AsSpan(offset, end - offset);

    return GetNextTokenRare();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SkipDelimiters()
  {
    // CallCount++;
    while (offset < length && IsDelimiter(buffer[offset]))
      offset++;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int FindDelimiter(int end)
  {
    while (end < length && !IsDelimiter(buffer[end]))
      end++;
    return end;
  }

  // clearly show our intention to make rare called code path
  [MethodImpl(MethodImplOptions.NoInlining)]
  private ReadOnlySpan<char> ReadSingleCharSpanRare()
  {
    while (offset == length)
    {
      if (!FetchData())
        return default;
      SkipDelimiters();
    }

    return buffer.AsSpan(offset, 1);
  }

  // clearly show our intention to make rare called code path
  [MethodImpl(MethodImplOptions.NoInlining)]
  private ReadOnlySpan<char> GetNextTokenRare()
  {
    while (offset == length)
    {
      if (!FetchData())
        return ReadOnlySpan<char>.Empty;

      SkipDelimiters();
    }

    MoveDataToBufferStart();

    int end = 0;

    while (true)
    {
      end = FindDelimiter(end);

      if (end < length)
        return buffer.AsSpan(0, end);
      //
      // if (length >= MAX_TOKEN_LENGTH)
      //   return ReadOnlySpan<char>.Empty;

      if (!FetchData())
        return buffer.AsSpan(0, end);
    }
  }

  private bool FetchData()
  {
    MoveDataToBufferStart();

    if (length == buffer.Length)
      GrowBuffer();

    // read the data from TextReader right after remaining data
    int count = reader.Read(buffer, length, buffer.Length - length);

    // end of the data reached
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
  private static bool IsDelimiter(char c)
  {
    return TConfig.IsDelimiter(c);
  }

  private void GrowBuffer()
  {
    var newBuffer = new char[buffer.Length * 2];
    buffer.CopyTo(newBuffer, 0);
    buffer = newBuffer;
  }

  public void Dispose()
  {
    if (!leaveOpen)
      reader.Dispose();
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryRead<T>(out T value)
    where T : ISpanParsable<T>
    => TryRead(out value, CultureInfo.InvariantCulture);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T Read<T>()
    where T : ISpanParsable<T>
    => Read<T>(CultureInfo.InvariantCulture);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T Read<T>(IFormatProvider? formatProvider)
    where T : ISpanParsable<T>
  {
    if (!TryRead(out T value, formatProvider))
      ThrowFormatException();
    return value;
  }
  
  public string ReadString()
  {
    if (!TryReadString(out var value))
      ThrowFormatException();
    return value;
  }
  
  private static void ThrowFormatException() => throw new FormatException();
}
#endif