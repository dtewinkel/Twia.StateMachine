---
Name: 'Observable TransitionAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [Transition(nameof(Trigger1), nameof(State1))]
    private partial void State1();

    [Trigger]
    public partial void Trigger1();
}
