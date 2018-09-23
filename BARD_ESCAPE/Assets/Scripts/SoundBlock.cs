using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBlock
{
    public int soundblockId;
    public int clipId;
    public bool isLooping;
    public SoundBlock nextBlock;
    public LinkType linkType;
}
