---
Name: 'Observable, Actions, Conditions and multiple TransitionAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [Transition(nameof(Trigger2), nameof(State2), Condition = nameof(Condition1), Action = $"{nameof(Action1)}()")]
    [Transition(nameof(Trigger3), nameof(State3), Condition = nameof(Condition2), Action = $"{nameof(Action2)}()")]
    private partial void State1();

    [State]
    [Transition(nameof(Trigger1), nameof(State1), Condition = nameof(Condition2))]
    [Transition(nameof(Trigger3), nameof(State3), Action = $"{nameof(Action2)}()")]
    private partial void State2();

    [State]
    [Transition(nameof(Trigger1), nameof(State1))]
    [Transition(nameof(Trigger2), nameof(State2))]
    private partial void State3();

    [Trigger]
    public partial void Trigger1();

    [Trigger]
    public partial void Trigger2();

    [Trigger]
    public partial void Trigger3();

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
