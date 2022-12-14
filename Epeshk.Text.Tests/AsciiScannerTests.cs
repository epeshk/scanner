using System.Text;
using FluentAssertions;

namespace Epeshk.Text.Scanner.Tests;

public class AsciiScannerTests
{
  [SetUp]
  public void Setup()
  {
  }

  [Test]
  public void ShouldWorkOnEmptyInput()
  {
    var empty = Scanner("");

    empty.TryRead(out int _).Should().BeFalse();
    Assert.Throws<FormatException>(() => empty.ReadInt32());
  }

  [TestCase("123", 123)]
  [TestCase(" 123", 123, TestName = "Delimiter before")]
  [TestCase("123 ", 123, TestName = "Delimiter after")]
  [TestCase(" 123 ", 123, TestName = "Single delimiter before and after")]
  [TestCase("     123     ", 123, TestName = "Delimiters before and after")]
  public void ShouldReadSingleInt(string input, int expected)
  {
    var sc = Scanner(input);

    sc.ReadInt32().Should().Be(expected);
    sc.TryRead(out int _).Should().BeFalse();
  }
  
  [TestCase("1 2 3", new[]{1, 2, 3})]
  [TestCase("99 88 77", new[]{99, 88, 77})]
  public void ShouldReadInts(string input, int[] expected)
  {
    var sc = Scanner(input);

    for (int i = 0; i < expected.Length; i++)
      sc.ReadInt32().Should().Be(expected[i]);

    sc.TryRead(out int _).Should().BeFalse();
  }
  
  [TestCase(1, 100_000, 1, " ")]
  [TestCase(1, 100_000, 1, "       ")]
  public void ShouldReadInts_BufferSize(int seed, int count, int bufferSize, string delimiter)
  {
    var random = new Random(seed);
    var array = Enumerable.Range(0, count).Select(_ => random.Next()).ToArray();
    var input = string.Join(delimiter, array);
    
    var sc = Scanner(input, bufferSize);
  
    foreach (var expected in array)
      sc.ReadInt32().Should().Be(expected);

    sc.TryRead(out int _).Should().BeFalse();
  }
  
  [TestCase(1, 100_000, 1, " ")]
  [TestCase(1, 100_000, 1, "       ")]
  public void ShouldReadDoubles_BufferSize(int seed, int count, int bufferSize, string delimiter)
  {
    var random = new Random(seed);
    var array = Enumerable.Range(0, count).Select(_ => random.NextDouble()).ToArray();
    var input = string.Join(delimiter, array.Select(x => x.ToString("F500")));
    
    var sc = Scanner(input, bufferSize);
  
    foreach (var expected in array)
      sc.ReadDouble().Should().Be(expected);

    sc.TryRead(out int _).Should().BeFalse();
  }
  [TestCase(1, 100_000, 1, " ")]
  [TestCase(1, 100_000, 1, "       ")]
  public void ShouldReadStrings_BufferSize(int seed, int count, int bufferSize, string delimiter)
  {
    var random = new Random(seed);
    var array = Enumerable.Range(0, count).Select(_ => random.NextDouble().ToString()).ToArray();
    var input = string.Join(delimiter, array);
    
    var sc = Scanner(input, bufferSize);

    foreach (var expected in array)
    {
      sc.ReadString().Should().Be(expected);
    }

    sc.TryRead(out int _).Should().BeFalse();
  }

  [TestCase("texttexttext", 1)]
  [TestCase("    te xt   te  xt  te xt ", 1)]
  [TestCase("a a", 1)]
  [TestCase("a a", 2)]
  [TestCase("a a", 3)]
  [TestCase("a  a", 4)]
  public void ShouldReadChars(string text, int bufferSize)
  {
    var expected = new string(text.Where(x => x != ' ').ToArray());
    
    var sc = Scanner(text, bufferSize);
  
    foreach (var c in expected)
      sc.ReadChar().Should().Be(c);
  
    sc.TryRead(out char _).Should().BeFalse();
  }
  
  [Test]
  public void ShouldReadString()
  {
    var strings = new[] { "ab", "cd", "ef", "gh" };
    var sc = Scanner(string.Join(" ", strings));
    foreach (var s in strings)
      sc.ReadString().Should().Be(s);
    sc.TryRead(out string _).Should().BeFalse();
    Assert.Throws<FormatException>(() => sc.ReadString());
  }

  public static AsciiScanner Scanner(string s) => new(new MemoryStream(Encoding.ASCII.GetBytes(s)));
  public static AsciiScanner Scanner(params byte[][] s) => new(new PartStream(s));
  public static AsciiScanner Scanner(string s, int initialBufferSize) => new(new MemoryStream(Encoding.ASCII.GetBytes(s)), initialBufferSize);
}


class PartStream : Stream
{
  private readonly byte[][] strings;
  private int i = 0;
  private int j = 0;

  public PartStream(byte[][] strings)
  {
    this.strings = strings;
  }

  public override void Flush()
  {
    throw new NotImplementedException();
  }

  public override int Read(byte[] buffer, int index, int count)
  {
    if (i >= strings.Length)
      return 0;

    var size = Math.Min(count, strings[i].Length - j);
    strings[i].AsSpan(j, size).CopyTo(buffer.AsSpan(index));
    j += size;
    if (j == strings[i].Length)
    {
      i++;
      j = 0;
    }

    return size;
  }

  public override long Seek(long offset, SeekOrigin origin)
  {
    throw new NotImplementedException();
  }

  public override void SetLength(long value)
  {
    throw new NotImplementedException();
  }

  public override void Write(byte[] buffer, int offset, int count)
  {
    throw new NotImplementedException();
  }

  public override bool CanRead => true;
  public override bool CanSeek { get; }
  public override bool CanWrite { get; }
  public override long Length { get; }
  public override long Position { get; set; }
}