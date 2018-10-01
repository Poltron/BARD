using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSoundHandler : MonoBehaviour
{
    [SerializeField]
    private ResourcesManager resourcesManager;

    [SerializeField]
    private ScenarioManager scenarioManager;

    [SerializeField]
    private GUIManager guiManager;

    [SerializeField]
    private AudioListener listener;

    [SerializeField]
    private List<AudioSource> audioSources;

    private SoundBlock activeSoundblock;
    private AudioSource activeAudiosource;
    private SoundBlock nextSoundBlock;
    private AudioSource nextAudiosource;
    
    private double nextTransitionTime;

    private bool isPlaying;
    public bool IsPlaying { get { return isPlaying; } }

    void Start()
    {
        audioSources = new List<AudioSource>(GetComponentsInChildren<AudioSource>());
	}

    private AudioSource GetFreeAudioSource()
    {
        foreach(AudioSource audioSource in audioSources)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        return null;
    }

    public void PlayScenario()
    {
        if (AudioListener.pause)
        {
            AudioListener.pause = false;
            return;
        }

        StopScenario();

        AudioSource audioSource = GetFreeAudioSource();

        if (!audioSource)
        {
            Debug.LogError("no free sequencer");
            return;
        }

        activeSoundblock = scenarioManager.GetSoundBlock(scenarioManager.FirstBlock.soundblockId);
        activeAudiosource = audioSource;

        AudioClip clip = resourcesManager.GetResource(activeSoundblock.clipId).Clip;

        if (!clip)
        {
            Debug.LogError("no clip found for " + activeSoundblock.clipId);
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
        isPlaying = true;

        PrepareNextBlock();
    }

    public void StopScenario()
    {
        activeSoundblock = null;
        activeAudiosource = null;
        nextSoundBlock = null;
        nextAudiosource = null;
        nextTransitionTime = double.MaxValue;

        isPlaying = false;

        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.Stop();
            audioSource.volume = 1;
            audioSource.clip = null;
        }
    }

    public void NextBlock()
    {
        if (activeSoundblock.nextBlock != null)
        {
            Debug.Log("next block");

            float timeLeft = activeAudiosource.clip.length - activeAudiosource.time;

            nextTransitionTime = AudioSettings.dspTime + timeLeft;

            activeAudiosource.SetScheduledEndTime(AudioSettings.dspTime + timeLeft);
            nextAudiosource.PlayScheduled(AudioSettings.dspTime + timeLeft);
        }
    }

    public void PrepareNextBlock()
    {
        if (activeSoundblock.nextBlock != null)
        {
            Debug.Log("prepare next block");

            nextAudiosource = GetFreeAudioSource();
            nextSoundBlock = scenarioManager.GetSoundBlock(activeSoundblock.nextBlock.soundblockId);

            nextAudiosource.loop = nextSoundBlock.isLooping;
            nextAudiosource.clip = resourcesManager.GetResource(nextSoundBlock.clipId).Clip;

            if (!activeSoundblock.isLooping)
            {
                NextBlock();
            }
        }
    }

    public void PauseScenario()
    {
        AudioListener.pause = true;
    }

    public float GetProgression()
    {
        return activeAudiosource.time / activeAudiosource.clip.length;
    }

    void Update()
    {
        if (AudioSettings.dspTime > nextTransitionTime && isPlaying)
        {
            Debug.Log("update");
            activeSoundblock = nextSoundBlock;
            activeAudiosource = nextAudiosource;

            guiManager.ToggleNextPhaseButton(activeSoundblock.isLooping);

            nextTransitionTime = double.MaxValue;

            PrepareNextBlock();
        }
	}
}
