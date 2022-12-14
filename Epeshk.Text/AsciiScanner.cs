using System.Runtime.CompilerServices;

namespace Epeshk.Text;

public sealed class AsciiScanner : AsciiScanner<AsciiScanner.InvisibleAndNonAscii>
{
  public AsciiScanner(Stream? stream = null, int initialBufferSize = 4096, bool leaveOpen = false)
    : base(stream, initialBufferSize, leaveOpen)
  {
  }

  public struct InvisibleAndNonAscii : IAsciiScannerConfig
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDelimiter(byte c) => (byte)(c + 128) <= 160; // c <= ' ' || c >= 128
  }
}