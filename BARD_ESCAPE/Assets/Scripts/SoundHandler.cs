using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    [SerializeField]
    private ScenarioManager scenarioManager;

    public List<AudioClip> clips;

    [SerializeField]
    private SequencerDriver driver;

    [SerializeField]
    private AudioListener listener;

    private SoundBlock actualBlock;

    public void PlayScenario()
    {
        if (!driver.IsPlaying)
        {
            if (actualBlock == null)
            {
                PlayBlock(scenarioManager.FirstBlock.soundblockId, BEAT_TIMING.ON_1, null);
            }

            driver.Play();
        }
    }

    private void PlayBlock(int blockId, BEAT_TIMING timing, Action callbackAtSoundBeginning = null)
    {
        SequencerOneshot shot = GetFreeSequencer();

        if (!shot)
            Debug.LogError("no free sequencer");

        actualBlock = scenarioManager.GetSoundBlock(blockId);
        AudioClip clip = GetClip(actualBlock.clip);

        if (!clip)
            Debug.LogError("no clip for " + scenarioManager.FirstBlock.clip);

        shot.sequence[(int)timing] = true;
        shot.SetAudioClip(clip);
        shot.SetLoop(actualBlock.isLooping);

        if (callbackAtSoundBeginning != null)
        {
            shot.SetSoundBeginCallback(action);
        }

        if (actualBlock.nextBlock != null)
        {
            shot.SetNextSoundCallback(NextSoundblock, BEAT_TIMING.ON_2);
        }
    }

    private void NextSoundblock(Action action)
    {
        if (actualBlock.nextBlock != null)
        {
            PlayBlock(actualBlock.nextBlock.soundblockId, BEAT_TIMING.ON_1, action);
        }
    }

    public void ResetScenario()
    {

    }

    public void PauseScenario()
    {
        driver.Pause(true);
    }

    public void StopScenario()
    {
        actualBlock = null;
        driver.Stop();
    }

    private SequencerOneshot GetFreeSequencer()
    {
        foreach (SequencerBase sequencer in driver.sequencers)
        {
            SequencerOneshot oneShot = sequencer as SequencerOneshot;

            if (oneShot && !oneShot.IsBusy)
            {
                return oneShot;
            }
        }

        return null;
    }

    public AudioClip GetClip(string name)
    {
        foreach (AudioClip clip in clips)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }

        return null;
    }

    public void ClearClips()
    {
        for (int i = 0; i < clips.Count; i++)
        {
            clips[i].UnloadAudioData();
        }

        clips.Clear();
    }

}
