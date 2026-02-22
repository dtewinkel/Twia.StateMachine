using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public sealed class MethodDeclaration : Declaration, IEquatable<MethodDeclaration>
{
    public MethodDeclaration(MethodDeclarationSyntax node, IList<AttributeData> attributes) : base(node)
    {
        Name = node.Identifier.ToString();
        Modifiers = node.Modifiers.ToString();
        ReturnType = node.ReturnType.ToString();

        IsPartial = node.Modifiers.Any(SyntaxKind.PartialKeyword);
        IsState = attributes.Any(attribute => attribute.GetFullName() == StateMachineAttributeNames.StateAttributeName 
                                              || attribute.GetFullName() == StateMachineAttributeNames.InitialStateAttributeName);
        IsTrigger = attributes.Any(attribute => attribute.GetFullName() == StateMachineAttributeNames.TriggerAttributeName);
        IsInitial = attributes.Any(attribute => attribute.GetFullName() == StateMachineAttributeNames.InitialStateAttributeName);

        foreach (var attributeData in attributes)
        {
            switch (attributeData.GetFullName())
            {
                case StateMachineAttributeNames.OnEntryAttributeName:
                    Transitions.Add(new OnEntryTransitionDeclaration(attributeData));
                    break;

                case StateMachineAttributeNames.OnExitAttributeName:
                    Transitions.Add(new OnExitTransitionDeclaration(attributeData));
                    break;

                case StateMachineAttributeNames.TransitionAttributeName:
                    Transitions.Add(new OnTriggerTransitionDeclaration(attributeData));
                    break;

                case StateMachineAttributeNames.TransitionAfterAttributeName:
                    Transitions.Add(new AfterDelayTransitionDeclaration(Name, attributeData));
                    break;
            }
        }

        foreach (var parameter in node.ParameterList.Parameters)
        {
            Parameters.Add(parameter.ToString());
        }
    }

    public string Name { get; }

    public string Modifiers { get; }

    public string ReturnType { get; }

    public bool IsPartial { get; }
    
    public bool IsState { get; }

    public bool IsTrigger { get; }

    public bool IsInitial { get; }

    public List<string> Parameters { get; } = [];

    public List<TransitionDeclaration> Transitions { get; } = [];

    public bool Equals(MethodDeclaration? other)
    {
        if (other is null)
        {
            return false;
        }

        return Modifiers == other.Modifiers
               && Name == other.Name
               && ReturnType == other.ReturnType
               && IsPartial == other.IsPartial
               && IsState == other.IsState
               && IsTrigger == other.IsTrigger
               && IsInitial == other.IsInitial
               && Parameters.SequenceEqual(other.Parameters)
               && Transitions.SequenceEqual(other.Transitions);
    }

    public override bool Equals(object? other)
    {
        return other is MethodDeclaration otherMethodDeclaration && Equals(otherMethodDeclaration);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 23;
            hash = hash * 37 + Name.GetHashCode();
            hash = hash * 37 + Modifiers.GetHashCode();
            hash = hash * 37 + ReturnType.GetHashCode();
            hash = hash * 37 + IsInitial.GetHashCode();
            hash = hash * 37 + IsState.GetHashCode();
            hash = hash * 37 + IsPartial.GetHashCode();
            hash = Parameters.Aggregate(hash, (current, parameter) => current * 37 + parameter.GetHashCode());
            hash = Transitions.Aggregate(hash, (current, transition) => current * 37 + transition.GetHashCode());
            return hash * 37 + IsTrigger.GetHashCode();
        }
    }
}