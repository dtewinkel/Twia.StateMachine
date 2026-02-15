# Twia.StateMachine

A generator of state machines for C# applications.

## Overview

Generates state machines based on attributes applied to a class and methods of that class.

The following UML state machine diagram:

![Simple state machine](https://raw.githubusercontent.com/dtewinkel/Twia.StateMachine/refs/heads/main/documentation/media/generated/StateMachine.png)

Can be defined in code as:

```csharp
[StateMachine]
public partial class MyStateMachine
{
    private readonly IEngine _engine;

    public MyStateMachine(IEngine engine)
    {
        _engine = engine;
    }

    [InitialState]
    [Transition(nameof(Run), nameof(Running))]
    private partial void Stopped();

    [State]
    [OnEntry("_engine.Run()")]
    [OnExit("_engine.Stop()")]
    [Transition(nameof(Stop), nameof(Stopped))]
    [TransitionAfter("PT01:00:00", nameof(Stopped))]
    private partial void Running();

    [Trigger]
    public partial void Run();

    [Trigger]
    public partial void Stop();
}
```

The state machine will start in de Stopped state.

When the `Run` method is called, it will transition to the `Running` state, executing the `_engine.Run()` method on entry.

After one hour in the `Running` state, it will automatically transition back to the `Stopped` state, executing the `_engine.Stop()` method on entry if the engine is running.

When the `Stop` method is called, it will transition to the `Stopped` state, executing the `_engine.Stop()` method on entry if the engine is running.

To use the above state machine:

```csharp
var engine = new StrongEngine();
var stateMachine = new MyStateMachine(engine);
stateMachine.InitializeStateMachine(); // Initialize the state machine. Brings it to the initial state.

stateMachine.Run(); // Transitions to Running state.
stateMachine.Stop(); // Transitions to Stopped state.

...

if(stateMachine.CurrentState == MyStateMachine.State.Running)
{
    stateMachine.Stop();
}
```

## Attributes

### The state machine class

The state machine class must be marked with the `StateMachineAttribute` attribute.

The class must be declared as `partial` to allow the code generator to generate the implementation.

```csharp
[StateMachine]
public partial class MyStateMachine
{
}
```

A state machine class can be embedded in another class.

### Triggers

Methods that represent triggers must be marked with the `TriggerAttribute` attribute. These methods must be declared as `partial` and have no implementation.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [Trigger]
    public partial void Run();
}
```

### States

Methods that represent states must be marked with the `StateAttribute` attribute.
These methods must be declared as `partial` and have no implementation. Preferably, these methods should be private, as they should not be called by users of the state machine.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [State]
    private partial void Running();
}
```

Exactly one of the states must be marked with the `InitialStateAttribute` attribute to indicate the starting state of the state machine.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [InitialState]
    private partial void Stopped();
}
```

### Transitions

A transition is triggered by a given trigger and will bring the state machine to the target state.

A transition may have a guard condition that must evaluate to true for the transition to activate.

A transition may have an action that is executed when the transition executes.

Transitions are defined on the state the transition originates from, the source state. The target state of a transition may be another state, or it may transition back to the source state.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [State]
    [Transition(nameof(Stop), nameof(Stopped))]
    private partial void Running();
}
```

The example shows that, when in state `Running`, on reception of the trigger `Stop` the state should transition to the state `Stopped`.

:warning: The `Transition` attribute can only be used on methods that also have the `State` or `InitialState` attribute.

#### Guard conditions

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [State]
    [Transition(nameof(Stop), nameof(Stopped), Condition = "_engine.Speed > 1000")]
    private partial void Running();
}
```

The example shows that, when in state `Running`, on reception of the trigger `Stop`, and when the condition `_engine.Speed > 1000` is true, then the state should transition to the state `Stopped`.

#### Action on transition

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [State]
    [Transition(nameof(Stop), nameof(Stopped), Action = "_engine.Stop()")]
    private partial void Running();
}
```

The example shows that, when in state `Running`, on reception of the trigger `Stop`, the action `_engine.Stop()` will be executed, and the state will transition to the state `Stopped`.

### Entry and Exit actions

Actions can be define that should be executed on entry of a state, or on exit of a state.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [InitialState]
    [Transition(nameof(Run), nameof(Running))]
    private partial void Stopped();

    [State]
    [OnEntry("_engine.Run()")]
    [OnExit("_engine.Stop()")]
    [Transition(nameof(Stop), nameof(Stopped))]
    [TransitionAfter("PT01:00:00", nameof(Stopped))]
    private partial void Running();
 }
```

## Generated methods, types, and properties

In addition to the states and trigger, the following methods, types and properties are generated for each state machine class:

- method `void InitializeStateMachine()`. Initializes the state machine and brings it to the initial state.
- enum `State`, embedded in the state machine class. Contains all states of the state machine as enum members.
- Raad-only property `State CurrentState { get; }`. Returns the current state of the state machine.
