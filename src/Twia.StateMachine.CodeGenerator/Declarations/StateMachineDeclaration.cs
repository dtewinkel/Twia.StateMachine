using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public sealed class StateMachineDeclaration : ClassDeclaration, IEquatable<StateMachineDeclaration>
{
    public StateMachineDeclaration(ClassDeclarationSyntax node, INamedTypeSymbol symbol) : base(node)
    {
        foreach (var member in symbol.GetMembers())
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }
            var attributes = method
                .GetAttributes()
                .Where(attribute => StateMachineAttributeNames.AllMethodAttributeNames.Contains(attribute.GetFullName()))
                .ToList();
            if (attributes.Count == 0)
            {
                continue;
            }
            var methodNode = node.Members
                .First(nodeMember => nodeMember is MethodDeclarationSyntax methodDeclaration
                                     && methodDeclaration.Identifier.ToString() == method.Name);
            Methods.Add(new MethodDeclaration((MethodDeclarationSyntax)methodNode, attributes));
        }
    }

    public List<MethodDeclaration> Methods { get; } = [];

    public bool Equals(StateMachineDeclaration? other)
    {
        return base.Equals(other) 
               && Methods.SequenceEqual(other.Methods);
    }

    public override bool Equals(object? other)
    {
        return other is StateMachineDeclaration otherStateMachineDeclaration 
               && Equals(otherStateMachineDeclaration);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = base.GetHashCode();

            if (Methods.Count > 0)
            {
                hash = hash * 31 + Methods.Count;
                hash = Methods.Aggregate(hash, (current, method) => current * 31 + method.GetHashCode());
            }

            return hash;
        }
    }
}