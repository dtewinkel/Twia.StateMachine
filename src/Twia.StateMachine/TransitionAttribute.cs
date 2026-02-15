using System.Diagnostics;

namespace Twia.StateMachine;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class TransitionAttribute : Attribute
{
    public TransitionAttribute(string trigger, string targetState)
    {
        Trigger = trigger;
        TargetState = targetState;
    }

    public string Trigger { get; }

    public string TargetState { get; }

    public string? Condition { get; set; } = null;

    public string? Action { get; set; } = null;
}