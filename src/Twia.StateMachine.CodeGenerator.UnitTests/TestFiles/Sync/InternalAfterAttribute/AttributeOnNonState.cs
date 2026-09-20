---
Name: 'SMG0005 - InternalAfterAttribute not on a state'
Output: Source
Diagnostics:
- SMG0005, 15, 17, Method1
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [Trigger]
    public partial void Trigger1();

    [InternalAfter("PT1H", "DoSomething()")]
    public bool Method1(string name)
    {
        return name == "Method1";
    }

    private void DoSomething()
    {
        // Do something.
    }
}
