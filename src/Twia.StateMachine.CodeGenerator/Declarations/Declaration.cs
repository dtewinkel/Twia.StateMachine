using Microsoft.CodeAnalysis.CSharp;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public abstract class Declaration
{
    protected Declaration(CSharpSyntaxNode node)
    {
        Node = node;
    }

    public CSharpSyntaxNode Node { get;  }
}