using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [Header("General")]
    public bool playerWalking = true;
    public Transform player;
    GameManager gameManager;

    [Header("Go To Click")]
    float goToClickMaxY = 1.7f;
    Camera myCamera;
    Coroutine goToClickCoroutine;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        myCamera = GetComponent<Camera>();
    }
    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
            goToClickCoroutine = StartCoroutine(GoToClick(Input.mousePosition));
    }
    public IEnumerator GoToClick(Vector2 mousePos)
    {
        //wait to make room for GoToItem() checks
        yield return new WaitForSeconds(0.05f);

        Vector2 targetPos = myCamera.ScreenToWorldPoint(mousePos);
        if (targetPos.y > goToClickMaxY || playerWalking)
            yield break;

        //hide hint box
        gameManager.UpdateHintBox(null, false);
        //start walking
        playerWalking = true;
        StartCoroutine(gameManager.MoveToPoint(player, targetPos));
        //play animation
        player.GetComponent<SpriteAnimator>().PlayAnimation(gameManager.playerAnimations[1]);
        //stop walking
        StartCoroutine(CleanAfterClick());
    }
    private IEnumerator CleanAfterClick()
    {
        while (playerWalking)
            yield return new WaitForSeconds(0.05f);
        player.GetComponent<SpriteAnimator>().PlayAnimation(null);
        yield return null;
    }

    public void GoToItem(ItemData item)
    {
        if (!playerWalking)
        {
            //play click sound
            gameManager.PlaySound(GameManager.soundsNames.click);
            //update hint box
            gameManager.UpdateHintBox(null, false);
            //play walk animation
            player.GetComponent<SpriteAnimator>().PlayAnimation(gameManager.playerAnimations[1]);
            playerWalking = true;
            //start moving player
            StartCoroutine(gameManager.MoveToPoint(player, item.goToPoint.position));
            //equipment stuff
            TryGettingItem(item);
        }
    }

    private void TryGettingItem(ItemData item)
    {
        bool canGetItem = item.requiredItemID == ItemData.items.none || gameManager.selectedItemID == item.requiredItemID;
        if (canGetItem)
        {
            GameManager.collectedItems.Add(item);
        }
        StartCoroutine(UpdateSceneAfterAction(item, canGetItem));
    }

    private IEnumerator UpdateSceneAfterAction(ItemData item, bool canGetItem)
    {
        //prevent goToClick if going to item
        yield return null;
        if (goToClickCoroutine != null)
            StopCoroutine(goToClickCoroutine);

        //wait for player reaching target
        while (playerWalking)
            yield return new WaitForSeconds(0.05f);
        //play player's base animation
        player.GetComponent<SpriteAnimator>().PlayAnimation(null);
        yield return new WaitForSeconds(0.5f);

        if (canGetItem)
        {
            //play use sound
            gameManager.PlaySound(GameManager.soundsNames.use);
            //play use animation
            player.GetComponent<SpriteAnimator>().PlayAnimation(gameManager.playerAnimations[2]);
            //remove objects
            foreach (GameObject g in item.objectsToRemove)
                Destroy(g);
            //show objects
            foreach (GameObject g in item.objectsToSetActive)
                g.SetActive(true);
            if(item.successAnimation)
                item.GetComponent<SpriteAnimator>().PlayAnimation(item.successAnimation);
            gameManager.UpdateEquipmentCanvas();
        }
        else
        {
            gameManager.UpdateHintBox(item, player.GetComponentInChildren<SpriteRenderer>().flipX);
        }
        gameManager.CheckSpecialConditions(item, canGetItem);
        yield return null;
    }
}
