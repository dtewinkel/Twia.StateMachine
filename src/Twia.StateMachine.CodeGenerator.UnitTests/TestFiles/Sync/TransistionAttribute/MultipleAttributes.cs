---
Name: 'Multiple TransitionAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [Transition(nameof(Trigger1), nameof(State2))]
    private partial void State1();

    [State]
    [Transition(nameof(Trigger2), nameof(State1))]
    private partial void State2();

    [Trigger]
    public partial void Trigger1();

    [Trigger]
    public partial void Trigger2();
}
