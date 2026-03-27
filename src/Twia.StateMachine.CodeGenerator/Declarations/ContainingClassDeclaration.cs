using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public class ContainingClassDeclaration : ParentDeclaration, IEquatable<ContainingClassDeclaration>
{
    public ContainingClassDeclaration(ClassDeclarationSyntax classDeclaration) : base(classDeclaration)
    {
        ClassDeclaration = new ClassDeclaration(classDeclaration);
    }

    public ClassDeclaration ClassDeclaration { get; }

    public ParentDeclaration? Parent => ClassDeclaration.Parent;

    public override string? FullNamespaceName => Parent?.FullNamespaceName;

    public override string HintNameForSource => $"{(Parent is not null ? $"{Parent.HintNameForSource}." : "")}{ClassDeclaration.Name}";


    public bool Equals(ContainingClassDeclaration? other)
    {
        return other is not null
               && ClassDeclaration.Equals(other.ClassDeclaration);
    }

    public override bool Equals(object? other)
    {
        return other is ContainingClassDeclaration otherContainingClassDeclaration
               && Equals(otherContainingClassDeclaration);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return 571 + ClassDeclaration.GetHashCode();
        }
    }
}