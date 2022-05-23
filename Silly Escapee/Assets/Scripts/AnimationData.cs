using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Animation Data",menuName ="Scriptable Objects/Animation Data",order =1)]
public class AnimationData : ScriptableObject
{
    public static float targetFrameTime = 0.0167f;
    public int framesOfGap;
    public Sprite[] sprites;
    public bool loop;
    public bool returnToBase;
    public GameManager.soundsNames[] sounds;
}
