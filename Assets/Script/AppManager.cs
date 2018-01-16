using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Xml.Serialization;
using System.IO;
using SFB; // StandardFileBrowser Plugin

public class AppManager : Singleton<AppManager>
{
    protected AppManager() { }

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private Transform soundblockParent;

    [SerializeField]
    private GameObject SoundBlockPrefab;

    [SerializeField]
    private GameObject LinkPrefab;

    [SerializeField]
    SoundBlock firstBlock;

    public SoundBlock activeSoundBlock;

    public bool IsDrawingLink;

    private SoundBlock firstSoundblockClick;

    public List<SoundBlock> blocks;
    public AudioClip[] clips;

    public string loadingText;

    private int nextID;

    void Start ()
    {
        blocks = new List<SoundBlock>();
        nextID = 0;
    }
	
	void Update ()
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
        foreach(AudioClip clip in clips)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }

        return null;
    }

    public void SpawnSoundBlock()
    {
        SpawnSoundBlock(Vector3.zero, nextID, "", false);
        nextID++;
    }

    public void SpawnSoundBlock(Vector3 position, int blockID, string clipName, bool isLooping)
    {
        GameObject blockGO = GameObject.Instantiate(SoundBlockPrefab, Vector3.zero, Quaternion.identity);
        blockGO.transform.SetParent(soundblockParent);
        blockGO.transform.localPosition = position;
        
        SoundBlock soundBlock = blockGO.GetComponent<SoundBlock>();
        soundBlock.Init();
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
    }

    public void EnterDrawLink()
    {
        IsDrawingLink = true;
    }


    public void DrawLink(SoundBlock soundBlock)
    {
        if (!firstSoundblockClick)
        {
            firstSoundblockClick = soundBlock;
            GameObject go = GameObject.Instantiate(LinkPrefab, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(firstSoundblockClick.transform);
            firstSoundblockClick.link = go.GetComponent<Link>();

            if (!firstSoundblockClick.source.loop)
                firstSoundblockClick.link.IsActive = true;
            else
                firstSoundblockClick.link.IsActive = false;

        }
        else if (firstSoundblockClick != soundBlock)
        {
            firstSoundblockClick.nextBlock = soundBlock;

            StopDrawLink();
        }
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

        StopDrawLink();
    }

    public void StopDrawLink()
    {
        IsDrawingLink = false;
        firstSoundblockClick = null;
    }

    public void SoundblockClicked(SoundBlock soundBlock)
    {
        if (IsDrawingLink)
        {
            DrawLink(soundBlock);
        }
    }

    public void SaveScenario()
    {
        string Title="";
        string Directory="";
        string FileName="";
        string Extension="bard";

        var path = StandaloneFileBrowser.SaveFilePanel(Title, Directory, FileName, Extension);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        ScenarioSave scenario = new ScenarioSave();
        scenario.soundblocks = new SoundBlockData[blocks.Count];
        int linkCount = 0;
        for (int i = 0;i < blocks.Count;i++)
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
        using (StreamWriter file = new StreamWriter(path))
        {
            writer.Serialize(file, scenario);
        }

    }

    public void LoadScenario()
    {
        string Title = "";
        string Directory = "";
        string Extension = "bard";

        var paths = StandaloneFileBrowser.OpenFilePanel(Title, Directory, Extension, false);
        if (paths.Length > 0)
        {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }

    private IEnumerator OutputRoutine(string url)
    {
        var loader = new WWW(url);
        yield return loader;
        Debug.Log(loader.text);
        AppManager.Instance.LoadScenarioText(loader.text);
    }

    public void LoadScenarioText(string text)
    {
        ScenarioSave scenarioSave = new ScenarioSave();
        XmlSerializer serializer = new XmlSerializer(scenarioSave.GetType());
        using (TextReader reader = new StringReader(text))
        {
            scenarioSave = (ScenarioSave)serializer.Deserialize(reader);
        }

        ResetScenario();

        for (int i = 0; i < scenarioSave.soundblocks.Length; i++)
        {
            SpawnSoundBlock(scenarioSave.soundblocks[i].position, scenarioSave.soundblocks[i].blockId, scenarioSave.soundblocks[i].clip, scenarioSave.soundblocks[i].isLooping);
        }

        for (int i = 0; i < scenarioSave.links.Length; i++)
        {
            DrawLink(GetSoundBlock(scenarioSave.links[i].fromSoundblock), GetSoundBlock(scenarioSave.links[i].toSoundblock));
        }

        nextID = scenarioSave.nextId;
    }

    public void ResetScenario()
    {
        for(int i = 0; i < blocks.Count;i++)
        {
            if (blocks[i].link)
                Destroy(blocks[i].link);

            Destroy(blocks[i].gameObject);
        }

        blocks.Clear();

        firstBlock = null;
        nextID = 0;
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