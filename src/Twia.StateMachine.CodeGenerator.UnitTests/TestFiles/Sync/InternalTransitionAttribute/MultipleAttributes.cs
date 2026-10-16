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
    [InternalTransition(nameof(Trigger1), "Work()")]
    private partial void State1();

    [State]
    [InternalTransition(nameof(Trigger2), "Work()")]
    private partial void State2();

    [Trigger]
    public partial void Trigger1();

    [Trigger]
    public partial void Trigger2();

    private void Work()
    {
        // Do nothing
    }
}
