using System.Diagnostics;

namespace Twia.StateMachine;

/// <summary>
/// Indicate that the class is a state machine and should have state machine logic generated.
/// </summary>
/// <remarks>
/// The target type must be a class and be marked as partial. 
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class StateMachineAttribute : Attribute;