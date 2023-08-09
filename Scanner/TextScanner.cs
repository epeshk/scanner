using System.Runtime.CompilerServices;

namespace Scanner;

public sealed class TextScanner : TextScanner<TextScanner.WhitespaceSkippingConfig>
{
  public TextScanner(TextReader? reader=null, int initialBufferSize=2048, bool leaveOpen=false) : base(reader, initialBufferSize, leaveOpen)
  {
  }

  public struct WhitespaceSkippingConfig : ITextScannerConfig
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDelimiter(char c) => char.IsWhiteSpace(c);
  }
}
