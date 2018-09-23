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
    private ScenarioLoader scenarioLoader;

    [SerializeField]
    private SoundHandler soundHandler;

    [SerializeField]
    private ResourcesManager resourcesManager;

    public List<SoundBlock> blocks;

    private SoundBlock firstBlock;
    public SoundBlock FirstBlock { get { return firstBlock; } }

    public int nextID;

    private string scenarioUrl;

    private void Start()
    {
        blocks = new List<SoundBlock>();
    }

    public void LoadScenario(string url)
    {
        scenarioLoader.LoadScenarioFile(url);
        
        guiManager.GoToScenarioPlaying();
    }
    
    public void CreateSoundBlock(int blockID, int resourceID, bool isLooping)
    {
        Debug.Log("Spawning Soundblock " + blockID + " with " + resourceID);

        SoundBlock soundBlock = new SoundBlock();
        soundBlock.soundblockId = blockID;
        soundBlock.isLooping = isLooping;
        soundBlock.clipId = resourceID;

        if (blocks.Count == 0)
        {
            firstBlock = soundBlock;
        }

        blocks.Add(soundBlock);

        Debug.Log("Spawned Soundblock " + blockID);
    }

    public void DoLink(SoundBlock fromBlock, SoundBlock toBlock, LinkType linkType)
    {
        fromBlock.nextBlock = toBlock;
        fromBlock.linkType = linkType;
    }

    public void ClearScenario()
    {
        blocks.Clear();

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