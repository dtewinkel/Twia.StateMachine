---
Name: 'Observable, Actions, Conditions and multiple TriggerlessTransitionAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [TriggerlessTransition(nameof(State2), Condition = nameof(Condition1), Action = $"{nameof(Action1)}()")]
    [TriggerlessTransition(nameof(State3), Condition = nameof(Condition2), Action = $"{nameof(Action2)}()")]
    private partial void State1();

    [State]
    [TriggerlessTransition(nameof(State1), Condition = nameof(Condition2))]
    [TriggerlessTransition(nameof(State3), Action = $"{nameof(Action2)}()")]
    private partial void State2();

    [State]
    private partial void State3();

    public bool Condition1 => true;

    public bool Condition2 => false;

    private void Action1()
    {
        // do something;
    }

    private void Action2()
    {
        // do something;
    }
}
