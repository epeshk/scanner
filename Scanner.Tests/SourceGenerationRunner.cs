using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;
using Scanner.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Scanner.Tests;

public class SourceGenerationRunner
{
  [Test]
  public void Run()
  {
    var comp = CreateCompilation("");
    var newComp = RunGenerators(comp, out _, new ScannerSourceGenerator());

    var newFiles = newComp.SyntaxTrees
      .Where(x => Path.GetFileName(x.FilePath).EndsWith(".Generated.cs"));

    foreach (var newFile in newFiles)
    {
      var swriter = new StringWriter();
      newFile.GetText().Write(swriter);
      Console.WriteLine(swriter.ToString());
    }
  }
  
  private static Compilation CreateCompilation(string source)
    => CSharpCompilation.Create("compilation",
      new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
      new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location), MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).GetTypeInfo().Assembly.Location) },
      new CSharpCompilationOptions(OutputKind.ConsoleApplication));

  private static GeneratorDriver CreateDriver(params ISourceGenerator[] generators)
    => CSharpGeneratorDriver.Create(generators);

  private static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
  {
    CreateDriver(generators).RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics);
    return newCompilation;
  }
}