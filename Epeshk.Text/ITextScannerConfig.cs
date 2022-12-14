namespace Epeshk.Text;
#if NET7_0_OR_GREATER
public interface ITextScannerConfig
{
  static abstract bool IsDelimiter(char c);
}

#endif
