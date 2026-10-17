---
Name: 'Observable, Actions, Conditions and multiple InternalAfterAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalAfter("00:01:02", $"{nameof(Action1)}()", Condition = nameof(Condition1))]
    [InternalAfter("00:01:03", $"{nameof(Action2)}()", Condition = nameof(Condition2))]
    private partial void State1();

    [State]
    [InternalAfter("00:02:01", $"{nameof(Action1)}()", Condition = nameof(Condition2))]
    [InternalAfter("00:02:03", $"{nameof(Action2)}()")]
    private partial void State2();

    [State]
    [InternalAfter("00:03:01", $"{nameof(Action1)}()")]
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
