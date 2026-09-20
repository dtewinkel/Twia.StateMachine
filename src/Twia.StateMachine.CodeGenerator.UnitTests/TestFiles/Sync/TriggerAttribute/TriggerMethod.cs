---
Name: 'Single Trigger Method'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();
    
    [Trigger]
    private partial void Trigger1();
}
