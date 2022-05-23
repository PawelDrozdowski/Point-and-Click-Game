using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static List<ItemData> collectedItems = new List<ItemData>();
    static float moveSpeed = 3.5f, moveAccuracy = 0.15f;
    public enum soundsNames
    {
        none,
        click,
        introLand,
        introChoir,
        step,
        use,
        plantGrow,
        crunch,
        explosion,
        santaAppears,
        playerInABag,
        win,
        credits
    }

    [Header("Setup")]
    public AnimationData[] playerAnimations;
    public RectTransform nameTag, hintBox;
    public AudioSource soundtrackSource;
    public AudioClip[] soundEffects;
    public GameObject globalLight;

    [Header("Local Scenes")]
    public Image blockingImage;
    public GameObject[] localScenes;
    int activeLocalScene = 0;
    public Transform[] playerStartPositions;
    public Color goToNextRoomDarkBar, goToNextRoomLightBar;

    [Header("Start Scene")]
    public GameObject startSceneCanvas, startSceneLight;
    public GameObject[] authors;
    public Transform playerCopy, landingPoint, startSceneGoToPoint;

    [Header("Equipment")]
    public GameObject equipmentCanvas;
    public Image[] equipmentSlots,equipmentImages;
    public Sprite emptyItemSlotSprite;
    public Color selectedItemColor;
    public int selectedCanvasSlotID = 0;
    public ItemData.items selectedItemID = ItemData.items.none;


    public IEnumerator MoveToPoint(Transform myObject, Vector2 point)
    {
        //calculate position difference
        Vector2 positionDifference = point - (Vector2)myObject.position;
        //flip object
        if (myObject.GetComponentInChildren<SpriteRenderer>() && positionDifference.x != 0)
            myObject.GetComponentInChildren<SpriteRenderer>().flipX = positionDifference.x > 0;
        //stop when we are near the point
        while (positionDifference.magnitude > moveAccuracy)
        {
            //move in direction frame
            myObject.Translate(moveSpeed * positionDifference.normalized * Time.deltaTime);
            //recalculate position difference
            positionDifference = point - (Vector2)myObject.position;
            yield return null;
        }
        //snap to point
        myObject.position = point;

        //tell ClickManager that the player has arrived
        if (myObject == FindObjectOfType<ClickManager>().player || activeLocalScene == 0)
            FindObjectOfType<ClickManager>().playerWalking = false;
        yield return null;
    }

    public void SelectItem(int equipmentCanvasID)
    {
        Color c = Color.white;
        c.a = 0;
        //change the alpha of the previous slot to 0
        equipmentSlots[selectedCanvasSlotID].color = c;

        //save changes and stop if an empty slot is clicked or the last item is removed
        if(equipmentCanvasID>= collectedItems.Count || equipmentCanvasID <0)
        {
            //no items selected
            selectedItemID = ItemData.items.none;
            selectedCanvasSlotID = 0;
            return;
        }

        //change the alpha of the new slot to x
        equipmentSlots[equipmentCanvasID].color = selectedItemColor;
        //save changes
        selectedCanvasSlotID = equipmentCanvasID;
        selectedItemID = collectedItems[selectedCanvasSlotID].itemID;
    }

    public void ShowItemName(int equipmentCanvasID)
    {
        //if an item is in this slot
        if (equipmentCanvasID < collectedItems.Count)
            UpdateNameTag(collectedItems[equipmentCanvasID]);
    }

    public void UpdateEquipmentCanvas()
    {
        //find out how many items we have and when to stop
        int itemsAmount = collectedItems.Count, itemSlotsAmount = equipmentSlots.Length;
        //replace no item sprites and old sprites with collectedItems[x].itemSlotSprite
        for (int i =0;i<itemSlotsAmount;i++)
        {
            //choose between emptyItemSlotSprite and an item sprite
            if (i < itemsAmount && collectedItems[i].itemSlotSprite != null)
                equipmentImages[i].sprite = collectedItems[i].itemSlotSprite;
            else
                equipmentImages[i].sprite = emptyItemSlotSprite;
        }
        //add special conditions for selecting items
        if (itemsAmount == 0)
            SelectItem(-1);
        else if (itemsAmount == 1)
            SelectItem(0);
    }

    public void UpdateNameTag(ItemData item)
    {
        if (item == null)
        {
            nameTag.parent.gameObject.SetActive(false);
            return;
        }
        nameTag.parent.gameObject.SetActive(true);
        TextMeshProUGUI t = nameTag.GetComponentInChildren<TextMeshProUGUI>();
        string nameText = item.objectName;
        Vector2 size = TextBoxSize(item.objectName, t.fontSize);

        //if we have collected the item, use different name and size
        if (collectedItems.Contains(item))
        {
            nameText = item.itemName;
            size = TextBoxSize(item.itemName, t.fontSize);
        }

        //change name
        t.text = nameText;
        //change size
        nameTag.sizeDelta = size;
        //move tag
        nameTag.localPosition = new Vector2(size.x/2, -0.5f);
    }

    public void UpdateHintBox(ItemData item, bool playerFlipped)
    {
        if(item == null)
        {
            //hide hint box
            hintBox.gameObject.SetActive(false);
            return;
        }

        TextMeshProUGUI t = hintBox.GetComponentInChildren<TextMeshProUGUI>();
        //show hint box
        hintBox.gameObject.SetActive(true);
        //change name
        t.text = item.hintMessage;
        //change size
        hintBox.sizeDelta = TextBoxSize(item.hintMessage,t.fontSize);
        //move hint box
        if (playerFlipped)
            hintBox.parent.localPosition = new Vector2(-1, 0);
        else
            hintBox.parent.localPosition = Vector2.zero;
    }

    private Vector2 TextBoxSize(string s, float fontSize)
    {
        float characterWidth = 1.2f;
        float lineHeight = 0.65f;
        string[] lines = s.Split('\n');
        int maxChars = 0;
        foreach (string myString in lines)
            if (myString.Length > maxChars)
                maxChars = myString.Length;

        return new Vector2(characterWidth* maxChars * fontSize, lineHeight * lines.Length);
    }

    public void MakeGoToRoomBarDark(Image bar)
    {
        bar.color = goToNextRoomDarkBar;
    }
    
    public void MakeGoToRoomBarLight(Image bar)
    {
        bar.color = goToNextRoomLightBar;
    }

    public void CheckSpecialConditions(ItemData item, bool canGetItem)
    {
        switch(item.itemID)
        {
            case ItemData.items.goToScene1:
                //go to room 1;
                StartCoroutine(ChangeScene(1, 0));
                break;
            case ItemData.items.goToScene2:
                //go to room 2;
                StartCoroutine(ChangeScene(2, 0));
                break;
            case ItemData.items.santa:
                //win the game;
                if (canGetItem)
                {
                    float delay =
                        item.successAnimation.sprites.Length * item.successAnimation.framesOfGap * AnimationData.targetFrameTime;
                    StartCoroutine(ChangeScene(3, delay));
                }
                break;
            default:
                break;
        }
    }

    public IEnumerator ChangeScene(int sceneNumber,float delay)
    {
        yield return new WaitForSeconds(delay);

        //if end game remove player
        if (sceneNumber == localScenes.Length - 1)
        {
            FindObjectOfType<ClickManager>().player.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }

        Color c = blockingImage.color;
        //screen goes black (in one second) and clicking is blocked
        blockingImage.enabled = true;
        while(blockingImage.color.a<1)
        {
            //increase color.a
            c.a += Time.deltaTime;
            yield return null;
            blockingImage.color = c;
        }

        //hide the old scene
        localScenes[activeLocalScene].SetActive(false);
        //show the new scene
        localScenes[sceneNumber].SetActive(true);
        //as we leave the start scene
        if (activeLocalScene == 0)
        {
            //start playing soundtrack music 
            soundtrackSource.Play();
            //turn on the global light
            globalLight.SetActive(true);
        }
        //say which one is currently used
        activeLocalScene = sceneNumber;
        //teleport the player
        FindObjectOfType<ClickManager>().player.position = playerStartPositions[sceneNumber].position;
        //hide hint box
        UpdateHintBox(null, false);
        //hide name tag
        UpdateNameTag(null);
        //reset animations
        foreach (SpriteAnimator spriteAnimator in FindObjectsOfType<SpriteAnimator>())
            spriteAnimator.PlayAnimation(null);
        //show equipment bar if not going to start or end scene
        equipmentCanvas.SetActive(sceneNumber > 0 && sceneNumber < localScenes.Length - 1);

        //if end game play sound
        if (sceneNumber == localScenes.Length - 1)
            PlaySound(soundsNames.win);
        //show new scene and enable clicking
        while (blockingImage.color.a > 0)
        {
            //decrease color.a
            c.a -= Time.deltaTime;
            yield return null;
            blockingImage.color = c;
        }
        blockingImage.enabled = false;
        yield return null;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        collectedItems.Clear();
    }
    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }

    public IEnumerator StartGameCoroutine()
    {
        ClickManager clickManager = GetComponent<ClickManager>();
        SpriteRenderer playerCopySpriteRenderer = playerCopy.GetComponent<SpriteRenderer>();
        Color c = playerCopySpriteRenderer.color;
        //hide any text
        startSceneCanvas.SetActive(false);
        //play click sound
        PlaySound(soundsNames.click);
        //play choir sound
        PlaySound(soundsNames.introChoir);
        //show light sprite
        startSceneLight.SetActive(true);
        //move the player down
        StartCoroutine(MoveToPoint(playerCopy, landingPoint.position));
        //wait for landing
        while(clickManager.playerWalking)
            yield return null;
        //play landing sound
        PlaySound(soundsNames.introLand);
        //play landing (use) animation
        playerCopy.GetComponent<SpriteAnimator>().PlayAnimation(playerAnimations[2]);
        yield return new WaitForSeconds(0.5f);

        //play idle animation
        playerCopy.GetComponent<SpriteAnimator>().PlayAnimation(playerAnimations[0]);
        yield return new WaitForSeconds(0.5f);

        //play walking animation
        playerCopy.GetComponent<SpriteAnimator>().PlayAnimation(playerAnimations[3]);
        //move the player to the left
        StartCoroutine(MoveToPoint(playerCopy, startSceneGoToPoint.position));
        yield return new WaitForSeconds(0.5f);

        //show authors
        foreach (GameObject g in authors)
        {
            //show text
            g.SetActive(true);
            //play credits sound
            PlaySound(soundsNames.credits);
            yield return new WaitForSeconds(1.5f);
            //hide text
            g.SetActive(false);
        }
        StartCoroutine(ChangeScene(1, 0.2f));
        yield return null;
    }

    public void PlaySound(soundsNames name)
    {
        if(name != soundsNames.none)
            AudioSource.PlayClipAtPoint(soundEffects[(int)name], transform.position);
    }
}
