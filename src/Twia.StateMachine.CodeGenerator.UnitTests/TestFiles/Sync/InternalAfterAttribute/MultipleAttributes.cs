---
Name: 'Multiple InternalAfterAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalAfter("00:01:01", "Work()")]
    private partial void State1();

    [State]
    [InternalAfter("00:01:02", "Work()")]
    private partial void State2();

    private void Work()
    {
        // Do nothing
    }
}
