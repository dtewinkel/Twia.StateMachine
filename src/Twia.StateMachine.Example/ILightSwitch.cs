namespace Twia.StateMachine.Example;

public interface ILightSwitch
{
    Task ToOnAsync(CancellationToken cancellationToken = default);

    void ToOn();

    void ToOff();

    void Dim(int percentage);
}