---
Name: 'Observable, Conditions and multiple OnExitAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [OnExit($"{nameof(Action1)}()", Condition = nameof(Condition1))]
    [OnExit($"{nameof(Action2)}()", Condition = nameof(Condition2))]
    [Transition(nameof(Trigger1), nameof(State2))]
    private partial void State1();

    [State]
    [OnExit($"{nameof(Action1)}()", Condition = nameof(Condition2))]
    [OnExit($"{nameof(Action2)}()")]
    [Transition(nameof(Trigger1), nameof(State3))]
    private partial void State2();

    [State]
    [OnExit($"{nameof(Action1)}()")]
    [Transition(nameof(Trigger1), nameof(State1))]
    private partial void State3();

    [Trigger]
    public partial void Trigger1();

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
