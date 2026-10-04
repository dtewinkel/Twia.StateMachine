namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class TriggersBuilder : BuilderBase, ITriggersProvider
{
    private readonly CSharpDocumentWriter _document;
    private readonly TriggerMethodsProvider _triggerMethodsProvider;
    private readonly ClassCommonBuilder _classCommonBuilder;
    private readonly StatesBuilder _statesBuilder;
    private readonly bool _hasStates;
    private readonly bool _hasTriggers;

    public TriggersBuilder(CSharpDocumentWriter document, TriggerMethodsProvider triggerMethodsProvider, ClassCommonBuilder classCommonBuilder, StatesBuilder statesBuilder)
    {
        _document = document;
        _triggerMethodsProvider = triggerMethodsProvider;
        _classCommonBuilder = classCommonBuilder;
        _statesBuilder = statesBuilder;

        _hasTriggers = triggerMethodsProvider.TriggerNames.Any(name => !_statesBuilder.StateExists(name));
        _hasStates = statesBuilder.HasStates;

        UndefinedTrigger = classCommonBuilder.ToPrivateName("Undefined");
        TriggerEnumTypeName = classCommonBuilder.ToPrivateName("Trigger");
        LastTriggerFieldName = classCommonBuilder.ToPrivateName("LastTrigger");

        EntryTriggerName = classCommonBuilder.ToPrivateName("Entry");

        InvokeTriggerMethodName = classCommonBuilder.ToPrivateName("InvokeTrigger");

        TriggerNames = [EntryTriggerName];
        IsEnabled = triggerMethodsProvider.IsEnabled || _statesBuilder.HasStates;
    }

    public string UndefinedTrigger { get; }

    public string TriggerEnumTypeName { get; }

    public string EntryTriggerName { get; }

    public string InvokeTriggerMethodName { get; }

    public string LastTriggerFieldName { get; }

    public override bool IsEnabled { get; }

    public string[] TriggerNames { get; }

    public override bool AddFields()
    {
        if (_hasStates)
        {
            _document.WriteLine($"private {TriggerEnumTypeName} {LastTriggerFieldName} = {TriggerEnumTypeName}.{UndefinedTrigger};");
            return true;
        }

        return false;
    }


    public override bool AddPublicMethods()
    {
        return _hasTriggers && AddTriggerMethods();
    }

    private bool AddTriggerMethods()
    {
        var first = true;
        var triggersAdded = false;

        foreach (var trigger in _triggerMethodsProvider.Triggers)
        {
            var isAlsoState = _statesBuilder.StateExists(trigger.Name);

            if (!isAlsoState && trigger.IsPartial)
            {
                triggersAdded = true;
                first = _document.WriteSeparatorLine(first);

                var parameters = "";
                if (trigger.HasParameters)
                {
                    var parameterList = new List<string>();
                    foreach (var parameter in trigger.Parameters)
                    {
                        parameterList.Add($"{(string.IsNullOrWhiteSpace(parameter.Modifiers) ? "" : $"{parameter.Modifiers} ")}{parameter.ParameterType} {parameter.Name}");
                    }

                    parameters = string.Join(", ", parameterList);

                }

                _document.WriteLine($"{trigger.Modifiers} {trigger.ReturnType} {trigger.Name}({parameters})");
                _document.WriteLineBlockOpen();
                if (_hasStates)
                {
                    _classCommonBuilder.AddAssertIsInitialized();
                    AddInvokeTrigger($"{TriggerEnumTypeName}.{trigger.Name}");
                }

                if (trigger.ReturnType != CommonTypeNames.Void)
                {
                    _document.WriteLine($"return default({trigger.ReturnType});");
                }

                _document.WriteLineBlockClose();
            }
        }

        return triggersAdded;
    }

    public void AddInvokeTrigger(string trigger)
    {
        _document.WriteLine($"{InvokeTriggerMethodName}({trigger});");
    }
}