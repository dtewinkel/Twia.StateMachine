---
Name: 'SMG0012 - Invalid Period for TransistionAfterAttribute'
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
    [TransitionAfter("a", "State1")]
    private partial void State1();
}
