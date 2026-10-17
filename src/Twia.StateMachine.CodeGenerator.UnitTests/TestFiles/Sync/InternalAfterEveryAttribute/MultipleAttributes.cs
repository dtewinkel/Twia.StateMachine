---
Name: 'Multiple InternalAfterEveryAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalAfterEvery("00:01:01", "Work1()")]
    [InternalAfterEvery("00:01:03", "Work2()")]
    private partial void State1();

    [State]
    [InternalAfterEvery("00:01:02", "Work1()")]
    private partial void State2();

    private void Work1()
    {
        // Do nothing
    }

    private void Work2()
    {
        // Do nothing
    }
}
