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
    [TransitionAfter("PT1M2S", nameof(State2), Condition = nameof(Condition1), Action = $"{nameof(Action1)}()")]
    [TransitionAfter("PT1M3S", nameof(State3), Condition = nameof(Condition2), Action = $"{nameof(Action2)}()")]
    private partial void State1();

    [State]
    [TransitionAfter("PT2M1S", nameof(State1), Condition = nameof(Condition2))]
    [TransitionAfter("PT2M3S", nameof(State3), Action = $"{nameof(Action2)}()")]
    private partial void State2();

    [State]
    [TransitionAfter("PT3M1S", nameof(State1))]
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
