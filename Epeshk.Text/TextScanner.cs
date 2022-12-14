#if NET7_0_OR_GREATER

using System.Runtime.CompilerServices;

namespace Epeshk.Text;

public sealed class TextScanner : TextScanner<TextScanner.WhitespaceSkippingConfig>
{
  public TextScanner(TextReader? reader=null, int initialBufferSize=1024, bool leaveOpen=false) : base(reader, initialBufferSize, leaveOpen)
  {
  }

  public struct WhitespaceSkippingConfig : ITextScannerConfig
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDelimiter(char c) => char.IsWhiteSpace(c);
  }
}

#endif