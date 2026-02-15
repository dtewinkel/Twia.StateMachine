using System.Diagnostics;

namespace Twia.StateMachine;

/// <summary>
/// Defines an action to perform when entering a state.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="OnEntryAttribute"/> must only be used on <b>methods</b> that are marked with the <see cref="StateAttribute"/> or the <see cref="InitialStateAttribute"/>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class OnEntryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnEntryAttribute"/> class.
    /// </summary>
    /// <param name="action">The action to execute on entering a state. Must be valid code in the state machine's context.See <see cref="Action"/>.</param>
    public OnEntryAttribute(string action)
    {
        Action = action;
    }

    /// <summary>
    /// The action to execute on entering the state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see cref="Condition"/> is set, then the action will only be executed if the condition evaluates to true.
    /// </para>
    /// <para>
    /// The text in the action must be valid source code in the state machine's context that contains a valid expression, without a terminating <c>;</c>. For example : <c>someVariable = 5</c> or <c>SetReady()</c>.
    /// </para>
    /// </remarks>
    public string Action { get; }

    /// <summary>
    /// The optional condition that must be met for the action to be executed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the condition is set, then the action, as set in <see cref="Action"/> will only be executed if the condition evaluates to true.
    /// </para>
    /// <para>
    /// The text in the condition must be valid source code in the state machine's context that contains an expression that evaluates to a boolean value. For example : <c>someVariable == 5</c> or <c>IsReady()</c>.
    /// </para>
    /// </remarks>
    public string? Condition { get; set; } = null;
}