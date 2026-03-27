namespace Twia.StateMachine;

/// <summary>
/// Event arguments for the state change event, containing information about the state transition, such as the previous state, the new state, and the reason for the change.
/// </summary>
/// <typeparam name="TState">enum type of the state, as determined by the state machine this interface is implemented on.</typeparam>
public class StateChangedEventArgs<TState>
    where TState : struct, Enum
{
    /// <summary>
    /// The state that the state machine is transitioning from. May be null on entering the state machine for the first time, transitioning to the initial state.
    /// </summary>
    public TState? FromState { get; }

    /// <summary>
    /// The state that the state machine is transitioning to.
    /// </summary>
    public TState ToState { get; }

    /// <summary>
    /// Informational string with the reason for the state change.
    /// </summary>
    /// <remarks>
    /// Can contain a string similar to:
    /// <list type="bullet">
    /// <item>'<c>Initial</c>' Transitioning to the initial state of the state machine. </item>
    /// <item>'<c>Trigger: [trigger name]</c>' if the state change was triggered by a trigger. </item>
    /// <item>'<c>After: [timespan]</c>' if the state change was triggered bij an After timer expiring.</item>
    /// </list>
    /// </remarks>
    public string Reason { get; }

    /// <summary>
    /// Create a new instance of StateChangedEventArgs.
    /// </summary>
    /// <param name="fromState">The state that the state machine is transitioning from. May be null on entering the state machine for the first time, transitioning to the initial state.</param>
    /// <param name="toState">The state that the state machine is transitioning to.</param>
    /// <param name="reason">The reason for the state change.</param>
    public StateChangedEventArgs(TState? fromState, TState toState, string reason)
    {
        FromState = fromState;
        ToState = toState;
        Reason = reason;
    }
}