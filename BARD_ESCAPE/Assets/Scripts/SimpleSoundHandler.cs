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

        guiManager.ToggleNextPhaseButton(activeSoundblock.isLooping);

        if (activeSoundblock.isLooping == false)
        {
            NextSoundblock();
        }
    }

    public void StopScenario()
    {
        activeSoundblock = null;

        foreach(AudioSource audioSource in audioSources)
        {
            audioSource.Stop();
            audioSource.volume = 1;
            audioSource.clip = null;
        }
    }

    public void NextSoundblock()
    {
        Debug.Log("next soundblock");

        if (activeSoundblock.nextBlock != null)
        {
            float timeLeft = activeAudiosource.clip.length - activeAudiosource.time;
            activeAudiosource.SetScheduledEndTime(AudioSettings.dspTime + timeLeft);

            AudioSource previousActiveAudioSource = activeAudiosource;

            activeAudiosource = GetFreeAudioSource();
            activeSoundblock = scenarioManager.GetSoundBlock(activeSoundblock.nextBlock.soundblockId);

            activeAudiosource.loop = activeSoundblock.isLooping;
            activeAudiosource.clip = resourcesManager.GetResource(activeSoundblock.clipId).Clip;
            activeAudiosource.PlayScheduled(AudioSettings.dspTime + timeLeft);

            guiManager.ToggleNextPhaseButton(activeSoundblock.isLooping);

            if (activeSoundblock.isLooping == false)
            {
                NextSoundblock();
            }

            Debug.Log(activeSoundblock.soundblockId + " is active");
        }
    }

    void Update()
    {
        
	}
}
