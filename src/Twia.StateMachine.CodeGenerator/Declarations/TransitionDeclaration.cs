namespace Twia.StateMachine.CodeGenerator.Declarations;

public abstract class TransitionDeclaration : IEquatable<TransitionDeclaration>
{
    public TransitionType TransitionType { get; protected set; }

    public string Name { get; protected set; } = null!;

    public string Trigger { get; protected set; } = null!;

    public string TargetState { get; protected set; } = null!;

    public string? Condition { get; protected set; }
    
    public string? Action { get; protected set; }

    public bool Equals(TransitionDeclaration? other)
    {
        return other is not null
               && Name == other.Name
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
            hash = hash * 31 + Name.GetHashCode();
            hash = hash * 31 + Trigger.GetHashCode();
            hash = hash * 31 + TargetState.GetHashCode();
            hash = hash * 31 + (Condition?.GetHashCode() ?? 0);
            hash = hash * 31 + (Action?.GetHashCode() ?? 0);
            return hash * 31 + TransitionType.GetHashCode();
        }
    }
}
