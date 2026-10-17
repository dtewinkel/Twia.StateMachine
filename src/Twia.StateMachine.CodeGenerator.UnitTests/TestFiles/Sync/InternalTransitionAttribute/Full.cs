---
Name: 'Observable, Actions, Conditions and multiple InternalTransitionAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalTransition(nameof(Trigger1), $"{nameof(Action1)}()", Condition = nameof(Condition1))]
    [InternalTransition(nameof(Trigger2), $"{nameof(Action2)}()", Condition = nameof(Condition2))]
    private partial void State1();

    [State]
    [InternalTransition(nameof(Trigger1), $"{nameof(Action1)}()", Condition = nameof(Condition2))]
    [InternalTransition(nameof(Trigger2), $"{nameof(Action2)}()")]
    private partial void State2();

    [State]
    [InternalTransition(nameof(Trigger1), $"{nameof(Action1)}()")]
    private partial void State3();

    [Trigger]
    public partial void Trigger1();

    [Trigger]
    public partial void Trigger2();

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
