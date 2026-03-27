namespace Twia.StateMachine.CodeGenerator.Builders;

internal interface ITriggersProvider
{
    bool IsEnabled { get; }

    string[] GetTriggerNames();
}