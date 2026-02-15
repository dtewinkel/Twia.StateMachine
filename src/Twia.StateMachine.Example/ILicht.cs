namespace Twia.StateMachine.Example;

public interface ILicht
{
    Task AanAsync(CancellationToken cancellationToken = default);

    void Aan();

    void Uit();

    void Dim(int percentage);
}