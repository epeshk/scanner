using System.Text;
using Microsoft.CodeAnalysis;

namespace Epeshk.Text.Scanner.SourceGenerator;

[Generator]
public class ScannerSourceGenerator : ISourceGenerator
{
  public void Initialize(GeneratorInitializationContext context)
  {
  }

  public void Execute(GeneratorExecutionContext context)
  {
    GenerateAsciiScannerSources(context);
  }

  private static void GenerateAsciiScannerSources(GeneratorExecutionContext context)
  {
    var sb = new StringBuilder();

    sb.AppendLine(@"
using System.Text;
using System.Collections.Generic;
using System.IO;
using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace Epeshk.Text;

public partial class AsciiScanner<TConfig>
{");

    var types = new[]
    {
      "bool", "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal"
    };

    var typeName = new Dictionary<string, string>
    {
      ["string"] = "String",
      ["bool"] = "Boolean",
      ["sbyte"] = "SByte",
      ["byte"] = "Byte",
      ["short"] = "Int16",
      ["ushort"] = "UInt16",
      ["int"] = "Int32",
      ["uint"] = "UInt32",
      ["long"] = "Int64",
      ["ulong"] = "UInt64",
      ["float"] = "Single",
      ["double"] = "Double",
      ["decimal"] = "Decimal",
      ["TimeSpan"] = "TimeSpan",
      ["Guid"] = "Guid",
      ["DateTime"] = "DateTime",
      ["DateTimeOffset"] = "DateTimeOffset",
    };

    foreach (var type in types)
    {
      var method = @"
  private struct #TYPENAME#Parser : IParser<#TYPE#> {[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool TryParse(ReadOnlySpan<byte> s, out #TYPE# v, out int c, char f='\0') => Utf8Parser.TryParse(s, out v, out c, f);}";
      sb.Append(method
        .Replace("#TYPE#", type)
        .Replace("#TYPENAME#", typeName[type]));
    }
    foreach (var type in types)
    {
      var method = @"
  [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool TryRead(out #TYPE# value, char format='\0') => TryRead<#TYPE#, #TYPENAME#Parser>(out value, format);
  [MethodImpl(MethodImplOptions.AggressiveInlining)] public #TYPE# Read#TYPENAME#() => Read<#TYPE#, #TYPENAME#Parser>();
";

      sb.Append(method
        .Replace("#TYPE#", type)
        .Replace("#TYPENAME#", typeName[type]));
    }

    sb.Append("}");

    context.AddSource("AsciiScanner.Generated.cs", sb.ToString());
  }
}