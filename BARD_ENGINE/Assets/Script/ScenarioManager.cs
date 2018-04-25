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
    //public List<AudioClip> clips;

    private int nextID;

    private bool isScenarioOpened;
    private string scenarioUrl;

    void Start()
    {
        blocks = new List<SoundBlock>();
        //clips = new List<AudioClip>();

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

    /*
     *      SOUNDBLOCKS
     */


    /*
     *      Spawns a default soundblock
     */
    public void SpawnSoundBlock()
    {
        SpawnSoundBlock(Vector3.zero, nextID, -1, false);
        nextID++;
    }

    /*
     *      Spawns a soundblock with specific values
     */
    public void SpawnSoundBlock(Vector3 position, int blockID, int clipId, bool isLooping)
    {
        Debug.Log("Spawning Soundblock " + blockID + " with " + clipId);
        GameObject blockGO = GameObject.Instantiate(SoundBlockPrefab, Vector3.zero, Quaternion.identity);
        blockGO.transform.SetParent(AppManager.Instance.GUIManager.scenarioOrigin, false);
        blockGO.transform.localPosition = position;
        
        // Set the audio on the soundblock
        SoundBlock soundBlock = blockGO.GetComponent<SoundBlock>();
        soundBlock.UpdateSoundlist();
        soundBlock.soundblockId = blockID;

        AudioSource source = soundBlock.GetComponent<AudioSource>();
        source.loop = isLooping;
        
        if (clipId != -1)
            soundBlock.SetClip(AppManager.Instance.ResourcesManager.GetResource(clipId).Name);
        
        // check if soundblock is the first, if so it is the entry point of the scenario
        if (blocks.Count == 0)
        {
            firstBlock = soundBlock;
            firstBlock.GetComponent<Image>().color = Color.green;
            firstBlock.rear.gameObject.SetActive(false);
        }

        blocks.Add(soundBlock);
        Debug.Log("Spawned Soundblock " + blockID);
    }

    /*
     *      Draw a transition link between to soundblocks
     */
    public void DrawLink(SoundBlock FromSoundBlock, SoundBlock ToSoundBlock)
    {
        // Spawns link
        GameObject go = GameObject.Instantiate(LinkPrefab, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(FromSoundBlock.transform, false);
        FromSoundBlock.link = go.GetComponent<Link>();

        // if the FROM block is looping, disable automatic transition
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
        AppManager.Instance.GUIManager.ToggleScenarioUI(true);
    }

    public void SaveScenario()
    {
        ScenarioSave scenario = new ScenarioSave();

        scenario.resources = new ResourceData[AppManager.Instance.ResourcesManager.Count()];
        for (int i = 0; i < scenario.resources.Length; ++i)
        {
            ResourceData resourceData = new ResourceData();
            resourceData.id = AppManager.Instance.ResourcesManager.Resources[i].Id;
        }

        scenario.soundblocks = new SoundBlockData[blocks.Count];
        int linkCount = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            SoundBlockData blockData = new SoundBlockData();
            SoundBlock block = blocks[i];
            blockData.blockId = block.soundblockId;
            if (block.source.clip != null)
                blockData.clipId = AppManager.Instance.ResourcesManager.GetResource(block.source.clip.name).Id;
            else
                blockData.clipId = -1;

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

        scenario.soundBlockNextId = nextID;
        scenario.resourceNextId = AppManager.Instance.ResourcesManager.nextResourceID;

        XmlSerializer writer = new XmlSerializer(scenario.GetType());
        using (FileStream stream = File.Create("structure"))
        {
            writer.Serialize(stream, scenario);
            stream.Close();
        }

        ZipFile zip = ZipFile.Read(scenarioUrl);
        zip.RemoveEntry("structure");
        zip.AddFile("structure");
        zip.Save(scenarioUrl);

        File.Delete("structure");

        Debug.Log("Saved Project " + scenarioUrl);
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

            LoadScenarioFile(new System.Uri(paths[0]).AbsolutePath);

            AppManager.Instance.GUIManager.ChangeScenarioName(paths[0]);
            AppManager.Instance.GUIManager.ToggleScenarioUI(true);

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

        if (structure != "")
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

            Resource soundBlockResource = AppManager.Instance.ResourcesManager.GetResource(scenarioSave.soundblocks[i].clipId);

            if (soundBlockResource == null)
            {
                Debug.Log("Soundblock " + i + ", Clip not found");
                LoadAudioFile(scenarioSave.soundblocks[i].clipId.ToString());
            }
            else
            {
                Debug.Log("Soundblock " + i + ", Clip already loaded");
            }

            SpawnSoundBlock(scenarioSave.soundblocks[i].position, scenarioSave.soundblocks[i].blockId, scenarioSave.soundblocks[i].clipId, scenarioSave.soundblocks[i].isLooping);

            Debug.Log("Loaded Soundblock " + scenarioSave.soundblocks[i].blockId);
        }

        for (int i = 0; i < scenarioSave.links.Length; i++)
        {
            Debug.Log("Drawing link ( " + scenarioSave.links[i].fromSoundblock + " -> " + scenarioSave.links[i].toSoundblock + " ) ");
            DrawLink(GetSoundBlock(scenarioSave.links[i].fromSoundblock), GetSoundBlock(scenarioSave.links[i].toSoundblock));
            Debug.Log("Drawn link");
        }

        Debug.Log("Scenario Loaded");

        nextID = scenarioSave.soundBlockNextId;
        AppManager.Instance.ResourcesManager.nextResourceID = scenarioSave.resourceNextId;
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

        AppManager.Instance.ResourcesManager.ClearResources();

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
        int fileId = AppManager.Instance.ResourcesManager.nextResourceID;

        while (!loader.isDone)
            yield return loader;

        AudioClip myAudioClip = loader.GetAudioClip();

        float[] samples = new float[myAudioClip.samples * myAudioClip.channels];
        myAudioClip.GetData(samples, 0);

        using (FileStream fs = File.Open(fileId.ToString(), FileMode.Create))
        {
            StreamWriter sw = new StreamWriter(fs);

            int i = 0;
            sw.WriteLine(fileName + "/" + myAudioClip.frequency + "/" + myAudioClip.channels + "/" + myAudioClip.length + "/" + myAudioClip.samples);

            for (i = 0; i < samples.Length; i++)
            {
                sw.WriteLine(samples[i]);
            }
        }

        myAudioClip.UnloadAudioData();

        ZipFile zip = ZipFile.Read(scenarioUrl);
        zip.AddFile(fileId.ToString());
        zip.Save(scenarioUrl);
        zip.Dispose();

        File.Delete(fileId.ToString());
        Debug.Log("Imported Audio File " + url);
        LoadAudioFile(fileId.ToString());
    }

    private void LoadAudioFile(string fileUrl)
    {
        if (fileUrl == "" || fileUrl == "-1")
            return;

        Debug.Log("LoadAudioFile " + fileUrl);
        ZipFile scenario = ZipFile.Read(scenarioUrl);
        Debug.Log("scenario found");

        Directory.CreateDirectory("extraction");
        foreach(var entry in scenario.Entries)
        {
            if (entry.FileName == fileUrl)
            {
                entry.Extract("extraction");
                Debug.Log(fileUrl + " found!");
                break;
            }
        }
        scenario.Dispose();

        string[] lines = File.ReadAllLines("extraction\\" + fileUrl);
        Directory.Delete("extraction", true);

        Debug.Log(fileUrl + " read!");

        string[] info = lines[0].Split('/');

        string audioName = info[0];
        int frequency = int.Parse(info[1]);
        int channels = int.Parse(info[2]);
        float length = float.Parse(info[3]);
        int nbOfSamples = int.Parse(info[4]);

        float[] readSamples = new float[nbOfSamples * channels];

        for (int i = 1; i < lines.Length; i++)
        {
            readSamples[i] = float.Parse(lines[i]);
        }

        Debug.Log(fileUrl + " yup!");

        int id = AppManager.Instance.ResourcesManager.CreateResource(audioName, nbOfSamples, channels, frequency, readSamples);

        if (AppManager.Instance.ResourcesManager.GetResource(id).Clip.loadState == AudioDataLoadState.Loaded)
        {
            UpdateSoundblockAudioLists();
        }
        else
        {
            Debug.LogError("Resource " + audioName + " / " + fileUrl + " didn't load properly");
        }

        Debug.Log("Audio File Loaded : " + audioName + " / " + fileUrl);
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
    public ResourceData[] resources;
    public SoundBlockData[] soundblocks;
    public LinkData[] links;
    public int soundBlockNextId;
    public int resourceNextId;
}

[System.Serializable]
public struct SoundBlockData
{
    public int blockId;
    public Vector3 position;
    public int clipId;
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

[System.Serializable]
public struct ResourceData
{
    public int id;
}