using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bubble", menuName ="Bubble")]
public class BubbleProperties : ScriptableObject
{
    public enum BubbleColors
    {
        Red, 
        Green,
        Blue
    }

    public BubbleColors color;
    public Sprite sprite;
    public char textChar;
}
