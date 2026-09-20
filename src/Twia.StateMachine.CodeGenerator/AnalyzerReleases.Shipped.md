; Shipped analyzer releases
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0.0

### New rules

Rule ID | Category | Severity | Notes
---|---|---|---
SMG0001 | Generator | Error | Class with StateMachineAttribute must be partial
SMG0002 | Generator | Error | Only One InitialStateAttribute is allowed per class
SMG0003 | Generator | Error | An InitialState must be defined
SMG0004 | Generator | Error | Method cannot be a trigger and a state at the same time
SMG0005 | Generator | Error | Method must be declared as a state to be able to use any of the transition attributes
SMG0006 | Generator | Error | Trigger can not have transition attributes
SMG0007 | Generator | Error | Method must be partial
SMG0008 | Generator | Error | Method must have void return
SMG0009 | Generator | Error | Method must have no parameters
SMG0010 | Generator | Error | Trigger must be defined
SMG0011 | Generator | Error | State must be defined
SMG0012 | Generator | Error | TimeSpan value must be valid

