using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public SpriteRenderer mySpriteRenderer;
    public AnimationData baseAnimation;
    Coroutine previousAnimation;
    GameManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        PlayAnimation(baseAnimation);
    }

    public void PlayAnimation(AnimationData data)
    {
        //stop previous animation
        if (previousAnimation != null)
            StopCoroutine(previousAnimation);
        //grab a new animation
        previousAnimation = StartCoroutine(PlayAnimationCoroutine(data));
    }

    public IEnumerator PlayAnimationCoroutine(AnimationData data)
    {
        if (data == null)
            data = baseAnimation;

        int spritesAmount = data.sprites.Length, i=0, soundsAmount = data.sounds.Length;
        float waitTime = data.framesOfGap * AnimationData.targetFrameTime;
        //change sprites
        while(i<spritesAmount)
        {
            //play sound
            if(i<soundsAmount)
                gameManager.PlaySound(data.sounds[i]);
            //change sprite and increase i
            mySpriteRenderer.sprite = data.sprites[i++];        
            yield return new WaitForSeconds(waitTime);

            //looping
            if (data.loop && i >= spritesAmount)
                i = 0;
        }
        if (data.returnToBase && data != baseAnimation)
            PlayAnimation(baseAnimation);
        yield return null;
    }
}
