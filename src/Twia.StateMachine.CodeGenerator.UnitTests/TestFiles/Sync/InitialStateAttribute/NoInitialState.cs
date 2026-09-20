---
Name: 'SMG0003 - No InitialState'
Output: Source
Diagnostics:
- SMG0003, 6, 22, UnitTestStateMachine
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [State]
    private partial void State1();
}
