---
Name: 'SMG0012 - Invalid Period for InternalAfterAttribute'
Output: Source
Diagnostics:
- SMG0012, 10, 26, "a", "State1"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalAfter("a", "Work()")]
    private partial void State1();

    private void Work()
    {
        // Do nothing
    }
}
