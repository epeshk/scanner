## Epeshk.Text

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

Embeddable version for programming contests available in the project repository!

And not forget to visit [my blog](https://epeshk.github.io)
