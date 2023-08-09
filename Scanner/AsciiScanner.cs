using System.Runtime.CompilerServices;

namespace Scanner;

public sealed class AsciiScanner : AsciiScanner<AsciiScanner.InvisibleAndNonAscii>
{
  public AsciiScanner(Stream? stream = null, int initialBufferSize = 4096, bool leaveOpen = false)
    : base(stream, initialBufferSize, leaveOpen)
  {
  }

  public struct InvisibleAndNonAscii : IAsciiScannerConfig
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDelimiter(byte c) => (byte)(c + 128) <= 160; // c <= ' ' || c >= 128
  }
}