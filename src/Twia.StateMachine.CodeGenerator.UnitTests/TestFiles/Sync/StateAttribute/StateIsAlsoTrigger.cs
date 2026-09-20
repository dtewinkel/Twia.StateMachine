---
Name: 'SMG0004 - State cannot be a trigger'
Output: Source
Diagnostics:
- SMG0004, 13, 25, State2 
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [State]
    [Trigger]
    public partial void State2();

}
