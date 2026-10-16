---
Name: 'Multiple OnEntryAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [OnEntry("Work()")]
    private partial void State1();

    [State]
    [OnEntry("Work()")]
    private partial void State2();

    private void Work()
    {
        // Do nothing
    }
}
