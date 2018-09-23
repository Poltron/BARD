using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

using Ionic.Zip;

public class ScenarioLoader : MonoBehaviour
{
    [SerializeField]
    private ResourcesManager resourcesManager;

    [SerializeField]
    private ScenarioManager scenarioManager;

    public void LoadScenarioFile(string url)
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

        if (structure == "")
        {
            Debug.LogError("Chargement d'un fichier structure vide ?");
            return;
        }

        ScenarioSave scenarioSave = new ScenarioSave();
        XmlSerializer serializer = new XmlSerializer(scenarioSave.GetType());

        using (TextReader reader = new StringReader(structure))
        {
            scenarioSave = (ScenarioSave)serializer.Deserialize(reader);

            reader.Dispose();
        }

        LoadScenarioResources(url, scenarioSave);
        LoadScenarioStructure(url, scenarioSave);
    }

    private void LoadScenarioResources(string scenarioUrl, ScenarioSave scenarioSave)
    {
        Debug.Log("Project Resources loading...");

        for (int i = 0; i < scenarioSave.resources.Length; i++)
        {
            Debug.Log("Loading Resource " + scenarioSave.resources[i].id);
            LoadAudioFile(scenarioUrl, scenarioSave.resources[i].id.ToString());
            Debug.Log("Loaded Resource " + scenarioSave.resources[i].id + " / " + resourcesManager.GetResource(scenarioSave.soundblocks[i].clipId).Name);
        }

        Debug.Log("Project Resources loaded");
    }

    private void LoadScenarioStructure(string scenarioUrl, ScenarioSave scenarioSave)
    {
        Debug.Log("Scenario Loading...");

        for (int i = 0; i < scenarioSave.soundblocks.Length; i++)
        {
            Debug.Log("Loading Soundblock " + scenarioSave.soundblocks[i].blockId);
            scenarioManager.CreateSoundBlock(scenarioSave.soundblocks[i].blockId, scenarioSave.soundblocks[i].clipId, scenarioSave.soundblocks[i].isLooping);
            Debug.Log("Loaded Soundblock " + scenarioSave.soundblocks[i].blockId);
        }

        for (int i = 0; i < scenarioSave.links.Length; i++)
        {
            Debug.Log("Drawing link ( " + scenarioSave.links[i].fromSoundblock + " -> " + scenarioSave.links[i].toSoundblock + " ) ");
            scenarioManager.DoLink(scenarioManager.GetSoundBlock(scenarioSave.links[i].fromSoundblock), scenarioManager.GetSoundBlock(scenarioSave.links[i].toSoundblock), scenarioSave.links[i].linkType);
            Debug.Log("Drawn link");
        }

        Debug.Log("Scenario Loaded");

        scenarioManager.nextID = scenarioSave.soundBlockNextId;
        resourcesManager.nextResourceID = scenarioSave.resourceNextId;
    }

    private void LoadAudioFile(string scenarioUrl, string fileUrl)
    {
        if (fileUrl == "" || fileUrl == "-1")
            return;

        Debug.Log("LoadAudioFile " + fileUrl);

        using (ZipFile scenario = ZipFile.Read(scenarioUrl))
        {
            if (Directory.Exists("extraction"))
                Directory.Delete("extraction", true);

            Directory.CreateDirectory("extraction");

            scenario.ExtractSelectedEntries("name=" + fileUrl + " OR name =" + fileUrl + "_setup", null, "extraction");
            scenario.Dispose();
        }

        if (!File.Exists("extraction\\" + fileUrl) || !File.Exists("extraction\\" + fileUrl + "_setup"))
        {
            Debug.LogError("LoadAudioFile raté : Fichiers " + fileUrl + " ou " + fileUrl + "_setup manquant.");
            return;
        }

        string[] linesData = File.ReadAllLines("extraction\\" + fileUrl);
        string[] infoData = linesData[0].Split('/');
        string audioName = infoData[0];
        int frequency = int.Parse(infoData[1]);
        int channels = int.Parse(infoData[2]);
        float length = float.Parse(infoData[3]);
        int nbOfSamples = int.Parse(infoData[4]);

        string[] linesSetup = File.ReadAllLines("extraction\\" + fileUrl + "_setup");
        string[] infoSetup = linesSetup[0].Split('/');
        int BPM = int.Parse(infoSetup[0]);
        int BPB = int.Parse(infoSetup[1]);
        float beginLoop = float.Parse(infoSetup[2]);
        float endLoop = float.Parse(infoSetup[3]);

        List<TransitionData> transitionsData = new List<TransitionData>();
        int nextTransitionId = 0;

        if (linesSetup.Length > 1)
        {
            string transitionSetup = linesSetup[1];

            int index = transitionSetup.IndexOf(":");
            string nextTransitionIdStr = transitionSetup.Substring(0, index);
            nextTransitionId = int.Parse(nextTransitionIdStr);

            transitionSetup = transitionSetup.Substring(index + 1, transitionSetup.Length - 1 - index);

            int endIndex = 0;
            int midIndex = 0;

            while ((index = transitionSetup.IndexOf("(")) != -1)
            {
                midIndex = transitionSetup.IndexOf(";");
                endIndex = transitionSetup.IndexOf(")");

                TransitionData trData = new TransitionData(0, 0);

                string transitionId = transitionSetup.Substring(index + 1, midIndex - index - 1);
                string transitionValue = transitionSetup.Substring(midIndex + 1, endIndex - midIndex - 1);

                Debug.Log("New Transition : " + transitionId + " / " + transitionValue);

                trData.id = int.Parse(transitionId);
                trData.value = float.Parse(transitionValue);

                transitionsData.Add(trData);

                transitionSetup = transitionSetup.Substring(endIndex + 1, transitionSetup.Length - 1 - endIndex);
            }
        }


        float[] readSamples = new float[nbOfSamples * channels];
        for (int i = 1; i < linesData.Length; i++)
        {
            if (linesData[i] != "")
                readSamples[i - 1] = float.Parse(linesData[i]);
        }

        if (Directory.Exists("extraction"))
            Directory.Delete("extraction", true);

        int clipId = int.Parse(fileUrl);
        resourcesManager.LoadResource(clipId, audioName, nbOfSamples, channels, frequency, readSamples, BPM, BPB, beginLoop, endLoop, transitionsData, nextTransitionId);

        if (resourcesManager.GetResource(clipId).Clip.loadState == AudioDataLoadState.Loaded)
        {
            //UpdateSoundblockAudioLists();
        }
        else
        {
            Debug.LogError("Resource " + audioName + " / " + fileUrl + " didn't load properly");
            return;
        }

        Debug.Log("Audio File Loaded : " + audioName + " / " + fileUrl);
    }

}

public enum LinkType
{
    Cut = 0,
    CrossFade,
    DemiCrossFade
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
    public LinkType linkType;
    public bool isActive;
}

[System.Serializable]
public struct ResourceData
{
    public int id;
}
