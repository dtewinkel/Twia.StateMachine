---
Name: 'SMG0004 - InitialState cannot be a trigger'
Output: Source
Diagnostics:
- SMG0004, 10, 25, "State1" 
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [Trigger]
    public partial void State1();
}
