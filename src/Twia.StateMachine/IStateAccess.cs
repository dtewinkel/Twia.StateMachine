namespace Twia.StateMachine;

/// <summary>
/// Access the state property of the state machine.
/// </summary>
/// <remarks>
/// This interface is implemented when the StateAccessible property of the StateMachineAttribute is set to true (which is the default).
/// </remarks>
/// <typeparam name="TState">enum type of the state, as determined by the state machine this interface is implemented on.</typeparam>
public interface IStateAccess<out TState>
    where TState : struct, Enum
{
    /// <summary>
    /// The current state of the state machine.
    /// </summary>
    /// <remarks>
    /// The type of the state is determined by the enum that is used to define the states of the state machine.
    /// </remarks>
    TState CurrentState { get; }
}