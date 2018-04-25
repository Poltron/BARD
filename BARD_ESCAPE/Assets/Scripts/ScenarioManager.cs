using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using Ionic.Zip;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField]
    private GUIManager guiManager;

    [SerializeField]
    private SoundHandler soundHandler;

    public List<SoundBlock> blocks;

    private SoundBlock firstBlock;
    public SoundBlock FirstBlock { get { return firstBlock; } }

    private int nextID;

    private string scenarioUrl;

    private void Start()
    {
        blocks = new List<SoundBlock>();
    }

    public void LoadScenario(string url)
    {
        if (!File.Exists(url))
        {
            Debug.LogError(url + " could not be opened ; file does not exist");
            return;
        }

        Directory.CreateDirectory("tmp");
        scenarioUrl = Path.GetFullPath(url);

        using (ZipFile zip = ZipFile.Read(url))
        {
            foreach (var entry in zip.Entries)
            {
                if (entry.FileName == "structure")
                {
                    entry.Extract("tmp");
                }
            }

            zip.Dispose();
        }

        string structure = File.ReadAllText("tmp/structure");
        File.Delete("tmp/structure");
        Directory.Delete("tmp");

        ClearScenario();

        LoadScenarioStructure(structure);
    }

    public void LoadScenarioStructure(string structure)
    {
        ScenarioSave scenarioSave = new ScenarioSave();
        XmlSerializer serializer = new XmlSerializer(scenarioSave.GetType());

        using (TextReader reader = new StringReader(structure))
        {
            scenarioSave = (ScenarioSave)serializer.Deserialize(reader);
        }

        Debug.Log("Scenario Loading...");

        for (int i = 0; i < scenarioSave.soundblocks.Length; i++)
        {
            Debug.Log("Loading Soundblock " + scenarioSave.soundblocks[i].blockId);

            if (!soundHandler.GetClip(scenarioSave.soundblocks[i].clip))
            {
                Debug.Log("Soundblock " + i + ", Clip not found");
                LoadAudioFile(scenarioSave.soundblocks[i].clip);
            }
            else
            {
                Debug.Log("Soundblock " + i + ", Clip already loaded");
            }

            CreateSoundBlock(scenarioSave.soundblocks[i].blockId, scenarioSave.soundblocks[i].clip, scenarioSave.soundblocks[i].isLooping);

            Debug.Log("Loaded Soundblock " + scenarioSave.soundblocks[i].blockId);
        }

        for (int i = 0; i < scenarioSave.links.Length; i++)
        {
            Debug.Log("Drawing link ( " + scenarioSave.links[i].fromSoundblock + " -> " + scenarioSave.links[i].toSoundblock + " ) ");
            GetSoundBlock(scenarioSave.links[i].fromSoundblock).nextBlock = GetSoundBlock(scenarioSave.links[i].toSoundblock);
            Debug.Log("Drawn link");
        }

        Debug.Log("Scenario Loaded");

        nextID = scenarioSave.nextId;

        guiManager.GoToScenarioPlaying();
    }

    private void LoadAudioFile(string url)
    {
        Debug.Log("LoadAudioFile " + url);
        ZipFile scenario = ZipFile.Read(scenarioUrl);

        Directory.CreateDirectory("extraction");
        foreach (var entry in scenario.Entries)
        {
            if (entry.FileName == url)
            {
                entry.Extract("extraction");
                break;
            }
        }
        scenario.Dispose();

        string[] lines = File.ReadAllLines("extraction\\" + url);
        Directory.Delete("extraction", true);

        string[] info = lines[0].Split('/');

        int frequency = int.Parse(info[0]);
        int channels = int.Parse(info[1]);
        int nbOfSamples = int.Parse(info[3]);

        AudioClip loadedAudioClip = AudioClip.Create(url, nbOfSamples, channels, frequency, false);
        float[] readSamples = new float[nbOfSamples * channels];

        for (int i = 1; i < lines.Length; i++)
        {
            readSamples[i] = float.Parse(lines[i]);
        }

        loadedAudioClip.SetData(readSamples, 0);

        if (loadedAudioClip.loadState == AudioDataLoadState.Loaded)
        {
            soundHandler.clips.Add(loadedAudioClip);
        }
        else
        {
            Debug.LogError("AudioClip didn't load properly");
        }
        Debug.Log("Audio File Loaded : " + url);
    }

    public void CreateSoundBlock()
    {
        CreateSoundBlock(nextID, "", false);
        nextID++;
    }

    public void CreateSoundBlock(int blockID, string clipName, bool isLooping)
    {
        Debug.Log("Spawning Soundblock " + blockID + " with " + clipName);

        SoundBlock soundBlock = new SoundBlock();
        soundBlock.soundblockId = blockID;
        soundBlock.isLooping = isLooping;
        soundBlock.clip = clipName;
            
        if (blocks.Count == 0)
        {
            firstBlock = soundBlock;
        }

        blocks.Add(soundBlock);

        Debug.Log("Spawned Soundblock " + blockID);
    }

    public void ClearScenario()
    {
        blocks.Clear();

        soundHandler.ClearClips();

        firstBlock = null;

        nextID = 0;
    }

    public SoundBlock GetSoundBlock(int id)
    {
        foreach (SoundBlock block in blocks)
        {
            if (block.soundblockId == id)
            {
                return block;
            }
        }

        return null;
    }
}

[System.Serializable]
public struct ScenarioSave
{
    public SoundBlockData[] soundblocks;
    public LinkData[] links;
    public int nextId;
}

[System.Serializable]
public struct SoundBlockData
{
    public int blockId;
    public Vector3 position;
    public string clip;
    public bool isFirstBlock;
    public bool isLooping;
}

[System.Serializable]
public struct LinkData
{
    public int fromSoundblock;
    public int toSoundblock;
    public bool isActive;
}
