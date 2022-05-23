using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    public enum items
    {
        none,
        unreachable,
        goToScene1,
        goToScene2,
        water,
        plant,
        booze,
        santa
    }
    [Header("Setup")]
    public Transform goToPoint;
    public items itemID, requiredItemID;
    public string objectName;
    public string itemName;

    [Header("Success")]
    public GameObject[] objectsToRemove;
    public GameObject[] objectsToSetActive;
    public AnimationData successAnimation;
    public Sprite itemSlotSprite;

    [Header("Failure")]
    [TextArea(3,3)]
    public string hintMessage;
    public Vector2 hintBoxSize = new Vector2(4, 0.65f);
}
