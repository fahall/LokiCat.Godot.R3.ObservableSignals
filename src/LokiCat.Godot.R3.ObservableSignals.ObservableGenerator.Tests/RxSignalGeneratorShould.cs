using System.Text;
using FluentAssertions;
using LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit.Abstractions;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Tests;

public class RxSignalGeneratorShould
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RxSignalGeneratorShould(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void GenerateSignalAndPropertyForRxSignalField()
    {
        const string rxSignalInput = """
                                     using R3;
                                     using Godot;
                                     using LokiCat.Godot.R3.ObservableSignals;

                                     public partial class TestNode : Node
                                     {
                                         [RxSignal]
                                         private Subject<Unit> _onJump = new();
                                     }
                                     """;

        const string r3Stub = """
                              namespace R3 {
                                  public class Subject<T> : Observable<T> {
                                      public void OnNext(T value) { }
                                  }
                                  public class Observable<T> { }
                                  public struct Unit {}
                              }
                              """;

        const string attributeStub = """
                                     using System;
                                     namespace LokiCat.Godot.R3.ObservableSignals {
                                         [AttributeUsage(AttributeTargets.Field)]
                                         public sealed class RxSignalAttribute : Attribute { }
                                     }
                                     """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(SourceText.From(rxSignalInput, Encoding.UTF8)),
            CSharpSyntaxTree.ParseText(SourceText.From(r3Stub, Encoding.UTF8)),
            CSharpSyntaxTree.ParseText(SourceText.From(attributeStub, Encoding.UTF8))
        };

        var references = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                                  .Select(a => MetadataReference.CreateFromFile(a.Location))
                                  .Cast<MetadataReference>()
                                  .ToList();

        var compilation = CSharpCompilation.Create("TestAssembly",
                                                   syntaxTrees,
                                                   references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new RxSignalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
                                          .RunGeneratorsAndUpdateCompilation(
                                              compilation, out var updatedCompilation, out _);
        var result = driver.GetRunResult();

        result.Results.Should().NotBeNullOrEmpty("the generator should have executed");

        var generatedSources = result.Results.SelectMany(r => r.GeneratedSources).ToList();
        generatedSources.Should().NotBeEmpty("at least one file should have been generated");

        generatedSources.Select(s => s.HintName).Should().Contain(n => n.Contains("TestNode"));

        var source = generatedSources.FirstOrDefault(s => s.HintName.Contains("TestNode"));
        source.Should().NotBeNull("we should find a generated file for TestNode");

        var code = source!.SourceText.ToString();

        code.Should().Contain("public delegate void JumpEventHandler();");
        code.Should().Contain("public Observable<R3.Unit> OnJump");
        code.Should().Contain("EmitSignal(nameof(Jump))");
    }

    [Fact]
    public void GenerateMultipleSignalsFromMultipleRxSignalFields()
    {
        const string rxSignalInput = """
                                     using R3;
                                     using Godot;
                                     using LokiCat.Godot.R3.ObservableSignals;

                                     public partial class TestNode : Node
                                     {
                                         [RxSignal] private Subject<R3.Unit> _onJump = new();
                                         [RxSignal] private Subject<R3.Unit> _onLand = new();
                                     }
                                     """;

        const string r3Stub = """
                              namespace R3 {
                                  public class Subject<T> : Observable<T> {
                                      public void OnNext(T value) { }
                                  }
                                  public class Observable<T> { }
                                  public struct Unit {}
                              }
                              """;

        const string attributeStub = """
                                     using System;
                                     namespace LokiCat.Godot.R3.ObservableSignals {
                                         [AttributeUsage(AttributeTargets.Field)]
                                         public sealed class RxSignalAttribute : Attribute { }
                                     }
                                     """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(rxSignalInput),
            CSharpSyntaxTree.ParseText(r3Stub),
            CSharpSyntaxTree.ParseText(attributeStub)
        };

        var references = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                                  .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create("TestAssembly", syntaxTrees, references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new RxSignalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
                                          .RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var result = driver.GetRunResult();

        var source = result.Results.SelectMany(r => r.GeneratedSources).First(s => s.HintName.Contains("TestNode"));
        var code = source.SourceText.ToString();

        code.Should().Contain("public delegate void JumpEventHandler();");
        code.Should().Contain("public Observable<R3.Unit> OnJump");
        code.Should().Contain("EmitSignal(nameof(Jump))");

        code.Should().Contain("public delegate void LandEventHandler();");
        code.Should().Contain("public Observable<R3.Unit> OnLand");
        code.Should().Contain("EmitSignal(nameof(Land))");
    }

    [Fact]
    public void GenerateSignalWithArgumentsFromObservableT()
    {
        const string rxSignalInput = """
                                     using R3;
                                     using Godot;
                                     using LokiCat.Godot.R3.ObservableSignals;

                                     public partial class TestNode : Node
                                     {
                                         [RxSignal] private Subject<string> _onNamed = new();
                                     }
                                     """;

        const string r3Stub = """
                              namespace R3 {
                                  public class Subject<T> : Observable<T> {
                                      public void OnNext(T value) { }
                                  }
                                  public class Observable<T> { }
                              }
                              """;

        const string attributeStub = """
                                     using System;
                                     namespace LokiCat.Godot.R3.ObservableSignals {
                                         [AttributeUsage(AttributeTargets.Field)]
                                         public sealed class RxSignalAttribute : Attribute { }
                                     }
                                     """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(rxSignalInput),
            CSharpSyntaxTree.ParseText(r3Stub),
            CSharpSyntaxTree.ParseText(attributeStub)
        };

        var references = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                                  .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create("TestAssembly", syntaxTrees, references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new RxSignalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
                                          .RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var result = driver.GetRunResult();

        var source = result.Results.SelectMany(r => r.GeneratedSources).First(s => s.HintName.Contains("TestNode"));
        var code = source.SourceText.ToString();

        code.Should().Contain("public delegate void NamedEventHandler(string value);");
        code.Should().Contain("Subscribe(value => EmitSignal(nameof(Named), value))");
        code.Should().Contain("EmitSignal(nameof(Named), value)");
    }

    [Fact]
    public void WarnIfRxSignalFieldIsNotObservable()
    {
        const string input = """
                             using Godot;
                             using LokiCat.Godot.R3.ObservableSignals;

                             public partial class TestNode : Node
                             {
                                 [RxSignal] private int _badSignal = 5;
                             }
                             """;

        const string attrStub = """
                                using System;
                                namespace LokiCat.Godot.R3.ObservableSignals {
                                    [AttributeUsage(AttributeTargets.Field)]
                                    public sealed class RxSignalAttribute : Attribute { }
                                }
                                """;

        var trees = new[]
        {
            CSharpSyntaxTree.ParseText(input),
            CSharpSyntaxTree.ParseText(attrStub)
        };

        var references = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                                  .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create("TestAssembly", trees, references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new RxSignalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
                                          .RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        diagnostics.Should().Contain(d => d.Id == "RXSG0002");
        diagnostics.Any(d => d.GetMessage().Contains("_badSignal"))
                   .Should()
                   .BeTrue("the diagnostic message should mention the bad field name");
    }

    [Fact]
    public void DoesNotGenerateCodeIfNoRxSignalFields()
    {
        const string input = """
                             using Godot;

                             public partial class TestNode : Node
                             {
                                 private int _foo = 0;
                             }
                             """;

        var syntaxTrees = new[] { CSharpSyntaxTree.ParseText(input) };

        var references = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                                  .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create("TestAssembly", syntaxTrees, references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new RxSignalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
                                          .RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var result = driver.GetRunResult();

        result.Results.SelectMany(r => r.GeneratedSources)
              .Should()
              .OnlyContain(s => s.HintName == "RxSignalAttribute.g.cs");
    }

    [Fact]
    public void GenerateCodeForClassImplementingInterfaceWithRxSignals()
    {
        const string input = """
                             using Godot;
                             using R3;
                             using LokiCat.Godot.R3.ObservableSignals;

                             public interface IPauseMenu {
                                 Observable<Unit> OnMainMenuSelected { get; }
                             }

                             public partial class PauseMenu : Control, IPauseMenu {
                                 [RxSignal] private Subject<Unit> _onMainMenuSelected = new();
                             }
                             """;

        const string r3Stub = """
                              namespace R3 {
                                  public class Subject<T> : Observable<T> {
                                      public void OnNext(T value) { }
                                  }
                                  public class Observable<T> { }
                                  public struct Unit {}
                              }
                              """;

        const string attributeStub = """
                                     using System;
                                     namespace LokiCat.Godot.R3.ObservableSignals {
                                         [AttributeUsage(AttributeTargets.Field)]
                                         public sealed class RxSignalAttribute : Attribute { }
                                     }
                                     """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(input),
            CSharpSyntaxTree.ParseText(r3Stub),
            CSharpSyntaxTree.ParseText(attributeStub)
        };

        var references = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                                  .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create("TestAssembly", syntaxTrees, references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new RxSignalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
                                          .RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var result = driver.GetRunResult();

        var source = result.Results.SelectMany(r => r.GeneratedSources)
                           .FirstOrDefault(s => s.HintName.Contains("PauseMenu"));
        source.Should().NotBeNull("PauseMenu should have generated observable code");

        var code = source!.SourceText.ToString();
        code.Should().Contain("public delegate void MainMenuSelectedEventHandler();");
        code.Should().Contain("public Observable<R3.Unit> OnMainMenuSelected");
        code.Should().Contain("EmitSignal(nameof(MainMenuSelected))");
    }

    [Fact]
    public void GenerateCodeForPauseMenuLikeClass()
    {
        const string input = """
                             namespace Slinky.Pause.UI.Menu;

                             using Godot;
                             using R3;
                             using LokiCat.Godot.R3.ObservableSignals;
                             using Chickensoft.AutoInject;
                             using Chickensoft.GodotNodeInterfaces;
                             using Chickensoft.Introspection;

                             public interface IPauseMenu : IControl {
                                 Observable<Unit> OnMainMenuSelected { get; }
                                 Observable<Unit> OnResumeSelected { get; }
                                 Observable<Unit> OnSaveSelected { get; }
                                 Observable<Unit> OnTransitionCompleted { get; }
                             }

                             [Meta(typeof(IAutoNode))]
                             public partial class PauseMenu : Control, IPauseMenu {
                                 [RxSignal] private Subject<Unit> _onResumeSelected = new();
                                 [RxSignal] private Subject<Unit> _onSaveSelected = new();
                                 [RxSignal] private Subject<Unit> _onTransitionCompleted = new();
                                 [RxSignal] private Subject<Unit> _onMainMenuSelected = new();
                             }
                             """;

        const string r3Stub = """
                              namespace R3 {
                                  public class Subject<T> : Observable<T> {
                                      public void OnNext(T value) { }
                                  }
                                  public class Observable<T> { }
                                  public struct Unit {}
                              }
                              """;

        const string attributeStub = """
                                     using System;
                                     namespace LokiCat.Godot.R3.ObservableSignals {
                                         [AttributeUsage(AttributeTargets.Field)]
                                         public sealed class RxSignalAttribute : Attribute { }
                                     }
                                     """;

        const string autoNodeStub = """
                                    namespace Chickensoft.AutoInject {
                                        using System;
                                        public sealed class MetaAttribute : Attribute {
                                            public MetaAttribute(Type t) {}
                                        }
                                    
                                        public interface IAutoNode {}
                                    }

                                    namespace Chickensoft.GodotNodeInterfaces {
                                        public interface IControl {}
                                    }

                                    namespace Chickensoft.Introspection {
                                        // empty placeholder
                                    }
                                    """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(input),
            CSharpSyntaxTree.ParseText(r3Stub),
            CSharpSyntaxTree.ParseText(attributeStub),
            CSharpSyntaxTree.ParseText(autoNodeStub),
        };

        var references = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                                  .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create("TestAssembly", syntaxTrees, references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new RxSignalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
                                          .RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var result = driver.GetRunResult();

        var sources = result.Results.SelectMany(r => r.GeneratedSources).ToList();
        sources.Select(s => s.HintName)
               .Should()
               .Contain(h => h.Contains("PauseMenu"), "PauseMenu should result in generated code");
    }
}