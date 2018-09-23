using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionData
{
    public int id;
    public float value;

    public TransitionData(int id, float value)
    {
        this.id = id;
        this.value = value;
    }

    public TransitionData(TransitionData tr)
    {
        this.id = tr.id;
        this.value = tr.value;
    }
}

public class Resource
{
    private AudioClip clip;
    private string name;
    private int id;

    public int BPM;
    public int BPB;
    public float BeginLoop;
    public float EndLoop;

    public List<TransitionData> Transitions;
    public int nextTransitionId;

    public AudioClip Clip { get { return clip; } }
    public string Name { get { return name; } }
    public int Id { get { return id; } }

    public Resource(int id)
    {
        this.id = id;
    }

    public void Initialize(string name, int nbOfSamples, int channels, int frequency, float[] data, int BPM, int BPB, float BeginLoop, float EndLoop, List<TransitionData> _Transitions, int nextTransitionId)
    {
        clip = AudioClip.Create(name, nbOfSamples, channels, frequency, false);
        clip.SetData(data, 0);

        this.BPM = BPM;
        this.BPB = BPB;
        this.BeginLoop = BeginLoop;
        this.EndLoop = EndLoop;

        this.Transitions = _Transitions;

        this.nextTransitionId = nextTransitionId;

        this.name = name;
    }
}