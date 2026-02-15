using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public class ClassDeclaration : Declaration, IEquatable<ClassDeclaration>
{
    public ClassDeclaration(ClassDeclarationSyntax node) : base(node)
    {
        Modifiers = node.Modifiers.ToString();
        Name = node.Identifier.Text + node.TypeParameterList?.ToFullString();
        IsPartial = node.Modifiers.Any(SyntaxKind.PartialKeyword);
        switch (node.Parent)
        {
            case ClassDeclarationSyntax classDeclaration:
                Parent = new ContainingClassDeclaration(classDeclaration);
                break;
            case NamespaceDeclarationSyntax namespaceDeclaration:
                Parent = new NamespaceDeclaration(namespaceDeclaration);
                break;
            case FileScopedNamespaceDeclarationSyntax namespaceDeclaration:
                Parent = new NamespaceDeclaration(namespaceDeclaration);
                break;
            default:
                return;
        }
    }
    
    public string Modifiers { get; }
    public string Name { get; }
    public bool IsPartial { get; }
    public ParentDeclaration? Parent { get; }

    public string? FullNamespaceName => Parent?.FullNamespaceName;

    public string HintNameForSource => $"{(Parent is not null ? $"{Parent.HintNameForSource}." : "")}{Name}";

    public virtual bool Equals(ClassDeclaration? other)
    {
        if (other is null)
        {
            return false;
        }

        return Modifiers == other.Modifiers
               && Name == other.Name
               && IsPartial == other.IsPartial
               && (ReferenceEquals(Parent, other.Parent) || (Parent?.Equals(other.Parent) ?? false));
    }

    public override bool Equals(object? other)
    {
        return other is ClassDeclaration otherClassDeclaration && Equals(otherClassDeclaration);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 31;
            hash = hash * 37 + Name.GetHashCode();
            hash = hash * 37 + Modifiers.GetHashCode();
            hash = hash * 37 + IsPartial.GetHashCode();
            return hash * 37 + (Parent?.GetHashCode() ?? 0);
        }
    }
}