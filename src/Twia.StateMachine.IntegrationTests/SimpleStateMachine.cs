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
