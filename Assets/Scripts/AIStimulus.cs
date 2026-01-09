using System;
using UnityEngine;

public enum StimulusType
{
    Food,
    Stone,
    Firecracker
}

public struct AIStimulus
{
    public StimulusType Type;
    public Vector3 Position;

    public AIStimulus(StimulusType type, Vector3 position)
    {
        Type = type;
        Position = position;
    }
}
public static class AIStimulusDispatcher
{
    public static Action<AIStimulus> OnStimulusEmitted;

    public static void Emit(AIStimulus stimulus)
    {
        OnStimulusEmitted?.Invoke(stimulus);
    }
}
