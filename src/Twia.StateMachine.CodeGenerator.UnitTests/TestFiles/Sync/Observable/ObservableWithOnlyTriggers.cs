---
Name: 'SMG0003 - Observable on a StateMachine with only triggers'
Output: Source
Diagnostics:
- SMG0003, 6 , 24, UnitTestStateMachine
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
internal partial class UnitTestStateMachine
{
    [Trigger]
    public partial void ButtonPressed();
}