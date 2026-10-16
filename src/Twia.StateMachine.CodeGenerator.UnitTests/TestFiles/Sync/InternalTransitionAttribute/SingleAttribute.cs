---
Name: 'Single InternalTransitionAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalTransition(nameof(Trigger1), "Work()")]
    private partial void State1();

    [Trigger]
    public partial void Trigger1();

    private void Work()
    {
        // Do nothing
    }
}
