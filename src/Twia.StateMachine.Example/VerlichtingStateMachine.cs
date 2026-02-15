namespace Twia.StateMachine.Example;

[StateMachine]
public partial class VerlichtingStateMachine
{   
    private readonly ILicht _licht;
    private readonly ILichtSensor _lichtSensor;

    public VerlichtingStateMachine(ILicht licht, ILichtSensor lichtSensor)
    {
        _licht = licht;
        _lichtSensor = lichtSensor;
    }

    [OnEntry("_licht.Uit()")]
    [Transition(nameof(DetectieAanwezigheid), nameof(LichtAan), Condition = $"{nameof(IsHetDonker)}()", Action = "_licht.Dim(25)")]
    [Transition(nameof(DetectieAanwezigheid), nameof(LichtUit), Condition = $"!{nameof(IsHetDonker)}()")]
    [Transition(nameof(KnopBediend), nameof(HandmatigAan))]
    [InitialState]
    private partial void LichtUit();

    [OnEntry("_licht.Aan()")]
    [Transition(nameof(DetectieGeenAanwezigheid), nameof(AutoNaarUit))]
    [Transition(nameof(KnopBediend), nameof(HandNaarUit))]
    [State]
    private partial void LichtAan();

    [OnEntry("_licht.Aan()")]
    [Transition(nameof(DetectieGeenAanwezigheid), nameof(AutoHandNaarUit))]
    [Transition(nameof(KnopBediend), nameof(HandNaarUit))]
    [State]
    private partial void HandmatigAan();

    [OnEntry("_licht.Uit()")]
    [Transition(nameof(KnopBediend), nameof(HandmatigAan))]
    [TransitionAfter("PT00:00:05", nameof(LichtUit))]
    [State]private partial void HandNaarUit();

    [Transition(nameof(DetectieAanwezigheid), nameof(LichtAan), Condition = $"{nameof(IsHetDonker)}()")]
    [Transition(nameof(KnopBediend), nameof(HandNaarUit))]
    [TransitionAfter("PT00:00:05", nameof(LichtUit))]
    [State]
    private partial void AutoNaarUit();

    [Transition(nameof(DetectieAanwezigheid), nameof(HandmatigAan))]
    [Transition(nameof(KnopBediend), nameof(HandNaarUit))]
    [TransitionAfter("PT00:20:00", nameof(LichtUit))]
    [State]
    private partial void AutoHandNaarUit();

    [Trigger]
    public partial void KnopBediend();

    [Trigger]
    public partial void DetectieAanwezigheid();

    [Trigger]
    public partial void DetectieGeenAanwezigheid();

    public bool IsHetDonker()
        => _lichtSensor.SensorValue < 2.9m;
}