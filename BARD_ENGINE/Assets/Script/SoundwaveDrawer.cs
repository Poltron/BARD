using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundwaveDrawer : MonoBehaviour
{
    [SerializeField]
    Color waveColor;

    [SerializeField]
    Image rect;

    [SerializeField]
    AudioClip testClip;

    MaterialPropertyBlock propBlock;

    void Start ()
    {
        propBlock = new MaterialPropertyBlock();

        //if (SceneManager.GetActiveScene().name == "soundWave")
        //    Draw(testClip);

        propBlock.SetFloat("arraySize", 6.0f);

	}
	
    public void Draw(AudioClip clip)
    {
        int width = (int)rect.rectTransform.rect.width;
        int height = (int)rect.rectTransform.rect.height;
        Debug.Log("Drawing " + clip.name + " on " + width + ";" + height);
        StartCoroutine(DrawSoundwave(clip, width, height));
    }

    IEnumerator DrawSoundwave(AudioClip clip, int width, int height)
    {
        float[] data = new float[clip.channels * clip.samples];
        clip.GetData(data, 0);

        width = 1000;
        Texture2D texture = new Texture2D(width, height);

        for (int i = 0; i < data.Length; ++i)
        {
            texture.SetPixel((i * width / data.Length), (int)(height * (data[i] + 1f) / 2.0f), waveColor);
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);

        rect.sprite = sprite;

        yield return new WaitForSeconds(0.1f);
    }
}
