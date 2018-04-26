using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource
{
    AudioClip clip;
    string name;
    int id;

    public int BPM;
    public int BPB;

    public AudioClip Clip { get { return clip; } }
    public string Name { get { return name; } }
    public int Id { get { return id; } }

    public Resource(int id)
    {
        this.id = id;
    }

    public void Initialize(string name, int nbOfSamples, int channels, int frequency, float[] data, int BPM, int BPB)
    {
        clip = AudioClip.Create(name, nbOfSamples, channels, frequency, false);
        clip.SetData(data, 0);

        this.BPM = BPM;
        this.BPB = BPB;
        this.name = name;
    }
}
