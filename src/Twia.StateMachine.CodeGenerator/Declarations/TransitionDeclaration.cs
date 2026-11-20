using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public abstract partial record TransitionDeclaration
{
    public TransitionType TransitionType { get; protected set; }

    public string Name { get; protected set; } = null!;

    public string Trigger { get; protected set; } = null!;

    public string TargetState { get; protected set; } = null!;

    public string? Condition { get; protected set; }

    public string? Action { get; protected set; }

    public SnippetSourceLocation? ConditionSource { get; private set; }

    public SnippetSourceLocation? ActionSource { get; private set; }

    public void SetSourceLocations(AttributeData attributeData)
    {
        if (attributeData.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attribute)
        {
            return;
        }

        var position = 0;
        foreach (var argument in attribute.ArgumentList?.Arguments ?? default)
        {
            var name = argument.NameEquals?.Name.Identifier.ValueText
                       ?? argument.NameColon?.Name.Identifier.ValueText;
            if (name is null && attributeData.AttributeConstructor is { } constructor
                             && position < constructor.Parameters.Length)
            {
                name = constructor.Parameters[position].Name;
            }

            if (argument.NameEquals is null)
            {
                position++;
            }

            var span = argument.Expression.GetLocation().GetMappedLineSpan();
            var source = new SnippetSourceLocation(span.Path, span.StartLinePosition.Line + 1);
            if (name is "Condition")
            {
                ConditionSource = source;
            }
            else if (name is "Action" or "action")
            {
                ActionSource = source;
            }
        }
    }
}

public sealed record SnippetSourceLocation(string Path, int Line);
