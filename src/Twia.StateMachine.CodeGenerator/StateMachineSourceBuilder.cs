using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator;

public class StateMachineSourceBuilder
{
    private string RandomIdInPrefix { get; } = Guid.NewGuid().ToString("N").Substring(0, 8);

    private readonly string _generatorVersion;
    private readonly string _generatorName;
    private string _stateMachineName = "<undefined>";
    private string PrivateMemberPrefix => $"__{_stateMachineName}_{RandomIdInPrefix}_";
    private string TriggerEnumTypeName => $"{PrivateMemberPrefix}Trigger";
    private string CurrentStateBackingFieldName => $"{PrivateMemberPrefix}CurrentStateBacking";

    private string _lastTriggerFieldName = "<undefined>";


    public StateMachineSourceBuilder()
    {
        var assembly = typeof(StateMachineIncrementalCodeGenerator).Assembly;
        var versionAttribute = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
            .FirstOrDefault();
        _generatorVersion = versionAttribute?.InformationalVersion ?? "Unknown";
        _generatorName = assembly.GetName().Name ?? "Unknown";
    }

    public void AddSource(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var states = declaration.Methods.Where(method => method.IsState).ToArray();
        var initialState = states.FirstOrDefault(state => state.IsInitial);
        var triggers = declaration.Methods.Where(method => method.IsTrigger).ToArray();
        _stateMachineName = declaration.Name;

        var nestingLevel = 0;
        var builder = new StringBuilder();
        using var writer = new StringWriter(builder, CultureInfo.InvariantCulture);
        using var document = new IndentedTextWriter(writer, "    ");

        GenerateHeader(document);
        document.WriteLineNoTabs();

        var namespaceName = declaration.FullNamespaceName;
        if (namespaceName is not null)
        {
            document.WriteLine($"namespace {namespaceName};");
            document.WriteLineNoTabs();
        }

        var parentStack = new Stack<ContainingClassDeclaration>();
        var parent = declaration.Parent;
        while (parent is ContainingClassDeclaration classParent)
        {
            parentStack.Push(classParent);
            parent = classParent.Parent;
        }

        while (parentStack.Count > 0)
        {
            var parentDeclaration = parentStack.Pop();
            document.WriteLine($"{parentDeclaration.Modifiers} class {parentDeclaration.Name}");
            document.WriteLineBlockOpen();
            nestingLevel++;
        }

        document.WriteLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{_generatorName}\", \"{_generatorVersion}\")]");
        document.WriteLine($"{declaration.Modifiers} class {declaration.Name}");
        document.WriteLineBlockOpen();

        CreateTriggersEnum(document, triggers, states);

        document.WriteLineNoTabs();
        CreateStatesEnum(document, states);

        document.WriteLineNoTabs();
        CreateCurrentStateProperty(document);

        document.WriteLineNoTabs();

        CreateInitializeMethod(document, initialState);
        WriteTriggerMethods(document, triggers, states);
        WriteStateMethods(document, states);
        
        document.WriteLineBlockClose();

        while (nestingLevel > 0)
        {
            document.WriteLineBlockClose();
            nestingLevel--;
        }

        Debug.Assert(document.Indent == 0);

        var hintName = $"{declaration.HintNameForSource}_StateMachine.g.cs";
        context.AddSource(hintName, SourceText.From(writer.ToString(), Encoding.UTF8));
    }

    private void CreateInitializeMethod(IndentedTextWriter document, MethodDeclaration? initialState)
    {
        document.WriteLine("/// <summary>");
        document.WriteLine("/// Initialize the state machine before it is used.");
        document.WriteLine("/// </summary>");
        document.WriteLine("/// <remarks>");
        document.WriteLine("/// The state machine must be initialize once (and only once) before any of the other generated methods and properties can be used.");
        document.WriteLine("/// </remarks>");
        document.WriteLine("/// <exception cref=\"global::System.InvalidOperationException\">");
        document.WriteLine("/// InitializeStateMachine() can only be called one in the life of a state machine");
        document.WriteLine("/// </exception>");
        document.WriteLine("public void InitializeStateMachine()");
        document.WriteLineBlockOpen();
        document.WriteLine($"if ({CurrentStateBackingFieldName} != 0)");
        document.WriteLineBlockOpen();
        document.WriteLine("""throw new global::System.InvalidOperationException("'InitializeStateMachine()' can only be called once in the lifecycle of an instance.");""");
        document.WriteLineBlockClose();
        if (initialState is not null)
        {
            document.WriteLineNoTabs();
            document.WriteLine($"// Move to initial state '{initialState.Name}'.");
            document.WriteLine($"{CurrentStateBackingFieldName} = State.{initialState.Name};");
            document.WriteLine($"{PrivateMemberPrefix}InvokeTrigger({TriggerEnumTypeName}.{PrivateMemberPrefix}Entry);");
        }
        document.WriteLineBlockClose();
        document.WriteLineNoTabs();

        document.WriteLine($"private void {PrivateMemberPrefix}AssertIsInitialized()");
        document.WriteLineBlockOpen();
        document.WriteLine($"if ({CurrentStateBackingFieldName} == 0)");
        document.WriteLineBlockOpen();
        document.WriteLine("""throw new global::System.InvalidOperationException("The state machine is not initialized yet. Call 'InitializeStateMachine()' before using it.");""");
        document.WriteLineBlockClose();
        document.WriteLineBlockClose();
    }

    private void GenerateHeader(IndentedTextWriter document)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        document.WriteLine("// <auto-generated/>");
        document.WriteLine($"// Generated by {_generatorName} version {_generatorVersion} at {timestamp} UTC.");
        document.WriteLine("#nullable enable");
    }

    private void CreateTriggersEnum(IndentedTextWriter document, MethodDeclaration[] triggers, MethodDeclaration[] states)
    {
        var afterStates = states
            .Where(state => state.Transitions.Any(transition => transition.TransitionType == TransitionType.AfterDelay))
            .GroupBy(state => state.Name).ToList();
        var hasAfterStates = afterStates.Count > 0;
        document.WriteLine($"private enum {TriggerEnumTypeName}");
        document.WriteLineBlockOpen();
        document.WriteLine("// Undefined value.");
        AddEnumMembers(document, [$"{PrivateMemberPrefix}Undefined"], moreToFollow: true, firstValue: 0);
        document.WriteLine("// User defined triggers.");
        AddEnumMembers(document, [.. triggers.Select(trigger => trigger.Name)], moreToFollow: true);
        document.WriteLine("// System defined triggers.");
        AddEnumMembers(document, [$"{PrivateMemberPrefix}Entry"], moreToFollow: hasAfterStates);
        if (hasAfterStates)
        {
            List<string> afterTriggerNames = [];
            foreach (var afterState in afterStates)
            {
                var index = 1;
                foreach (var _ in afterState.ToList())
                {
                    afterTriggerNames.Add($"{PrivateMemberPrefix}{afterState.Key}After{index}");
                    index++;
                }
            }
            AddEnumMembers(document, afterTriggerNames);
        }
        document.WriteLineBlockClose();

        document.WriteLineNoTabs();
        _lastTriggerFieldName = $"_current{TriggerEnumTypeName}";
        document.WriteLine($"private {TriggerEnumTypeName} {_lastTriggerFieldName} = {TriggerEnumTypeName}.{PrivateMemberPrefix}Undefined;");
    }

    private void CreateStatesEnum(IndentedTextWriter document, MethodDeclaration[] states)
    {
        document.WriteLine("/// <summary>");
        document.WriteLine("/// Enumeration of the states that the state machine can be in.");
        document.WriteLine("/// </summary>");
        document.WriteLine("/// <remarks>");
        document.WriteLine($"/// The names of the members of the <see cref=\"State\" /> enum are the names of the state methods of the <see cref=\"{_stateMachineName}\" /> class.");
        document.WriteLine("/// </remarks>");
        document.WriteLine("public enum State");
        document.WriteLineBlockOpen();
        AddEnumMembers(document, 
            [.. states.Select(state => state.Name)], firstValue: 1);
        document.WriteLineBlockClose();
    }

    private static void AddEnumMembers(IndentedTextWriter document, List<string> members, bool moreToFollow = false, int? firstValue = null)
    {
        var count = members.Count;
        var position = 1;
        foreach (var member in members)
        {
            document.Write($"{member}");
            if (firstValue.HasValue && position == 1)
            {
                document.Write($" = {firstValue.Value}");
            }
            document.WriteLine(position == count && !moreToFollow ? "" : ",");
            position++;
        }
    }

    private void CreateCurrentStateProperty(IndentedTextWriter document)
    {
        document.WriteLine($"private State {CurrentStateBackingFieldName} = 0;");
        document.WriteLineNoTabs();
        document.WriteLine("/// <summary>");
        document.WriteLine("/// Readonly property to get the current state the state machine is in.");
        document.WriteLine("/// </summary>");
        document.WriteLine("/// <value>");
        document.WriteLine("/// The current state of the state machine.");
        document.WriteLine("/// </value>");
        document.WriteLine("/// <exception cref=\"global::System.InvalidOperationException\">");
        document.WriteLine("/// the state is not initialized yet");
        document.WriteLine("/// </exception>");
        document.WriteLine("public State CurrentState");
        document.WriteLineBlockOpen();
        document.WriteLine("get");
        document.WriteLineBlockOpen();
        document.WriteLine($"{PrivateMemberPrefix}AssertIsInitialized();");
        document.WriteLineNoTabs();
        document.WriteLine($"return {CurrentStateBackingFieldName};");    
        document.WriteLineBlockClose();
        document.WriteLineNoTabs();
        document.WriteLine("private set");
        document.WriteLineBlockOpen();
        document.WriteLine($"{CurrentStateBackingFieldName} = value;");
        document.WriteLineBlockClose();
        document.WriteLineBlockClose();
    }

    private void WriteTriggerMethods(IndentedTextWriter document, MethodDeclaration[] triggers, MethodDeclaration[] states)
    {
        foreach (var trigger in triggers)
        {
            document.WriteLineNoTabs();
            document.WriteLine($"{trigger.Modifiers} {trigger.ReturnType} {trigger.Name}()");
            document.WriteLineBlockOpen();
            document.WriteLine($"{PrivateMemberPrefix}AssertIsInitialized();");
            document.WriteLineNoTabs();
            document.WriteLine($"{PrivateMemberPrefix}InvokeTrigger({TriggerEnumTypeName}.{trigger.Name});");
            document.WriteLineBlockClose();
        }

        document.WriteLineNoTabs();
        document.WriteLine($"private void {PrivateMemberPrefix}EnterState(State state)");
        document.WriteLineBlockOpen();
        document.WriteLine("CurrentState = state;");
        document.WriteLine($"{PrivateMemberPrefix}InvokeTrigger({TriggerEnumTypeName}.{PrivateMemberPrefix}Entry);");
        document.WriteLineBlockClose();

        document.WriteLineNoTabs();
        document.WriteLine($"private void {PrivateMemberPrefix}InvokeTrigger({TriggerEnumTypeName} trigger)");
        document.WriteLineBlockOpen();
        document.WriteLine($"{_lastTriggerFieldName} = trigger;");
        document.WriteLineNoTabs();
        document.WriteLine("switch (CurrentState)");
        document.WriteLineBlockOpen();
        var first = true;
        foreach (var state in states)
        {
            if (!first)
            {
                document.WriteLineNoTabs();
            }
            document.WriteLine($"case State.{state.Name}:");
            document.Indent++;
            document.WriteLine($"{state.Name}();");
            document.WriteLine("break;");
            document.Indent--;
            first = false;
        }
        document.WriteLineBlockClose();
        document.WriteLineBlockClose();
    }

    private void WriteStateMethods(IndentedTextWriter document, MethodDeclaration[] states)
    {
        foreach (var state in states)
        {
            var onEntryTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.OnEntry).ToList();
            var hasEntryTransitions = onEntryTransitions.Count > 0;

            var onExitTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.OnExit).ToList();
            var hasExitTransactions = onExitTransitions.Count > 0;

            var triggerTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.OnTrigger).ToList();
            var hasTriggerTransactions = triggerTransitions.Count > 0;

            document.WriteLineNoTabs();
            document.WriteLine($"{state.Modifiers} {state.ReturnType} {state.Name}()");
            document.WriteLineBlockOpen();

            var onExitCall = hasExitTransactions ? $"OnExit{state.Name}();" : null;

            if (hasExitTransactions)
            {
                document.WriteLine($"{state.ReturnType} OnExit{state.Name}()");
                document.WriteLineBlockOpen();
                foreach (var transitionDeclaration in onExitTransitions)
                {
                    WriteConditionAndAction(document, transitionDeclaration);
                }
                document.WriteLineBlockClose();
                document.WriteLineNoTabs();
            }

            if (hasEntryTransitions || hasTriggerTransactions)
            {
                document.WriteLine($"switch ({_lastTriggerFieldName})");
                document.WriteLineBlockOpen();
                var first = true;

                if (hasEntryTransitions)
                {
                    first = false;

                    document.WriteLine($"case {TriggerEnumTypeName}.{PrivateMemberPrefix}Entry:");
                    document.Indent++;
                    foreach (var transitionDeclaration in onEntryTransitions)
                    {
                        WriteConditionAndAction(document, transitionDeclaration);
                    }
                    document.WriteLine("break;");
                    document.Indent--;
                }

                if (hasTriggerTransactions)
                {
                    var triggersGrouped = triggerTransitions.GroupBy(trigger => trigger.Trigger);
                    foreach (var trigger in triggersGrouped)
                    {
                        if (!first)
                        {
                            document.WriteLineNoTabs();
                        }

                        document.WriteLine($"case {TriggerEnumTypeName}.{trigger.Key}:");
                        document.Indent++;
                        foreach (var transition in trigger.ToList())
                            WriteConditionActionAndTransition(document, transition, onExitCall);
                        document.WriteLine("break;");
                        document.Indent--;
                        first = false;
                    }
                }

                document.WriteLineBlockClose();
            }

            document.WriteLineBlockClose();
        }
    }

    private void WriteConditionActionAndTransition(IndentedTextWriter document, TransitionDeclaration transitionDeclaration, string? onExitCall)
    {
        var condition = transitionDeclaration.Condition;
        var hasCondition = !string.IsNullOrWhiteSpace(condition);
        var action = transitionDeclaration.Action;
        var hasAction = !string.IsNullOrWhiteSpace(action);

        if (hasCondition)
        {
            document.WriteLine($"if ({condition})");
            document.WriteLineBlockOpen();
        }

        if (!string.IsNullOrWhiteSpace(onExitCall))
        {
            document.WriteLine(onExitCall);
        }

        if (hasAction)
        {
            document.WriteLine($"{action};");
        }

        document.WriteLine($"{PrivateMemberPrefix}EnterState(State.{transitionDeclaration.TargetState});");

        if (hasCondition)
        {
            document.WriteLineBlockClose();
        }
    }

    private static void WriteConditionAndAction(IndentedTextWriter document, TransitionDeclaration transitionDeclaration)
    {
        var condition = transitionDeclaration.Condition;
        var hasCondition = !string.IsNullOrWhiteSpace(condition);
        var action = transitionDeclaration.Action;

        if (hasCondition)
        {
            hasCondition = true;
            document.WriteLine($"if ({condition})");
            document.WriteLineBlockOpen();
        }
        document.WriteLine($"{action};");
        if (hasCondition)
        {
            document.WriteLineBlockClose();
        }
    }
}