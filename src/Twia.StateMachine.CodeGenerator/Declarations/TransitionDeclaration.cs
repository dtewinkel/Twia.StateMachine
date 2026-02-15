using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public class TransitionDeclaration : IEquatable<TransitionDeclaration>
{
    public TransitionType TransitionType { get; }

    public string Trigger { get; }
    
    public string TargetState { get; }
    
    public string? Condition { get; }
    
    public string? Action { get; }

    public TransitionDeclaration(TransitionType transitionType, AttributeData attributeData)
    {
        TransitionType = transitionType;

        switch (transitionType)
        {
            case TransitionType.OnEntry:
            case TransitionType.OnExit:
                Trigger = transitionType.ToString();
                TargetState = "self";
                Action = attributeData.ConstructorArguments[0].Value?.ToString();
                Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
                break;

            case TransitionType.AfterDelay:
            case TransitionType.OnTrigger:
                Trigger = attributeData.ConstructorArguments[0].Value?.ToString() ?? "";
                TargetState = attributeData.ConstructorArguments[1].Value?.ToString() ?? "";
                Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
                Action = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Action").Value.Value?.ToString();
                break;

            default:
                Trigger = "";
                TargetState = "";
                break;
        }
    }

    public bool Equals(TransitionDeclaration? other)
    {
        return other is not null
               && Trigger == other.Trigger
               && TargetState == other.TargetState
               && Condition == other.Condition
               && Action == other.Action
               && TransitionType == other.TransitionType;
    }

    public override bool Equals(object? other)
    {
        return other is TransitionDeclaration otherTransitionDeclaration
               && Equals(otherTransitionDeclaration);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 29;
            hash = hash * 31 + Trigger.GetHashCode();
            hash = hash * 31 + TargetState.GetHashCode();
            hash = hash * 31 + (Condition?.GetHashCode() ?? 0);
            hash = hash * 31 + (Action?.GetHashCode() ?? 0);
            return hash * 31 + TransitionType.GetHashCode();
        }
    }
}
