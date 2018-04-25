using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    List<Resource> resources;
    public List<Resource> Resources { get { return resources; } }

    public int nextResourceID;

    void Awake()
    {
        resources = new List<Resource>();
    }

    public int CreateResource(string name, int nbOfSamples, int channels, int frequency, float[] data)
    {
        Resource resource = new Resource(GetNextResourceId());
        nextResourceID++;
        resource.Initialize(name, nbOfSamples, channels, frequency, data);
        resources.Add(resource);

        return resource.Id;
    }

    public int Count()
    {
        return resources.Count;
    }

    public Resource GetResource(string name)
    {
        return resources.Find(x => x.Name == name);
    }

    public Resource GetResource(int resourceId)
    {
        return resources.Find(x => x.Id == resourceId);
    }

    public void ClearResources()
    {
        foreach (Resource res in resources)
        {
            res.Clip.UnloadAudioData();
        }

        resources.Clear();

        nextResourceID = 0;
    }

    private int GetNextResourceId()
    {
        int toReturn = nextResourceID;
        nextResourceID++;
        return toReturn;
    }
}
