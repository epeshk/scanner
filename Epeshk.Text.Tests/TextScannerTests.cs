using FluentAssertions;

namespace Epeshk.Text.Scanner.Tests;

public class TextScannerTests
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
    Assert.Throws<FormatException>(() => empty.Read<int>());
  }

  [TestCase("123", 123)]
  [TestCase(" 123", 123, TestName = "Delimiter before")]
  [TestCase("123 ", 123, TestName = "Delimiter after")]
  [TestCase(" 123 ", 123, TestName = "Single delimiter before and after")]
  [TestCase("     123     ", 123, TestName = "Delimiters before and after")]
  public void ShouldReadSingleInt(string input, int expected)
  {
    var sc = Scanner(input);

    sc.Read<int>().Should().Be(expected);
    sc.TryRead(out int _).Should().BeFalse();
  }
  
  [TestCase("1 2 3", new[]{1, 2, 3})]
  [TestCase("99 88 77", new[]{99, 88, 77})]
  public void ShouldReadInts(string input, int[] expected)
  {
    var sc = Scanner(input);

    for (int i = 0; i < expected.Length; i++)
      sc.Read<int>().Should().Be(expected[i]);

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
      sc.Read<int>().Should().Be(expected);

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
      sc.Read<char>().Should().Be(c);

    sc.TryRead(out char _).Should().BeFalse();
  }

  [Test]
  public void ShouldReadString()
  {
    var strings = new[] { "ab", "cd", "ef", "gh" };
    var sc = Scanner(string.Join(" ", strings));
    foreach (var s in strings)
      sc.ReadString().Should().Be(s);
    sc.TryReadString(out _).Should().BeFalse();
    Assert.Throws<FormatException>(() => sc.ReadString());
  }

  public static TextScanner Scanner(string s) => new(new StringReader(s));
  public static TextScanner Scanner(params string[] s) => new(new PartStringReader(s));
  public static TextScanner Scanner(string s, int initialBufferSize) => new(new StringReader(s), initialBufferSize);
}

class PartStringReader : TextReader
{
  private readonly string[] strings;
  private int i = 0;
  private int j = 0;

  public PartStringReader(string[] strings)
  {
    this.strings = strings;
  }

  public override int Read(char[] buffer, int index, int count)
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
}