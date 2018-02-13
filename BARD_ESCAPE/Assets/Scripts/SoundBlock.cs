using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBlock : MonoBehaviour
{
    public int soundblockId;
    public string clip;
    public bool isLooping;
    public SoundBlock nextBlock;
}
