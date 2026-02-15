using System.Diagnostics;

namespace Twia.StateMachine;

/// <summary>
/// Indicate that the method is a state machine trigger and should have state machine logic generated.
/// </summary>
/// <remarks>
/// <para>
/// Code using the state machine should call this method to trigger a state transition.
/// </para>
/// <para>
/// The method must be partial, non-static, parameterless, and return void or <see cref="System.Threading.Tasks.Task"/>.
/// </para>
/// <para>
/// This attribute may not be combined with <see cref="StateAttribute"/> or <see cref="InitialStateAttribute"/>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class TriggerAttribute : Attribute;