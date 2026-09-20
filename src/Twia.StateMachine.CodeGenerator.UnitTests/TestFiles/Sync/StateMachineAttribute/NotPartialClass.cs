---
Name: 'SMG001 - StateMachine attribute on class that is not partial'
Output: None
Diagnostics:
- SMG0001, 6, 14, "UnitTestStateMachine"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public class UnitTestStateMachine
{
}
