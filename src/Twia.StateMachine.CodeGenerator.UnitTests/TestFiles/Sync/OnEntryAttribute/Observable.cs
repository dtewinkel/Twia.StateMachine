---
Name: 'Observable OnEntryAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [OnEntry("Work()")]
    private partial void State1();

    private void Work()
    {
        // Do nothing
    }
}
