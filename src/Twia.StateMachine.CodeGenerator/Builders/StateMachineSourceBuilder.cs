using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Text;
using Twia.StateMachine.CodeGenerator.Builders.Sync;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders;

public static class StateMachineSourceBuilder
{
    public static void AddSource(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        if (!declaration.CanBeGenerated)
        {
            return;
        }

        try
        {
            using var document = new CSharpDocumentWriter();

            var triggerMethodsProvider = new TriggerMethodsProvider(declaration);
            var classCommonBuilder = new ClassCommonBuilder(document, declaration);
            var statesBuilder = new StatesBuilder(document, declaration, classCommonBuilder);
            var triggersBuilder = new TriggersBuilder(document, triggerMethodsProvider, classCommonBuilder, statesBuilder);
            var afterTransitionsBuilder = new AfterTransitionsBuilder(document, declaration, classCommonBuilder, statesBuilder, triggersBuilder);

            var triggerProviders = new List<ITriggersProvider> { triggerMethodsProvider, triggersBuilder, afterTransitionsBuilder }
                .Where(provider => provider.IsEnabled)
                .ToList();

            var triggersEnumBuilder = new TriggersEnumBuilder(document, triggersBuilder, triggerProviders, statesBuilder);
            var observableBuilder = new ObservableBuilder(document, declaration, classCommonBuilder, statesBuilder);
            var statesManagementBuilder = new StatesManagementBuilder(document, declaration, statesBuilder, triggerMethodsProvider, triggersBuilder, afterTransitionsBuilder, observableBuilder, classCommonBuilder);

            var builders = new List<BuilderBase>
            {
                statesBuilder, statesManagementBuilder, triggersBuilder, triggersEnumBuilder, afterTransitionsBuilder, observableBuilder
            }
                .Where(builder => builder.IsEnabled)
                .ToList();

            classCommonBuilder.StartClass();

            var codeAdded = GenerateAll(builders, document, false, sourceBuilder => sourceBuilder.AddTypes());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddConstants());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddFields());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddEvents());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddPublicProperties());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddPublicMethods());
            _ = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddPrivateMethods());

            classCommonBuilder.EndClass();

            Debug.Assert(document.Indent == 0);

            var hintName = $"{declaration.HintNameForSource}_StateMachine.g.cs";
            context.AddSource(hintName, SourceText.From(document.InnerWriter.ToString() ?? "<Geen>", Encoding.UTF8));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }

    private static bool GenerateAll(List<BuilderBase> builders, CSharpDocumentWriter document, bool codeAdded, Func<BuilderBase, bool> builderAction)
    {
        foreach (var sourceBuilder in builders.Where(sourceBuilder => sourceBuilder.IsEnabled))
        {
            if (codeAdded)
            {
                document.WriteLineNoTabs();
            }

            codeAdded = builderAction(sourceBuilder);
        }

        return codeAdded;
    }
}