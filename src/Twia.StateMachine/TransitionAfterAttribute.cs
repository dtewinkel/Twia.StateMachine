using System.Diagnostics;

namespace Twia.StateMachine;

/// <summary>
/// Define a transition from the state the attribute is applied to, to the state defined in <see cref="TargetState"/>
/// when the time specified in <see cref="After"/> has elapsed and the optional condition defined in <see cref="Condition"/> is met.
/// When the transition is triggered, the optional action defined in <see cref="Action"/> will be executed.
/// </summary>
/// <remarks>
/// The attribute must be applied to a method marked with the <see cref="StateAttribute"/> or <see cref="InitialStateAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class TransitionAfterAttribute(
    string after,
    string targetState) : Attribute
{
    /// <summary>
    /// The time to wait before the transition is triggered.
    /// This should be a valid TimeSpan parseable string.
    /// See <see href="https://learn.microsoft.com/en-us/dotnet/api/system.timespan.parse?view=net-10.0#system-timespan-parse(system-string)"/> for more information on the format of the After string.
    /// </summary>
    public string After { get; set; } = after;

    /// <summary>
    /// The state to transition to.
    /// This must be the name of a method marked with the <see cref="StateAttribute"/> or <see cref="InitialStateAttribute"/> in the same class as the state method this attribute is applied to.
    /// </summary>
    public string TargetState { get; set; } = targetState;

    /// <summary>
    /// The condition that must be met for the transition to occur. This should be a valid C# expression that can be evaluated at runtime.
    /// The expression can reference any method or property of the state machine class, but it cannot reference any parameters of the trigger method.
    /// </summary>
    public string? Condition { get; set; } = null;

    /// <summary>
    /// The action to execute when the transition is triggered. This should be a valid C# statement or block of statements that can be executed at runtime.
    /// </summary>
    public string? Action { get; set; } = null;
}