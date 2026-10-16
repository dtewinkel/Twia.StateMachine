---
Name: 'Observable InternalAfterEveryAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalAfterEvery("00:01:02", "Work()")]
    private partial void State1();

    private void Work()
    {
        // Do nothing
    }
}
