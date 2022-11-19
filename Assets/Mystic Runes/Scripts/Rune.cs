using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rune : MonoBehaviour
{

    private static readonly Color hintColor = new Color(1, 1, 1, 0.6f);

    [SerializeField]
    private Color activationColor;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private Image runeImage;
    [SerializeField]
    private float colorTransitionDuration = 0.3f;
    [SerializeField]
    private float minActivationDuration = 0.5f;
    [SerializeField]
    private Button button;

    public int Index
    {
        get;
        private set;
    }
    private Coroutine animationCoroutine;

    public void OnClick()
    {
        GameManager.Instance.OnRuneActivated(Index);
        ActivateRune();
    }

    public void SetUp(int runeIndex)
    {
        Index = runeIndex;
        transform.SetSiblingIndex(Index);
    }

    public void DisableInteraction()
    {
        button.interactable = false;
    }
    public void EnableInteraction()
    {
        button.interactable = true;
    }

    public Coroutine ActivateRune()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(ActivateRuneCoroutine());

        return animationCoroutine;
    }

    public Coroutine SetHintVisual(bool state)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (state)
        {
            animationCoroutine = StartCoroutine(LerpToColor(Color.white, hintColor));
        }
        else
        {
            animationCoroutine = StartCoroutine(LerpToColor(hintColor, Color.white));
        }

        return animationCoroutine;
    }

    public IEnumerator ActivateRuneCoroutine()
    {
        // play the audiosource
        audioSource.Play();

        // lerp to the activation color, wait while it finishs
        yield return LerpToColor(Color.white, activationColor);

        // wait for a little bit (minActivationDuration)
        yield return new WaitForSeconds(minActivationDuration);

        // 
        var duration = audioSource.clip.length;
        while (audioSource.isPlaying)
        {
            yield return new WaitForSeconds(duration - audioSource.time);
        }

        // lerp to white, wait while it finishes
        yield return LerpToColor(activationColor, Color.white);
    }

    private IEnumerator LerpToColor(Color start, Color end)
    {
        float elapsedTime = 0;
        float startTime = Time.time;

        while (elapsedTime < colorTransitionDuration)
        {
            runeImage.color = Color.Lerp(start, end, elapsedTime / colorTransitionDuration);
            elapsedTime = Time.time - startTime;
            yield return null;
        }
    }
}
