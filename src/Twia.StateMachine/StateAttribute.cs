using System.Diagnostics;

namespace Twia.StateMachine;

/// <summary>
/// A state of a state machine.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class StateAttribute: Attribute;