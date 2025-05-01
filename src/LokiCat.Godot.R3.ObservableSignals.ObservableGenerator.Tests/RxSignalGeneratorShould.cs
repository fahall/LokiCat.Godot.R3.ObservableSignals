using System.Text;
using FluentAssertions;
using LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Features.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Xunit.Abstractions;

namespace LokiCat.Godot.R3.ObservableSignals.ObservableGenerator.Tests;

public class RxSignalGeneratorShould
{
    private readonly ITestOutputHelper _testOutputHelper;
    public RxSignalGeneratorShould(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

    private const string R3_STUB = """
                                  namespace R3 {
                                      public class Subject<T> : Observable<T> {
                                          public void OnNext(T value) { }
                                      }
                                      public class Observable<T> { }
                                      public struct Unit {}
                                  }
                                  """;

    private const string RX_SIGNAL_ATTRIBUTE_STUB = """
                                                 using System;
                                                 namespace LokiCat.Godot.R3.ObservableSignals {
                                                     [AttributeUsage(AttributeTargets.Field)]
                                                     public sealed class RxSignalAttribute : Attribute { }
                                                 }
                                                 """;

    private static CSharpCompilation
        CreateCompilation(IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references) =>
        CSharpCompilation.Create("TestAssembly", trees, references,
                                 new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static GeneratorDriverRunResult RunGenerator(CSharpCompilation compilation) => CSharpGeneratorDriver
        .Create(new RxSignalGenerator())
        .RunGeneratorsAndUpdateCompilation(compilation, out _, out _)
        .GetRunResult();

    private static IEnumerable<MetadataReference> DefaultReferences() => AppDomain.CurrentDomain
        .GetAssemblies()
        .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
        .Select(a => MetadataReference.CreateFromFile(a.Location));

    private static string GetGeneratedSource(GeneratorDriverRunResult result, string typeName) => result.Results
            .SelectMany(r => r.GeneratedSources)
            .FirstOrDefault(s => s.HintName.Contains(typeName))
            .SourceText.ToString()
        ?? throw new Exception($"Generated source for {typeName} not found.");

    #region Generation Success Cases

    [Fact]
    public void GenerateSignalAndPropertyForRxSignalField()
    {
        const string INPUT = """
                             using R3;
                             using Godot;
                             using LokiCat.Godot.R3.ObservableSignals;

                             public partial class TestNode : Node {
                                 [RxSignal] private Subject<Unit> _onJump = new();
                             }
                             """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(SourceText.From(INPUT, Encoding.UTF8)),
            CSharpSyntaxTree.ParseText(R3_STUB),
            CSharpSyntaxTree.ParseText(RX_SIGNAL_ATTRIBUTE_STUB)
        };

        var compilation = CreateCompilation(syntaxTrees, DefaultReferences());
        var result = RunGenerator(compilation);

        var code = GetGeneratedSource(result, "TestNode");

        code.Should().Contain("public delegate void JumpEventHandler();");
        code.Should().Contain("public Observable<R3.Unit> OnJump");
        code.Should().Contain("EmitSignal(nameof(Jump))");
    }

    [Fact]
    public void GenerateMultipleSignalsFromMultipleRxSignalFields()
    {
        const string INPUT = """
                             using R3;
                             using Godot;
                             using LokiCat.Godot.R3.ObservableSignals;

                             public partial class TestNode : Node {
                                 [RxSignal] private Subject<R3.Unit> _onJump = new();
                                 [RxSignal] private Subject<R3.Unit> _onLand = new();
                             }
                             """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(INPUT),
            CSharpSyntaxTree.ParseText(R3_STUB),
            CSharpSyntaxTree.ParseText(RX_SIGNAL_ATTRIBUTE_STUB)
        };

        var compilation = CreateCompilation(syntaxTrees, DefaultReferences());
        var result = RunGenerator(compilation);

        var code = GetGeneratedSource(result, "TestNode");

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
        const string INPUT = """
                             using R3;
                             using Godot;
                             using LokiCat.Godot.R3.ObservableSignals;

                             public partial class TestNode : Node {
                                 [RxSignal] private Subject<string> _onNamed = new();
                             }
                             """;

        var r3Stub = R3_STUB.Replace("Unit {}", string.Empty); // No Unit needed

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(INPUT),
            CSharpSyntaxTree.ParseText(r3Stub),
            CSharpSyntaxTree.ParseText(RX_SIGNAL_ATTRIBUTE_STUB)
        };

        var compilation = CreateCompilation(syntaxTrees, DefaultReferences());
        var result = RunGenerator(compilation);

        var code = GetGeneratedSource(result, "TestNode");

        code.Should().Contain("public delegate void NamedEventHandler(string value);");
        code.Should().Contain("EmitSignal(nameof(Named), value)");
    }

    #endregion

    #region Validation / Diagnostics

    [Fact]
    public void WarnIfRxSignalFieldIsNotObservable()
    {
        const string INPUT = """
                             using Godot;
                             using LokiCat.Godot.R3.ObservableSignals;

                             public partial class TestNode : Node {
                                 [RxSignal] private int _badSignal = 5;
                             }
                             """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(INPUT),
            CSharpSyntaxTree.ParseText(RX_SIGNAL_ATTRIBUTE_STUB)
        };

        var compilation = CreateCompilation(syntaxTrees, DefaultReferences());
        var driver = CSharpGeneratorDriver.Create(new RxSignalGenerator());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        diagnostics.Should().Contain(d => d.Id == "RXSG0002");
        diagnostics.Any(d => d.GetMessage().Contains("_badSignal")).Should().BeTrue();
    }

    [Fact]
    public void DoesNotGenerateCodeIfNoRxSignalFields()
    {
        const string INPUT = """
                             using Godot;

                             public partial class TestNode : Node {
                                 private int _foo = 0;
                             }
                             """;

        var syntaxTrees = new[] { CSharpSyntaxTree.ParseText(INPUT) };
        var compilation = CreateCompilation(syntaxTrees, DefaultReferences());
        var result = RunGenerator(compilation);

        var generated = result.Results.SelectMany(r => r.GeneratedSources).Select(s => s.HintName);
        generated.Should().OnlyContain(name => name == "RxSignalAttribute.g.cs");
    }

    #endregion

    #region Attribute Matching / Recognition

    [Fact]
    public void ShouldRecognizeRxSignalAttributeEvenIfNamespaceDiffers()
    {
        const string INPUT = """
                             using Godot;
                             using R3;
                             using MyProject.CustomNamespace;

                             public partial class TestNode : Control {
                                 [RxSignal] private Subject<Unit> _onCustom = new();
                             }
                             """;

        const string ALT_ATTRIBUTE = """
                                    using System;
                                    namespace MyProject.CustomNamespace {
                                        [AttributeUsage(AttributeTargets.Field)]
                                        public sealed class RxSignalAttribute : Attribute { }
                                    }
                                    """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(INPUT),
            CSharpSyntaxTree.ParseText(R3_STUB),
            CSharpSyntaxTree.ParseText(ALT_ATTRIBUTE)
        };

        var compilation = CreateCompilation(syntaxTrees, DefaultReferences());
        var result = RunGenerator(compilation);

        var code = GetGeneratedSource(result, "TestNode");
        code.Should().Contain("public Observable<R3.Unit> OnCustom");
    }

    [Fact]
    public void ShouldRecognizeRxSignalAttributeFromReferencedAssembly()
    {
        const string ATTRIBUTE_SOURCE = """
                                       using System;
                                       namespace LokiCat.Godot.R3.ObservableSignals {
                                           [AttributeUsage(AttributeTargets.Field)]
                                           public sealed class RxSignalAttribute : Attribute { }
                                       }
                                       """;

        var attrCompilation =
            CreateCompilation(new[] { CSharpSyntaxTree.ParseText(ATTRIBUTE_SOURCE) }, DefaultReferences());

        using var attrStream = new MemoryStream();
        attrCompilation.Emit(attrStream);
        attrStream.Seek(0, SeekOrigin.Begin);
        var attributeReference = MetadataReference.CreateFromStream(attrStream);

        const string INPUT = """
                             using Godot;
                             using R3;
                             using LokiCat.Godot.R3.ObservableSignals;

                             public partial class TestNode : Control {
                                 [RxSignal] private Subject<Unit> _onCustom = new();
                             }
                             """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(INPUT),
            CSharpSyntaxTree.ParseText(R3_STUB)
        };

        var references = DefaultReferences().Append(attributeReference);
        var compilation = CreateCompilation(syntaxTrees, references);
        var result = RunGenerator(compilation);

        var code = GetGeneratedSource(result, "TestNode");
        code.Should().Contain("public Observable<R3.Unit> OnCustom");
    }

    #endregion

    #region Interface + Structural Integration

    [Fact]
    public void GenerateCodeForClassImplementingInterfaceWithRxSignals()
    {
        const string INPUT = """
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

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(INPUT),
            CSharpSyntaxTree.ParseText(R3_STUB),
            CSharpSyntaxTree.ParseText(RX_SIGNAL_ATTRIBUTE_STUB)
        };

        var compilation = CreateCompilation(syntaxTrees, DefaultReferences());
        var result = RunGenerator(compilation);

        var code = GetGeneratedSource(result, "PauseMenu");
        code.Should().Contain("public delegate void MainMenuSelectedEventHandler();");
        code.Should().Contain("public Observable<R3.Unit> OnMainMenuSelected");
        code.Should().Contain("EmitSignal(nameof(MainMenuSelected))");
    }

    [Fact]
    public void GenerateCodeForPauseMenuLikeClass()
    {
        const string INPUT = """
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

        const string AUTO_NODE_STUB = """
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
                                        // placeholder
                                    }
                                    """;

        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(INPUT),
            CSharpSyntaxTree.ParseText(R3_STUB),
            CSharpSyntaxTree.ParseText(RX_SIGNAL_ATTRIBUTE_STUB),
            CSharpSyntaxTree.ParseText(AUTO_NODE_STUB)
        };

        var compilation = CreateCompilation(syntaxTrees, DefaultReferences());
        var result = RunGenerator(compilation);

        var generated = result.Results.SelectMany(r => r.GeneratedSources).Select(s => s.HintName);
        generated.Should().Contain(h => h.Contains("PauseMenu"), "PauseMenu should result in generated code");
    }

    #endregion
}