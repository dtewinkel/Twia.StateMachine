---
Name: 'Single OnExitAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [OnExit("Work()")]
    [Transition(nameof(Trigger1), nameof(State1))]
    private partial void State1();

    [Trigger]
    public partial void Trigger1();

    private void Work()
    {
        // Do nothing
    }
}
