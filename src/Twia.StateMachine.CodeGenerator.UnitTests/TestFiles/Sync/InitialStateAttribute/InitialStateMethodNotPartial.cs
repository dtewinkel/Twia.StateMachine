---
Name: 'SMG0007 - InitialState Method Not Partial'
Output: Source
Diagnostics:
- SMG0007, 9, 17, "State1"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    public void State1()
    {
    }
}
