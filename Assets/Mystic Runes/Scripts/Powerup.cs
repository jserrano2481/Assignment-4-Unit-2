using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MusicalRunes;


public abstract class Powerup : MonoBehaviour
{
    [SerializeField]
    protected PowerupConfig powerupConfig;
    [SerializeField]
    private Button powerupButton;
    [SerializeField]
    private RectTransform cooldownBar;

    protected int currentLevel;
    private int cooldownDuration => powerupConfig.GetCooldown(currentLevel);
    private float cooldownBarHeight;
    private int currentCooldown;

    public bool Interactable
    {
        get => powerupButton.interactable;
        set => powerupButton.interactable = IsAvailable && value;
    }

    protected bool IsAvailable => currentLevel > 0 && currentCooldown <= 0;


    void Start()
    {
        // grabbing the original height of the cooldownBar and using it as the maximum value for the bar height
        cooldownBarHeight = cooldownBar.sizeDelta.y;

        // Setting the cooldownbar height
        SetCooldownBarHeight();

        currentLevel = GameManager.Instance.GetPowerupLevel(powerupConfig.powerupType);

        // pavlov's dog action conditioning
        powerupButton.onClick.AddListener(OnClick);
        GameManager.Instance.sequenceCompleted += OnSequenceCompleted;
        GameManager.Instance.runeActivated += OnRuneActivated;
    }

    private void OnPowerupUpgraded(PowerupType upgradedPowerup, int newLevel)
    {
        if (upgradedPowerup != powerupConfig.powerupType)
        {
            return;
        }

        currentLevel = newLevel;

        Interactable = false;

    }

    protected abstract void PerformPowerupEffect();

    private void OnClick()
    {
        // reset the cooldown
        ResetCooldown();

        // mark the powerup button as unavailable and not interactable
        Interactable = false;

        // when player clicks on powerup, perform powerup
        PerformPowerupEffect();

        // if the player has clicked on the powerup and it's not available, scream at them
        Debug.Assert(IsAvailable, "Sod off, power up unavailable", gameObject);
    }


    private void ResetCooldown()
    {
        // return the currentCooldown to the original cooldownDuration value
        currentCooldown = cooldownDuration;

        // set the cooldown bar height
        SetCooldownBarHeight();
    }

    protected virtual void OnSequenceCompleted()
    {
        // check to see if rune activated
        if (powerupConfig.decreaseCooldownOnRuneActivation)
        {
            return;
        }

        // if decreaseCooldownOnRuneActivation is false, decrease cooldown
        DecreaseCooldown();
    }

    protected virtual void OnRuneActivated()
    {
        if (!powerupConfig.decreaseCooldownOnRuneActivation)
        {
            return;
        }

        DecreaseCooldown();
    }

    private void DecreaseCooldown()
    {
        // first have to check if cooldown is available
        if (IsAvailable)
        {
            return;
        }

        // decrease cooldown
        currentCooldown--;
        // aditional check: make sure currentCooldown is between 0 and currentCooldown value
        currentCooldown = Mathf.Max(0, currentCooldown);

        // set the bar height visual
        SetCooldownBarHeight();

        // when cooldown is unavailable/available, set it's interactivity
        Interactable = IsAvailable;
    }

    private void SetCooldownBarHeight()
    {
        // normalizing the height from 0 -> 1
        var fraction = (float)currentCooldown / cooldownDuration;

        // apply the fraction we calculated to the cooldownbar size
        cooldownBar.sizeDelta = new Vector2(cooldownBar.sizeDelta.x, fraction * cooldownBarHeight);
    }

    private void OnDestroy()
    {
        GameManager.Instance.sequenceCompleted -= OnSequenceCompleted;
        GameManager.Instance.runeActivated -= OnRuneActivated;
    }

}
