using System.Diagnostics;

namespace Twia.StateMachine;

/// <summary>
/// The initial state of a state machine.
/// </summary>
/// <remarks>
/// <para>
/// There can only be a single method marked with the <see cref="InitialStateAttribute"/> in a state machine.
/// </para>
/// <para>
/// The initial state is the state that the state machine will transition to when the <c>InitializeStateMachine()</c> method is called.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class InitialStateAttribute : StateAttribute;