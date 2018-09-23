using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    [SerializeField]
    private ResourcesManager resourcesManager;

    [SerializeField]
    private ScenarioManager scenarioManager;

    [SerializeField]
    private SequencerDriver driver;

    [SerializeField]
    private AudioListener listener;

    private SoundBlock actualBlock;

    SequencerOneshot activeSequencer;

    public void PlayScenario()
    {
        if (!driver.IsPlaying)
        {
            if (actualBlock == null)
            {
                PlayBlock(1, BEAT_TIMING.ON_1, null);
                PlayBlock(2, BEAT_TIMING.ON_1, null);
            }
        }

        driver.Play();
    }

    private void StopBlock(int blockId)
    {
        actualBlock = scenarioManager.GetSoundBlock(blockId);
        activeSequencer.Mute(true, 1.0f);
    }

    private void PlayBlock(int blockId, BEAT_TIMING timing, Action callbackAtSoundBeginning = null)
    {
        SequencerOneshot shot = GetFreeSequencer();

        if (!shot)
        {
            Debug.LogError("no free sequencer");
            return;
        }

        activeSequencer = shot;

        actualBlock = scenarioManager.GetSoundBlock(blockId);
        AudioClip clip = resourcesManager.GetResource(actualBlock.clipId).Clip;

        if (!clip)
        {
            Debug.LogError("no clip for " + actualBlock.clipId);
            return;
        }

        shot.sequence[(int)timing] = true;
        shot.SetAudioClip(clip);
        shot.SetLoop(actualBlock.isLooping);
        shot.OnAnyStep += () => { Debug.Log("onanystep"); };
        shot.OnBeat += () => { Debug.Log("onbeat"); };

        if (callbackAtSoundBeginning != null)
        {
            //shot.SetSoundBeginCallback(action);
        }

        if (actualBlock.nextBlock != null)
        {
            //shot.SetNextSoundCallback(NextSoundblock, BEAT_TIMING.ON_2);
        }
    }

    public void NextSoundblock(Action action)
    {
        if (actualBlock.nextBlock != null)
        {
            StopBlock(actualBlock.soundblockId);
            PlayBlock(actualBlock.nextBlock.soundblockId, BEAT_TIMING.ON_1, action);
        }
    }

    public void NextSoundblock()
    {
        if (actualBlock.nextBlock != null)
        {
            StopBlock(actualBlock.soundblockId);
            PlayBlock(actualBlock.nextBlock.soundblockId, BEAT_TIMING.ON_1, null);
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

            if (oneShot && !oneShot.IsBusy && oneShot != activeSequencer)
            {
                return oneShot;
            }
        }

        return null;
    }
}
