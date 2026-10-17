---
Name: 'Observable, Conditions and multiple OnEntryAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [OnEntry($"{nameof(Action1)}()", Condition = nameof(Condition1))]
    [OnEntry($"{nameof(Action2)}()", Condition = nameof(Condition2))]
    private partial void State1();

    [State]
    [OnEntry($"{nameof(Action1)}()", Condition = nameof(Condition2))]
    [OnEntry($"{nameof(Action2)}()")]
    private partial void State2();

    [State]
    [OnEntry($"{nameof(Action1)}()")]
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
