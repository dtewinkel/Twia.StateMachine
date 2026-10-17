---
Name: 'Multiple InternalTransitionAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalTransition(nameof(Trigger1), "Work1()")]
    [InternalTransition(nameof(Trigger3), "Work2()")]
    private partial void State1();

    [State]
    [InternalTransition(nameof(Trigger2), "Work1()")]
    private partial void State2();

    [Trigger]
    public partial void Trigger1();

    [Trigger]
    public partial void Trigger2();

    [Trigger]
    public partial void Trigger3();

    private void Work1()
    {
        // Do nothing
    }

    private void Work2()
    {
        // Do nothing
    }
}
