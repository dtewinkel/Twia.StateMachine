 namespace Twia.StateMachine.IntegrationTests;

[StateMachine]
public partial class SimpleStateMachine
{
    private int _field;

    [Trigger]
    public partial void Trigger1();

    [Trigger]
    public partial void Trigger2();

    [OnEntry($"{nameof(OnEntryState1)}()", Condition = $"{nameof(IsConditionTrue)}()")]
    [Transition(nameof(Trigger1), nameof(State2))]
    [State, InitialState]
    private partial void State1();

    [Transition(nameof(Trigger2), nameof(State1))]
    [Transition(nameof(Trigger1), nameof(State2))]
    [State]
    private partial void State2();

    private void OnEntryState1()
    {
        // Action to perform on entering State1
        _field++;
    }

    private bool IsConditionTrue()
    {
        // Condition to check
        return _field < 10;
    }
}

[StateMachine(StateAccessible = true, Observable = true)]
public partial class SimpleStateMachine2
{
    private int _counter;

    [InitialState]
    [Transition(nameof(Two), nameof(Three), Condition = "_counter < 500")]
    [OnEntry("_counter++", Condition = "_counter < 1000")]
    [OnExit("_counter++", Condition = "_counter > 100")]
    private partial void One();


    [State]
    [Transition(nameof(Two), nameof(One))]
    [TransitionAfter("1:00:00", nameof(Three))]
    private partial void Three();

    [Trigger]
    public partial void Two();
}
