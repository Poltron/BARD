using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Xml.Serialization;
using SFB; // StandardFileBrowser Plugin
using Ionic.Zip;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField]
    private GameObject LinkPrefab;

    [SerializeField]
    private GameObject SoundBlockPrefab;

    
    private SoundBlock firstBlock;

    private SoundBlock activeSoundBlock;

    public List<SoundBlock> blocks;
    public List<AudioClip> clips;

    private int nextID;

    private bool isScenarioOpened;
    private string scenarioUrl;

    void Start()
    {
        blocks = new List<SoundBlock>();
        clips = new List<AudioClip>();

        nextID = 0;
    }

    void Update()
    {

    }

    public void StartSoundTrack()
    {
        firstBlock.PlaySound();
        activeSoundBlock = firstBlock;
    }

    public void StopSoundTrack()
    {
        activeSoundBlock.StopSound();
    }

    public void SetActiveSoundBlock(SoundBlock active)
    {
        activeSoundBlock = active;
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

    /*
     *      SOUNDBLOCKS
     */

    public void SpawnSoundBlock()
    {
        SpawnSoundBlock(Vector3.zero, nextID, "", false);
        nextID++;
    }

    public void SpawnSoundBlock(Vector3 position, int blockID, string clipName, bool isLooping)
    {
        Debug.Log("Spawning Soundblock " + blockID + " with " + clipName);
        GameObject blockGO = GameObject.Instantiate(SoundBlockPrefab, Vector3.zero, Quaternion.identity);
        blockGO.transform.SetParent(AppManager.Instance.GUIManager.scenarioOrigin);
        blockGO.transform.localPosition = position;

        SoundBlock soundBlock = blockGO.GetComponent<SoundBlock>();
        soundBlock.UpdateSoundlist();
        soundBlock.soundblockId = blockID;

        AudioSource source = soundBlock.GetComponent<AudioSource>();
        source.loop = isLooping;
        soundBlock.SetClip(clipName);

        if (blocks.Count == 0)
        {
            firstBlock = soundBlock;
            firstBlock.GetComponent<Image>().color = Color.green;
            firstBlock.rear.gameObject.SetActive(false);
        }

        blocks.Add(soundBlock);
        Debug.Log("Spawned Soundblock " + blockID);
    }

    public void DrawLink(SoundBlock FromSoundBlock, SoundBlock ToSoundBlock)
    {
        GameObject go = GameObject.Instantiate(LinkPrefab, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(FromSoundBlock.transform);
        FromSoundBlock.link = go.GetComponent<Link>();

        if (!FromSoundBlock.source.loop)
            FromSoundBlock.link.IsActive = true;
        else
            FromSoundBlock.link.IsActive = false;

        FromSoundBlock.nextBlock = ToSoundBlock;
    }

    /*
     *      SAVE/LOAD SCENARIO
     */

    public void NewScenario()
    {
        string Title = "";
        string Directory = "";
        string FileName = "";
        string Extension = "bard";

        var path = StandaloneFileBrowser.SaveFilePanel(Title, Directory, FileName, Extension);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        System.Uri scenarioUri = new System.Uri(path);

        ResetScenario();

        FileStream stream = File.Create("structure");
        stream.Close();

        ZipFile zip = new ZipFile();
        zip.AddFile("structure");
        zip.Save(path);
        zip.Dispose();

        File.Delete("structure");

        isScenarioOpened = true;
        scenarioUrl = scenarioUri.AbsolutePath;

        Debug.Log(scenarioUrl);

        AppManager.Instance.GUIManager.ChangeScenarioName(path);
        AppManager.Instance.GUIManager.EnableScenarioUI();
    }

    public void SaveScenario()
    {
        ScenarioSave scenario = new ScenarioSave();
        scenario.soundblocks = new SoundBlockData[blocks.Count];
        int linkCount = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            SoundBlockData blockData = new SoundBlockData();
            SoundBlock block = blocks[i];
            blockData.blockId = block.soundblockId;
            if (block.source.clip != null)
                blockData.clip = block.source.clip.name;
            else
                blockData.clip = "";

            if (firstBlock == block)
                blockData.isFirstBlock = true;

            blockData.isLooping = block.source.loop;
            blockData.position = block.transform.localPosition;
            
            if (block.link != null)
            {
                linkCount++;
            }

            scenario.soundblocks[i] = blockData;
        }

        scenario.links = new LinkData[linkCount];
        int nextLinkIndex = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].link != null)
            {
                LinkData link = new LinkData();
                link.fromSoundblock = blocks[i].soundblockId;
                link.toSoundblock = blocks[i].nextBlock.soundblockId;
                link.isActive = blocks[i].link.IsActive;

                scenario.links[nextLinkIndex] = link;
                nextLinkIndex++;
            }
        }

        scenario.nextId = nextID;

        XmlSerializer writer = new XmlSerializer(scenario.GetType());
        using (FileStream stream = File.Create("structure"))
        {
            writer.Serialize(stream, scenario);
            stream.Close();
        }
        
        Debug.Log("Saving Project " + scenarioUrl);

        ZipFile zip = ZipFile.Read(scenarioUrl);
        zip.RemoveEntry("structure");
        zip.AddFile("structure");
        zip.Save(scenarioUrl);

        File.Delete("structure");
    }

    public void LoadScenario()
    {
        string Title = "";
        string Directory = "";
        string Extension = "bard";

        var paths = StandaloneFileBrowser.OpenFilePanel(Title, Directory, Extension, false);
        if (paths.Length > 0)
        {
            scenarioUrl = new System.Uri(paths[0]).AbsolutePath;
            isScenarioOpened = true;

            AppManager.Instance.GUIManager.ChangeScenarioName(paths[0]);
            AppManager.Instance.GUIManager.EnableScenarioUI();

            LoadScenarioFile(new System.Uri(paths[0]).AbsolutePath);
        }
    }

    private void LoadScenarioFile(string url)
    {
        ZipFile zip = ZipFile.Read(url);
 
        foreach (var entry in zip.Entries)
        {
            if (entry.FileName == "structure")
            {
                entry.Extract();
                break;
            }
        }
        zip.Dispose();

        string structure = File.ReadAllText("structure");
        File.Delete("structure");

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

        ResetScenario();

        Debug.Log("Scenario Loading...");
        for (int i = 0; i < scenarioSave.soundblocks.Length; i++)
        {
            Debug.Log("Loading Soundblock " + scenarioSave.soundblocks[i].blockId);
            bool clipFound = false;
            foreach(var clip in clips)
            {
                if (clip.name == scenarioSave.soundblocks[i].clip)
                {
                    clipFound = true;
                    break;
                }
            }

            if (!clipFound)
            {
                Debug.Log("Soundblock " + i + ", Clip not found");
                LoadAudioFile(scenarioSave.soundblocks[i].clip);
            }
            else
            {
                Debug.Log("Soundblock " + i + ", Clip already loaded");
            }

            SpawnSoundBlock(scenarioSave.soundblocks[i].position, scenarioSave.soundblocks[i].blockId, scenarioSave.soundblocks[i].clip, scenarioSave.soundblocks[i].isLooping);

            Debug.Log("Loaded Soundblock " + scenarioSave.soundblocks[i].blockId);
        }

        for (int i = 0; i < scenarioSave.links.Length; i++)
        {
            Debug.Log("Drawing link ( " + scenarioSave.links[i].fromSoundblock + " -> " + scenarioSave.links[i].toSoundblock + " ) ");
            DrawLink(GetSoundBlock(scenarioSave.links[i].fromSoundblock), GetSoundBlock(scenarioSave.links[i].toSoundblock));
            Debug.Log("Drawn link");
        }

        Debug.Log("Scenario Loaded");

        nextID = scenarioSave.nextId;
    }

    public void ResetScenario()
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].link)
                Destroy(blocks[i].link);

            Destroy(blocks[i].gameObject);
        }

        blocks.Clear();
        
        for (int i = 0; i < clips.Count; i++)
        {
            clips[i].UnloadAudioData();
        }

        clips.Clear();

        firstBlock = null;
        activeSoundBlock = null;

        nextID = 0;
    }

    /*
     *      AUDIO FILE MANAGEMENT
     */

    public void ImportAudioFileInScenario()
    {
        string Title = "";
        string Directory = "";
        string Extension = "";

        var paths = StandaloneFileBrowser.OpenFilePanel(Title, Directory, Extension, false);
        if (paths.Length > 0)
        {
            StartCoroutine(ImportAudioFileInScenarioRoutine(new System.Uri(paths[0]).AbsolutePath));
        }
    }

    private IEnumerator ImportAudioNAudioRoutine(string url)
    {
        var loader = new WWW(url);

        while (!loader.isDone)
            yield return loader;

        Debug.Log("NAudio file play");

        NAudio.Wave.WaveOut waveOut = new NAudio.Wave.WaveOut();
        NAudio.Wave.WaveFileReader wavReader = new NAudio.Wave.WaveFileReader("bardintro.wav");
        Debug.Log("Format : " + wavReader.WaveFormat.ToString() + "sample count : " + wavReader.SampleCount + " :: " + wavReader.Length);


        var pcmLength = (int)wavReader.Length;
        var buffer = new byte[pcmLength];
        var bytesRead = wavReader.Read(buffer, 0, pcmLength);

        using (FileStream fs = File.Open("generatedaudiofile", FileMode.Create))
        {
            StreamWriter sw = new StreamWriter(fs);

            int i = 0;
            for (i = 0; i < buffer.Length; i += 3)
            {

                short int16 = (short)(((buffer[i] & 0xFF) << 8) | (buffer[i + 1] & 0xFF));
                float f = int16;
                int j = 16;
                sw.WriteLine(f);
            }
        }

        //waveOut.Play();
        yield return new WaitForSeconds(3);
        //waveOut.Stop();
        wavReader.Dispose();
        waveOut.Dispose();
    }

    private IEnumerator ImportAudioFileInScenarioRoutine(string url)
    {
        Debug.Log("Importing Audio File " + url);
        var loader = new WWW(url);
        
        string[] urlSplit = url.Split('/');
        string fileName = urlSplit[urlSplit.Length - 1].Split('.')[0];

        while (!loader.isDone)
            yield return loader;

        AudioClip myAudioClip = loader.GetAudioClip();

        float[] samples = new float[myAudioClip.samples * myAudioClip.channels];
        myAudioClip.GetData(samples, 0);

        using (FileStream fs = File.Open(fileName, FileMode.Create))
        {
            StreamWriter sw = new StreamWriter(fs);

            int i = 0;
            sw.WriteLine(myAudioClip.frequency + "/" + myAudioClip.channels + "/" + myAudioClip.length + "/" + myAudioClip.samples);

            for (i = 0; i < samples.Length; i++)
            {

                sw.WriteLine(samples[i]);
            }
        }
        
        ZipFile zip = ZipFile.Read(scenarioUrl);
        zip.AddFile(fileName);
        zip.Save(scenarioUrl);
        zip.Dispose();

        File.Delete(fileName);
        Debug.Log("Imported Audio File " + url);
        LoadAudioFile(fileName);
    }

    private void LoadAudioFile(string url)
    {
        Debug.Log("LoadAudioFile " + url);
        ZipFile scenario = ZipFile.Read(scenarioUrl);

        Directory.CreateDirectory("extraction");
        foreach(var entry in scenario.Entries)
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
        float length = float.Parse(info[2]);
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
            clips.Add(loadedAudioClip);
            UpdateSoundblockAudioLists();
        }
        else
        {
            Debug.LogError("AudioClip didn't load properly");
        }
        Debug.Log("Audio File Loaded : " + url);
    }

    private void UpdateSoundblockAudioLists()
    {
        foreach (SoundBlock block in blocks)
        {
            block.UpdateSoundlist();
        }
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