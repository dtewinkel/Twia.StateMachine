namespace Twia.StateMachine;

/// <summary>
/// Interface with events related to the state machine to implement observability of the state machine.
/// This allows external code to subscribe to state changes and react accordingly.
/// </summary>
/// <remarks>
/// This interface is implemented when the Observable property of the StateMachineAttribute is set to <see langword="true"/>.
/// </remarks>
/// <typeparam name="TState">enum type of the state, as determined by the state machine this interface is implemented on.</typeparam>
public interface IStateMachineEvents<TState>
    where TState : struct, Enum
{
    /// <summary>
    /// Occurs when the state of the object changes, allowing subscribers to respond to state transitions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This event is raised whenever a state change occurs. This may a state change from and to the same state.
    /// Subscribers should handle this event to perform actions in response to state transitions.
    /// </para>
    /// <para>
    /// Ensure that event handlers are properly detached when no longer needed to prevent memory leaks.
    /// </para>
    /// <para>
    /// Internal state changes that do not result in a state transition (e.g., internal variable updates) will not trigger this event.
    /// </para>
    /// </remarks>
    event EventHandler<StateChangedEventArgs<TState>>? OnStateChanged;
}