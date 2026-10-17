---
Name: 'Multiple OnExitAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [OnExit("Work1()")]
    [OnExit("Work2()")]
    [Transition(nameof(Trigger1), nameof(State2))]
    private partial void State1();

    [State]
    [OnExit("Work1()")]
    [Transition(nameof(Trigger2), nameof(State1))]
    private partial void State2();

    [Trigger]
    public partial void Trigger1();

    [Trigger]
    public partial void Trigger2();

    private void Work1()
    {
        // Do nothing
    }

    private void Work2()
    {
        // Do nothing
    }
}
