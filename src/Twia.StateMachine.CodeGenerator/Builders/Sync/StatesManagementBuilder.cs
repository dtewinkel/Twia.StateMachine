using System.Transactions;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class StatesManagementBuilder : BuilderBase
{
    private readonly CSharpDocumentWriter _document;
    private readonly StatesBuilder _statesBuilder;
    private readonly TriggerMethodsProvider _triggerMethodsProvider;
    private readonly TriggersBuilder _triggersBuilder;
    private readonly AfterTransitionsBuilder _afterTransitionsBuilder;
    private readonly ObservableBuilder _observableBuilder;
    private readonly ClassCommonBuilder _classCommonBuilder;

    private readonly string _isInitializedFieldName;
    private readonly bool _stateIsAccessible;


    public StatesManagementBuilder(CSharpDocumentWriter document, StateMachineDeclaration declaration,
        StatesBuilder statesBuilder, TriggerMethodsProvider triggerMethodsProvider, TriggersBuilder triggersBuilder,
        AfterTransitionsBuilder afterTransitionsBuilder, ObservableBuilder observableBuilder,
        ClassCommonBuilder classCommonBuilder)
    {
        _document = document;
        _statesBuilder = statesBuilder;
        _triggerMethodsProvider = triggerMethodsProvider;
        _triggersBuilder = triggersBuilder;
        _afterTransitionsBuilder = afterTransitionsBuilder;
        _observableBuilder = observableBuilder;
        _classCommonBuilder = classCommonBuilder;
        _stateIsAccessible = declaration.StateAccessible;

        _isInitializedFieldName = classCommonBuilder.ToPrivateName("IsInitialized");
    }


    public override bool IsEnabled => _statesBuilder.IsEnabled || _triggersBuilder.IsEnabled;

    public override bool AddFields()
    {
        _document.WriteLine($"private bool {_isInitializedFieldName} = false;");
        return true;
    }

    public override bool AddPublicMethods()
    {
        AddInitializeMethod();
        return true;
    }

    private void AddInitializeMethod()
    {
        _document.WriteLine("/// <summary>");
        _document.WriteLine("/// Initialize the state machine before it is used.");
        _document.WriteLine("/// </summary>");
        _document.WriteLine("/// <remarks>");
        _document.WriteLine("/// The state machine must be initialized once (and only once) before any of the other generated methods and properties can be used.");
        _document.WriteLine("/// </remarks>");
        _document.WriteLine("/// <exception cref=\"global::System.InvalidOperationException\">");
        _document.WriteLine("/// InitializeStateMachine() can only be called once in the life of a state machine");
        _document.WriteLine("/// </exception>");
        _document.WriteLine("public void InitializeStateMachine()");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"if ({_isInitializedFieldName})");
        _document.WriteLineBlockOpen();
        _document.WriteLine("""throw new global::System.InvalidOperationException("'InitializeStateMachine()' can only be called once in the lifecycle of a state machine instance.");""");
        _document.WriteLineBlockClose();
        if (_statesBuilder.HasInitialState)
        {
            var initialStateName = _statesBuilder.InitialStateName;
            _document.WriteLineNoTabs();
            _document.WriteLine($"// Move to initial state '{initialStateName}'.");
            _document.WriteLine($"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{initialStateName}, \"Initial\");");
        }

        if (!_statesBuilder.HasStates)
        {
            _observableBuilder.InvokeOnStateChanged("null", _statesBuilder.UndefinedStateName, "\"Initial\"", addLine: true);
        }

        _document.WriteLineNoTabs();
        _document.WriteLine($"{_isInitializedFieldName} = true;");
        _document.WriteLineBlockClose();

    }

    public override bool AddPrivateMethods()
    {
        AddAssertIsInitialized();
        if (_statesBuilder.HasStates)
        {
            _document.WriteLineNoTabs();
            AddInvokeTriggerMethod();
            _document.WriteLineNoTabs();
            AddEnterStateMethod();
            _document.WriteLineNoTabs();
            return AddStateMethods();
        }

        return true;
    }

    private void AddEnterStateMethod()
    {
        const string stateParameterName = "state";
        const string reasonParameterName = "reason";
        _document.WriteLine($"private void {_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName} {stateParameterName}, string {reasonParameterName})");
        _document.WriteLineBlockOpen();
        _afterTransitionsBuilder.AddClearTimers();
        _observableBuilder.AddObserveStateChange(stateParameterName, reasonParameterName);
        _document.WriteLine($"{_statesBuilder.StateFieldName} = {stateParameterName};");
        _triggersBuilder.AddInvokeTrigger($"{_triggersBuilder.TriggerEnumTypeName}.{_triggersBuilder.EntryTriggerName}");
        _document.WriteLineBlockClose();
    }

    private void AddInvokeTriggerMethod()
    {
        _document.WriteLine($"private void {_triggersBuilder.InvokeTriggerMethodName}({_triggersBuilder.TriggerEnumTypeName} trigger)");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"{_triggersBuilder.LastTriggerFieldName} = trigger;");
        if (_statesBuilder.HasStates)
        {
            _document.WriteLineNoTabs();
            _document.WriteLine($"switch ({_statesBuilder.StateFieldName})");
            _document.WriteLineBlockOpen();
            var first = true;
            foreach (var stateName in _statesBuilder.StateNames)
            {
                first = _document.WriteSeparatorLine(first);
                _document.WriteLine($"case {_statesBuilder.StateFullTypeName}.{stateName}:");
                _document.Indent++;
                var state = _statesBuilder.GetState(stateName);
                if (state is { ReturnType: CommonTypeNames.Void, HasParameters: false })
                {
                    _document.WriteLine($"{stateName}();");
                    _document.WriteLine("break;");
                }
                else
                {
                    _document.WriteLine($"throw new global::System.InvalidOperationException(\"The state '{stateName}' is not usable because it does not have the right signature.\");");
                }
                _document.Indent--;
            }

            _document.WriteLineBlockClose();
        }

        _document.WriteLineBlockClose();
    }

    private void AddAssertIsInitialized()
    {
        _document.WriteLine($"private void {_classCommonBuilder.AssertIsInitializedMethodName}()");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"if (!{_isInitializedFieldName})");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"""throw new global::System.InvalidOperationException("The state machine is not initialized yet. Call 'InitializeStateMachine()' on the {_classCommonBuilder.StateMachineName} instance before using it.");""");
        _document.WriteLineBlockClose();
        _document.WriteLineBlockClose();
    }


    private bool AddStateMethods()
    {
        var firstStateMethod = true;
        var methodsAdded = false;

        foreach (var stateName in _statesBuilder.StateNames)
        {
            var state = _statesBuilder.GetState(stateName);
            if (state.IsPartial)
            {
                methodsAdded = true;
                var onEntryTransitions = state.Transitions
                    .Where(transition => transition.TransitionType == TransitionType.OnEntry).ToList();
                var hasEntryTransitions = onEntryTransitions.Count > 0;

                var onExitTransitions = state.Transitions
                    .Where(transition => transition.TransitionType == TransitionType.OnExit).ToList();
                var hasExitTransactions = onExitTransitions.Count > 0;

                var triggerTransitions = state.Transitions
                    .Where(transition => transition.TransitionType is TransitionType.OnTrigger or TransitionType.Internal)
                    .Where(transition => _triggerMethodsProvider.TriggerExists(transition.Trigger))
                    .ToList();
                var hasTriggerTransactions = triggerTransitions.Count > 0;

                var triggerlessTransitions = state.Transitions
                    .Where(transition => transition.TransitionType == TransitionType.Triggerless).ToList();
                var hasTriggerlessTransitions = triggerlessTransitions.Count > 0;

                var hasAfterTransitions = _afterTransitionsBuilder.HasAfterTransitions(stateName);

                firstStateMethod = _document.WriteSeparatorLine(firstStateMethod);
                var parameters = "";
                if (state.HasParameters)
                {
                    var parameterList = new List<string>();
                    foreach (var parameter in state.Parameters)
                    {
                        parameterList.Add($"{(string.IsNullOrWhiteSpace(parameter.Modifiers) ? "" : $"{parameter.Modifiers} ")}{parameter.ParameterType} {parameter.Name}");
                    }

                    parameters = string.Join(", ", parameterList);

                }
                _document.WriteLine($"{state.Modifiers} {state.ReturnType} {state.Name}({parameters})");
                _document.WriteLineBlockOpen();

                var onExitCall = hasExitTransactions ? $"OnExit{state.Name}();" : null;

                if (hasExitTransactions)
                {
                    _document.WriteLine($"void OnExit{state.Name}()");
                    _document.WriteLineBlockOpen();
                    foreach (var transitionDeclaration in onExitTransitions)
                    {
                        _document.WriteConditionAndAction(transitionDeclaration);
                    }

                    _document.WriteLineBlockClose();
                    _document.WriteLineNoTabs();
                }

                if (hasEntryTransitions || hasTriggerTransactions || hasAfterTransitions || hasTriggerlessTransitions)
                {
                    _document.WriteLine($"switch ({_triggersBuilder.LastTriggerFieldName})");
                    _document.WriteLineBlockOpen();
                    var first = true;

                    if (hasEntryTransitions || hasAfterTransitions || hasTriggerlessTransitions)
                    {
                        first = _document.WriteSeparatorLine(first);

                        _document.WriteLine(
                            $"case {_triggersBuilder.TriggerEnumTypeName}.{_triggersBuilder.EntryTriggerName}:");
                        _document.Indent++;

                        _afterTransitionsBuilder.AddStartTimers(stateName);
                        foreach (var transitionDeclaration in onEntryTransitions)
                        {
                            _document.WriteConditionAndAction(transitionDeclaration);
                        }

                        if (hasTriggerlessTransitions)
                        {
                            foreach (var transition in triggerlessTransitions.Where(transition => _statesBuilder.StateExists(transition.TargetState)))
                            {
                                _document.WriteConditionActionAndTransition(transition, onExitCall,
                                    (document, declaration) =>
                                    {
                                        document.WriteLine(
                                            $"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{declaration.TargetState}, \"Triggerless\");");
                                    }
                                );
                            }
                        }

                        _document.WriteLine("break;");
                        _document.Indent--;
                    }

                    if (hasTriggerTransactions)
                    {
                        var triggersGrouped = triggerTransitions
                                .GroupBy(trigger => trigger.Trigger);
                        foreach (var trigger in triggersGrouped)
                        {
                            first = _document.WriteSeparatorLine(first);
                            _document.WriteLine($"case {_triggersBuilder.TriggerEnumTypeName}.{trigger.Key}:");
                            _document.Indent++;
                            foreach (var transition in trigger.ToList())
                            {
                                switch (transition.TransitionType)
                                {
                                    case TransitionType.OnTrigger:
                                        if (_statesBuilder.StateExists(transition.TargetState))
                                        {
                                            _document.WriteConditionActionAndTransition(transition, onExitCall,
                                                (document, declaration) =>
                                                {
                                                    document.WriteLine(
                                                        $"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{declaration.TargetState}, \"Trigger: {trigger.Key}\");");
                                                }
                                            );
                                        }

                                        break;

                                    case TransitionType.Internal:
                                        _document.WriteConditionAndAction(transition);
                                        break;

                                    default:
                                        throw new InvalidOperationException(
                                            $"Unexpected transition type: {transition.TransitionType}");
                                }
                            }

                            _document.WriteLine("break;");
                            _document.Indent--;
                        }
                    }

                    if (hasAfterTransitions)
                    {
                        _afterTransitionsBuilder.AddTimerTransitions(stateName, first, onExitCall);
                    }

                    _document.WriteLineBlockClose();
                }

                if (state.ReturnType != CommonTypeNames.Void)
                {
                    _document.WriteLine($"return default({state.ReturnType});");
                }

                _document.WriteLineBlockClose();
            }
        }

        return methodsAdded;
    }

    /// <inheritdoc />
    public override bool AddPublicProperties()
    {
        if (!_stateIsAccessible)
        {
            return false;
        }

        _document.WriteLine("/// <summary>");
        _document.WriteLine("/// Readonly property to get the current state the state machine is in.");
        _document.WriteLine("/// </summary>");
        _document.WriteLine("/// <value>");
        _document.WriteLine("/// The current state of the state machine.");
        _document.WriteLine("/// </value>");
        _document.WriteLine("/// <exception cref=\"global::System.InvalidOperationException\">");
        _document.WriteLine("/// the state is not initialized yet");
        _document.WriteLine("/// </exception>");
        _document.WriteLine("public State CurrentState");
        _document.WriteLineBlockOpen();
        _document.WriteLine("get");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"{_classCommonBuilder.AssertIsInitializedMethodName}();");
        _document.WriteLineNoTabs();
        _document.WriteLine($"return {_statesBuilder.StateFieldName};");
        _document.WriteLineBlockClose();
        _document.WriteLineBlockClose();

        return true;
    }
}
