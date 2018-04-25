using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundwaveDrawer : MonoBehaviour
{

    [SerializeField]
    AudioClip clip;
    
    [SerializeField]
    int width;

    [SerializeField]
    int height;

    [SerializeField]
    Color waveColor;

    void Start ()
    {
        StartCoroutine(DrawSoundwave());
	}
	
    IEnumerator DrawSoundwave()
    {
        float[] data = new float[clip.channels * clip.samples];
        clip.GetData(data, 0);

        Texture2D texture = new Texture2D(width, height);

        for (int i = 0; i < data.Length; ++i)
        {
            texture.SetPixel(1 - (i * width / data.Length), (int)(height * (data[i] + 1f) / 2.0f), waveColor);
        }

        texture.Apply();

        GetComponent<MeshRenderer>().material.mainTexture = texture;

        yield return new WaitForSeconds(0.1f);
    }
}
