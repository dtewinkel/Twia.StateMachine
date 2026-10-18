using System.Xml;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class AfterTransitionsBuilder : BuilderBase, ITriggersProvider
{
    private readonly CSharpDocumentWriter _document;
    private readonly ClassCommonBuilder _classCommonBuilder;
    private readonly StatesBuilder _statesBuilder;
    private readonly TriggersBuilder _triggersBuilder;
    private readonly Dictionary<string, List<TransitionDeclaration>> _transitions;
    private readonly string _startTimerMethodName;
    private readonly string _timersBackingFieldName;

    public AfterTransitionsBuilder(CSharpDocumentWriter document, StateMachineDeclaration declaration,
        ClassCommonBuilder classCommonBuilder, StatesBuilder statesBuilder, TriggersBuilder triggersBuilder)
    {
        _document = document;
        _classCommonBuilder = classCommonBuilder;
        _statesBuilder = statesBuilder;
        _triggersBuilder = triggersBuilder;

        _transitions = declaration.Methods
            .Where(method => method.IsState)
            .Select(state => new
            {
                state.Name,
                AfterTriggers = state.Transitions
                    .Where(transition => transition.TransitionType
                        is TransitionType.AfterDelay
                        or TransitionType.InternalAfter
                        or TransitionType.InternalAfterEvery)
                    .ToList()
            })
            .Where(x => x.AfterTriggers.Count > 0)
            .ToDictionary(x => x.Name, x => x.AfterTriggers);

        _timersBackingFieldName = _classCommonBuilder.ToPrivateName("Timers");
        _startTimerMethodName = _classCommonBuilder.ToPrivateName("StartTimer");

        IsEnabled = _transitions.Count > 0;
        TriggerNames = [.. _transitions.Values
            .SelectMany(transitions => transitions
                .Select(transition => ToFullAfterTriggerName(transition.Name)))];
    }

    public string[] TriggerNames { get; }

    public override bool IsEnabled { get; }

    private string ToFullAfterTriggerName(string name) => _classCommonBuilder.ToPrivateName(name);

    private string ToFullyQualifiedAfterTriggerEnum(string name) => $"{_triggersBuilder.TriggerEnumTypeName}.{ToFullAfterTriggerName(name)}";

    public override bool AddFields()
    {
        _document.WriteLine($"private global::System.Collections.Generic.List<global::System.Threading.Timer> {_timersBackingFieldName} = [];");
        return true;
    }
    public override bool AddPrivateMethods()
    {
        var timerCallback = _classCommonBuilder.ToPrivateName("TimerCallback");

        _document.WriteLine($"private void {_startTimerMethodName}(global::System.TimeSpan delay, global::System.TimeSpan period, {_triggersBuilder.TriggerEnumTypeName} trigger)");
        _document.Indent++;
        _document.WriteLine($"=> {_timersBackingFieldName}.Add(new global::System.Threading.Timer({timerCallback}, trigger, delay, period));");
        _document.Indent--;
        _document.WriteLineNoTabs();
        _document.WriteLine($"private void {timerCallback}(object? trigger)");
        _document.Indent++;
        _document.Write("=> ");
        _triggersBuilder.AddInvokeTrigger($"({_triggersBuilder.TriggerEnumTypeName})trigger!");
        _document.Indent--;
        return true;
    }

    public void AddClearTimers()
    {
        if (IsEnabled)
        {
            _document.WriteLine($"if ({_timersBackingFieldName}.Count > 0)");
            _document.WriteLineBlockOpen();
            _document.WriteLine($"foreach (var timer in {_timersBackingFieldName})");
            _document.WriteLineBlockOpen();
            _document.WriteLine("timer.Dispose();");
            _document.WriteLineBlockClose();
            _document.WriteLine($"{_timersBackingFieldName} = [];");
            _document.WriteLineBlockClose();
            _document.WriteLineNoTabs();
        }
    }

    public void AddStartTimers(string stateName)
    {
        if (_transitions.TryGetValue(stateName, out var transitions))
        {
            foreach (var transition in transitions)
            {
                if (!TryParsePeriod(transition.Trigger, out var timeSpan))
                {
                    // Ignore Transition with invalid After or Delay.
                    continue;
                }

                TimeSpan? initialDelay = null;
                if (transition.TransitionType == TransitionType.InternalAfterEvery 
                   && !TryParsePeriod(((InternalAfterEveryDeclaration)transition).InitialDelay, out initialDelay))
                {
                    // Ignore InternalAfterEvery with invalid InitialDelay.
                    continue;
                }

                switch (transition.TransitionType)
                {
                    case TransitionType.AfterDelay:
                    case TransitionType.InternalAfter:
                        _document.WriteLine(
                            $"{_startTimerMethodName}({ToConstructor(timeSpan)}, {ToConstructor(null)}, {ToFullyQualifiedAfterTriggerEnum(transition.Name)});");
                        break;

                    case TransitionType.InternalAfterEvery:
                        _document.WriteLine(
                            $"{_startTimerMethodName}({ToConstructor(initialDelay)}, {ToConstructor(timeSpan)}, {ToFullyQualifiedAfterTriggerEnum(transition.Name)});");
                        break;
                }
            }
        }
    }

    private static string ToConstructor(TimeSpan? timeSpan)
    {
        return timeSpan is null 
            ? "global::System.Threading.Timeout.InfiniteTimeSpan" 
            : $"new global::System.TimeSpan({timeSpan.Value.Days}, {timeSpan.Value.Hours}, {timeSpan.Value.Minutes}, {timeSpan.Value.Seconds}, {timeSpan.Value.Milliseconds})";
    }

    private static bool TryParsePeriod(string trigger, out TimeSpan? timeSpan)
    {
        if(string.IsNullOrWhiteSpace(trigger))
        {
            timeSpan = null;
            return false;
        }

        if (TimeSpan.TryParse(trigger, out var ts))
        {
            timeSpan = ts;
            return true;
        }

        try
        {
            timeSpan = XmlConvert.ToTimeSpan(trigger);
            return true;
        }
        catch (FormatException)
        {
            timeSpan = TimeSpan.Zero;
            return false;
        }
    }

    public void AddTimerTransitions(string stateName, bool first, string? onExitCall)
    {
        if (_transitions.TryGetValue(stateName, out var transitions))
        {
            foreach (var transition in transitions)
            {
                first = _document.WriteSeparatorLine(first);
                _document.WriteLine(
                    $"case {ToFullyQualifiedAfterTriggerEnum(transition.Name)}:");
                _document.Indent++;
                switch (transition.TransitionType)
                {
                    case TransitionType.AfterDelay:
                        if (_statesBuilder.StateExists(transition.TargetState))
                        {
                            _document.WriteConditionActionAndTransition(transition, onExitCall,
                                (document, declaration) =>
                                {
                                    document.WriteLine(
                                        $"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{declaration.TargetState}, \"After: {declaration.Trigger}\");");
                                }
                            );
                        }
                        break;

                    case TransitionType.InternalAfter:
                    case TransitionType.InternalAfterEvery:
                        _document.WriteConditionAndAction(transition);
                        break;
                }
                _document.WriteLine("break;");
                _document.Indent--;
            }
        }
    }

    public bool HasAfterTransitions(string stateName)
    {
        return _transitions.ContainsKey(stateName);
    }
}