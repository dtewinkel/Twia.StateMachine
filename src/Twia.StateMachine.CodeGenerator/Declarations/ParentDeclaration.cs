using Microsoft.CodeAnalysis.CSharp;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public abstract class ParentDeclaration : Declaration
{
    protected ParentDeclaration(CSharpSyntaxNode node) : base(node)
    {
    }

    public abstract string? FullNamespaceName { get; }

    public abstract string HintNameForSource { get; }
}