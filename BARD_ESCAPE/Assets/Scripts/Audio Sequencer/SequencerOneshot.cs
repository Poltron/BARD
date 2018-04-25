using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

enum BEAT_TIMING : int
{
    ON_1 = 0,
    ON_2 = 1,
    ON_3 = 2,
    ON_4 = 3
}

enum TRACK_TIMING : int
{
    ON_NOTIF = 0,
    ON_TRACK_END = 1
}

[RequireComponent(typeof(AudioSource))]
internal class SequencerOneshot : SequencerBase
{
    #region Enumerations

    [Flags]
    public enum FadeTarget
    {
        Play = (1 << 0),
        Stop = (1 << 1),
        Mute = (1 << 2),
        UnMute = (1 << 3),
        Pause = (1 << 4),
        UnPause = (1 << 5)
    }

    #endregion

    #region Events and Delegates
    /// <summary>
    /// Event to be fired on non-empty steps.
    /// </summary>
    public Action OnBeat;
    /// <summary>
    /// Event to be fired on every step.
    /// </summary>
    public Action OnAnyStep;
    #endregion

    #region Variables
    /// <summary>
    /// Audio clip to be played by this sequencer.
    /// </summary>
    public AudioClip clip;
    /// <summary>
    /// Low signature.
    /// </summary>
    public int signatureLo = 4;
    /// <summary>
    /// Sequence of steps.
    /// True = Play
    /// False = Silent
    /// </summary>
    public bool[] sequence;
    /// <summary>
    /// Fade in duration from muted to unmuted.
    /// </summary>
    [Range(0, 60)]
    public float fadeInDuration;
    /// <summary>
    /// Fade in duration from unmuted to muted.
    /// </summary>
    [Range(0, 60)]
    public float fadeOutDuration;
    /// <summary>
    /// When to trigger fade.
    /// </summary>
    [BitMask]
    public FadeTarget fadeWhen;
    /// <summary>
    /// Current step.
    /// </summary>
    private int _currentStep;
    /// <summary>
    /// Time of next tick.
    /// </summary>
    private double _nextTick;
    /// <summary>
    /// Sample rate.
    /// </summary>
    private double _sampleRate;
    /// <summary>
    /// Current index of clip data.
    /// </summary>
    private int _index;
    /// <summary>
    /// Clip data.
    /// </summary>
    private float[] _clipData;
    /// <summary>
    /// Remaining beat events to be fired.
    /// </summary>
    private int _fireBeatEvent;
    /// <summary>
    /// Remaining any step events to be fired.
    /// </summary>
    private int _fireAnyStepEvent;
    /// <summary>
    /// Progress used to calculate approximate percentage.
    /// </summary>
    private double _progress;
    /// <summary>
    /// Temporary variable to set percentage on Audio Thread.
    /// </summary>
    private double _newPercentage = -1;
    /// <summary>
    /// Initial volume value to fade in.
    /// </summary>
    private float _initialVolumeValue;
    /// <summary>
    /// Volume of audio source just before fading in or out
    /// </summary>
    private float _volumeBeforeFade;
    /// <summary>
    /// Target volume when fade in/or finishes.
    /// </summary>
    private float _volumeAfterFade;
    /// <summary>
    /// Curernt percentage of fade progress.
    /// </summary>
    private float _fadeProgress = 1;
    /// <summary>
    /// Current fade speed;
    /// </summary>
    private float _fadeSpeed;
    /// <summary>
    /// What are we fading into.
    /// </summary>
    private FadeTarget _fadeTarget;
    /// <summary>
    /// Attached audio source.
    /// </summary>
    private AudioSource _audioSource;

    //[SerializeField]
    //private SequencerOneshot nextOneshot;

    private bool isBusy;
    private bool neededFadeout;
    private bool isLooping;
    private int beatNumber;
    private Action<Action> callback;
    private BEAT_TIMING callbackTiming;
    #endregion

    #region Properties
    /// <summary>
    /// True if clip data is loaded.
    /// </summary>
    public override bool IsReady
    {
        get { return _clipData != null; }
    }

    public bool IsBusy
    {
        get { return isBusy; }
    }

    #endregion

    #region Methods

    public override void OnAwake()
    {
#if UNITY_EDITOR
        _isMutedOld = this.isMuted;
        _oldBpm = this.bpm;
#endif
        StartCoroutine(Init());
    }

    /// <summary>
    /// Wait until sequencer is ready.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Init()
    {
        _audioSource = GetComponent<AudioSource>();
        _initialVolumeValue = _audioSource.volume;
        _volumeAfterFade = _initialVolumeValue;
        _sampleRate = AudioSettings.outputSampleRate;
        _audioSource.volume = 0;
        _index = -1;
        if (clip == null)
        {
            clip = _audioSource.clip;
        }
        if (clip != null)
        {
            while (_clipData == null)
            {
                if (clip.loadState == AudioDataLoadState.Loaded)
                {
                    _clipData = new float[clip.samples * clip.channels];
                    clip.GetData(_clipData, 0);
                }
                yield return null;
            }
            if (playWhenReady)
            {
                Play();
            }
            OnReady();
        }
        else Debug.LogWarning("Audio Clip can not be null.");
    }

    public void SetAudioClip(AudioClip newClip)
    {
        clip = newClip;
        if (clip != null)
        {
            _clipData = new float[clip.samples * clip.channels];
            clip.GetData(_clipData, 0);
        }
        else _clipData = null;
    }

    /// <summary>
    /// Set mute state.
    /// </summary>
    /// <param name="isMuted"></param>
    public override void Mute(bool isMuted)
    {
        Mute(isMuted, isMuted ? fadeOutDuration : fadeInDuration);
    }

    /// <summary>
    ///  Toggle mute state.
    /// </summary>
    /// <param name="isMuted"></param>
    /// <param name="fadeDuration"></param>
    public override void Mute(bool isMuted, float fadeDuration)
    {
        if (isMuted && fadeDuration > 0 && fadeWhen.IsFlagSet(FadeTarget.Mute))
        {
            _fadeTarget = FadeTarget.Mute;
            FadeOut(fadeDuration);
        }
        else if (!isMuted && fadeDuration > 0 && fadeWhen.IsFlagSet(FadeTarget.UnMute))
        {
            _fadeTarget = FadeTarget.UnMute;
            FadeIn(fadeDuration);
        }
        else
        {
            _audioSource.volume = isMuted ? 0 : _initialVolumeValue;
            _fadeProgress = 1;
            MuteInternal(isMuted);
        }
    }

    /// <summary>
    /// Changes default fade in and fade out durations.
    /// </summary>
    /// <param name="fadeIn"></param>
    /// <param name="fadeOut"></param>
    public override void SetFadeDurations(float fadeIn, float fadeOut)
    {
        fadeInDuration = fadeIn;
        fadeOutDuration = fadeOut;
    }

    private void MuteInternal(bool isMuted)
    {
        this.isMuted = isMuted;
#if UNITY_EDITOR
        _isMutedOld = this.isMuted;
#endif
    }

    /// <summary>
    /// Start playing.
    /// </summary>
    public override void Play()
    {
        Play(fadeInDuration);
    }

    /// <summary>
    /// Start playing from specified percentage.
    /// </summary>
    /// <param name="newPercentage"></param>
    public override void Play(double newPercentage)
    {
        //SetPercentage(newPercentage);
        Play();
    }

    /// <summary>
    /// Start playing from specified percentage.
    /// </summary>
    /// <param name="newPercentage"></param>
    public void Play(float timeInSecond, bool oneShot)
    {
        sequence[0] = true;

        Debug.Log(gameObject.name + " play ! ");

        //SetPercentage(yes);
        Play();
    }

    /// <summary>
    /// Start playing.
    /// </summary>
    /// <param name="fadeDuration"></param>
    public override void Play(float fadeDuration)
    {
        if (!IsPlaying && fadeDuration > 0 && fadeWhen.IsFlagSet(FadeTarget.Play))
        {
            _fadeTarget = FadeTarget.Play;
            PlayInternal();
            FadeIn(fadeDuration);
        }
        else
        {
            _audioSource.volume = isMuted ? 0 : _initialVolumeValue;
            _fadeProgress = 1;
            PlayInternal();
        }
    }

    private void PlayInternal()
    {
        _nextTick = AudioSettings.dspTime * _sampleRate;
        if (_clipData == null && clip != null)
        {
            _clipData = new float[clip.samples * clip.channels];
            clip.GetData(_clipData, 0);
        }

        _audioSource.Play();
        _isPlaying = true;
    }

    /// <summary>
    /// Stop playing.
    /// </summary>
    public override void Stop()
    {
        Stop(fadeOutDuration);
    }

    /// <summary>
    /// Stop playing.
    /// </summary>
    /// <param name="fadeDuration"></param>
    public override void Stop(float fadeDuration)
    {
        if (IsPlaying && fadeDuration > 0 && fadeWhen.IsFlagSet(FadeTarget.Stop))
        {
            _fadeTarget = FadeTarget.Stop;
            FadeOut(fadeDuration);
        }
        else
        {
            _audioSource.volume = isMuted ? 0 : _initialVolumeValue;
            _fadeProgress = 1;
            StopInternal();
        }
    }

    private void StopInternal()
    {
        _isPlaying = false;
        _audioSource.Stop();
        _clipData = null;
        _index = 0;
        _currentStep = 0;
        isBusy = false;
    }

    /// <summary>
    /// Pause/Unpause.
    /// </summary>
    /// <param name="isPaused"></param>
    public override void Pause(bool isPaused)
    {
        Pause(isPaused, isPaused ? fadeOutDuration : fadeInDuration);
    }

    /// <summary>
    /// Pause/Unpause.
    /// </summary>
    /// <param name="isPaused"></param>
    /// <param name="fadeDuration"></param>
    public override void Pause(bool isPaused, float fadeDuration)
    {
        if (isPaused && fadeDuration > 0 && fadeWhen.IsFlagSet(FadeTarget.Pause))
        {
            _fadeTarget = FadeTarget.Pause;
            FadeOut(fadeDuration);
        }
        else if (!isPaused && fadeDuration > 0 && fadeWhen.IsFlagSet(FadeTarget.UnPause))
        {
            _fadeTarget = FadeTarget.UnPause;
            PauseInternal(false);
            FadeIn(fadeDuration);
        }
        else
        {
            _audioSource.volume = isMuted ? 0 : _initialVolumeValue;
            _fadeProgress = 1;
            PauseInternal(isPaused);
        }
    }

    private void PauseInternal(bool isPaused)
    {
        if (isPaused)
        {
            _audioSource.Pause();
            _isPlaying = false;
        }
        else
        {
            _audioSource.UnPause();
            _isPlaying = true;
        }
    }

    /// <summary>
    /// Toggle mute state.
    /// </summary>
    public override void ToggleMute()
    {
        isMuted = !isMuted;
    }

    private void FadeIn(float duration)
    {
        _fadeSpeed = 1f / duration;
        _fadeProgress = 0;
        MuteInternal(false);
        _volumeBeforeFade = _audioSource.volume;
        _volumeAfterFade = _initialVolumeValue;
    }

    private void FadeOut(float duration)
    {
        _fadeSpeed = 1f / duration;
        _fadeProgress = 0;
        _volumeBeforeFade = _audioSource.volume;
        _volumeAfterFade = 0;
    }

    /// <summary>
    /// Get approximate percentage.
    /// </summary>
    /// <returns>Approximate percentage.</returns>
    public double GetPercentage()
    {
        double samplesTotal = _sampleRate * 60.0F / bpm * 4.0F;
        return _progress / samplesTotal;
    }

    /// <summary>
    /// Set approximate percentage.
    /// Ignores leftover percentage from rounding. Not precise.
    /// </summary>
    /// <param name="percentage">Approximate percentage.</param>
    public override void SetPercentage(double percentage)
    {
        _newPercentage = percentage;
    }

    /// <summary>
    /// Updates percentage of the sequence on Audio Thread.
    /// </summary>
    private void UpdatePercentage()
    {
        _index = 0;

        double samplesTotal = _sampleRate * 60.0F / bpm * 4.0F;
        double samplesPerTick = samplesTotal / signatureLo;
        double newSamplePos = samplesTotal * _newPercentage;
        double currentTickDouble = newSamplePos / samplesPerTick;
        _currentStep = (int)Math.Round(currentTickDouble, MidpointRounding.ToEven);
        if (log) print("Set Percentage: " + _currentStep + " (%" + _newPercentage + ")");
        _newPercentage = -1;
    }

    public void EndSound()
    {
        isBusy = false;
        _index = -1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isBusy)
        {
            neededFadeout = true;
        }

        if (neededFadeout && (_currentStep % 4) == (int)callbackTiming)
        {
            callback.Invoke(EndSound);
            neededFadeout = false;
        }

        while (_fireAnyStepEvent > 0)
        {
            _fireAnyStepEvent--;
            OnAnyStep();
        }
        while (_fireBeatEvent > 0)
        {
            _fireBeatEvent--;
            OnBeat();
        }
        if (_fadeProgress < 1)
        {
            _fadeProgress += Time.deltaTime * _fadeSpeed;
            if (_fadeProgress > 1) _fadeProgress = 1;
            _audioSource.volume = Mathf.Lerp(_volumeBeforeFade, _volumeAfterFade, _fadeProgress);
            if (_fadeProgress == 1)
            {
                switch (_fadeTarget)
                {
                    case FadeTarget.Play:
                    case FadeTarget.UnPause:
                    case FadeTarget.UnMute:
                        //Done on start of Fade.
                        break;
                    case FadeTarget.Stop:
                        StopInternal();
                        break;
                    case FadeTarget.Mute:
                        MuteInternal(true);
                        break;
                    case FadeTarget.Pause:
                        PauseInternal(true);
                        break;
                    /*default:
                        throw new ArgumentOutOfRangeException();*/
                }
            }
        }
    }

    /// <summary>
    /// Set Bpm.
    /// </summary>
    /// <param name="newBpm">Beats per minute.</param>
    public override void SetBpm(int newBpm)
    {
        if (newBpm < 10) newBpm = 10;
        bpm = newBpm;
    }

    public void SetCallback(Action<Action> _callback, BEAT_TIMING beatTiming)
    {
        callback = _callback;
        callbackTiming = beatTiming;
    }

    public void SetLoop(bool looping)
    {
        isLooping = looping;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!IsReady || !_isPlaying) return;
        if (_clipData == null) return;

        double samplesPerTick = _sampleRate * 60.0F / bpm * 4.0F / signatureLo;
        double sample = AudioSettings.dspTime * _sampleRate;

        if (_newPercentage > -1)
        {
            UpdatePercentage();
            return;
        }

        for (int dataIndex = 0; dataIndex < data.Length; dataIndex++)
        {
            if (sample + dataIndex >= _nextTick)
            {
                _nextTick += samplesPerTick;

                beatNumber++;
                _currentStep++;

                if (_currentStep > signatureLo)
                {
                    _currentStep = 1;
                }

                _progress = _currentStep * samplesPerTick;

                if (sequence[_currentStep - 1])
                {
                    Debug.Log("sound begin");
                    _index = 0;
                    isBusy = true;

                    if (OnBeat != null)
                    {
                        _fireBeatEvent++;
                    }

                    sequence[_currentStep - 1] = false;
                }

                if (OnAnyStep != null)
                {
                    _fireAnyStepEvent++;
                }
            }

            if (_index != -1)
            {
                data[dataIndex] += _clipData[_index];

                _index++;
                if (_index >= _clipData.Length)
                {
                    if (isLooping)
                    {
                        _index = 0;
                        Debug.Log("sound loop");
                    }
                    else
                    {
                        _index = -1;
                        neededFadeout = true;
                        Debug.Log("sound end");
                    }
                }
            }

            _progress = _currentStep * samplesPerTick + dataIndex;
        }
    }

#if UNITY_EDITOR

    private bool _isMutedOld;
    private int _oldBpm;

    /// <summary>
    /// Check and update when options are changed from editor.
    /// </summary>
    void LateUpdate()
    {
        if (IsReady)
        {
            if (_isMutedOld != isMuted)
            {
                _isMutedOld = isMuted;
                Mute(isMuted);
            }
            if (_oldBpm != bpm)
            {
                _oldBpm = bpm;
                SetBpm(bpm);
            }
        }
    }

    [MenuItem("GameObject/Sequencer/Sequencer", false, 10)]
    static void CreateSequencerController(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("Sequencer");
        go.AddComponent<AudioSource>().playOnAwake = false;
        go.AddComponent<Sequencer>();
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif

    #endregion

    #region Structs

    #endregion

    #region Classes

    #endregion
}