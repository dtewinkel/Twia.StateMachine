using System.Diagnostics;

namespace Twia.StateMachine;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class TransitionAfterAttribute(
    string after,
    string targetState) : Attribute
{
    public string After { get; set; } = after;

    public string TargetState { get; set; } = targetState;

    public string? Condition { get; set; } = null;

    public string? Action { get; set; } = null;
}