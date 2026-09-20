---
Name: 'SMG0008 - InitialState Method Not Void'
Output: Source
Diagnostics:
- SMG0008, 9, 25, "State1"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    public partial bool State1();
}
