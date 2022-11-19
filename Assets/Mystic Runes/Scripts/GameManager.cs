using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using MusicalRunes;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private readonly string savekey = "SaveKey";

    public static GameManager Instance
    {
        get;
        private set;
    }

    [Header("Rune Settings")]
    public int initialSequenceSize = 3;
    [SerializeField]
    private float delayBetweenRunePreview = 0.3f;
    [SerializeField]
    private int initialBoardSize = 4;
    [SerializeField]
    private RectTransform runesHolder;
    [SerializeField]
    private float maxTimeToChooseRune = 7f;
    [SerializeField]
    private int increaseBoardSizeEveryXSequences = 5;
    [SerializeField]
    private int shuffleBoardEveryXSequences = 5;
    [SerializeField]
    private List<Rune> availableRunePrefabs;

    [SerializeField]
    private int sequencesToAddRune = 5;

    [SerializeField]
    private int addedLives = 5;

    public List<Rune> BoardRunes
    {
        get;
        private set;
    }
    private List<Rune> instantiatedBoardRunes;
    /// <summary>
    /// Keep track of the current rune sequence
    /// </summary>
    private List<int> currentRuneSequence;

    /// <summary>
    /// Current index of the Rune that's been played
    /// </summary>
    private int currentPlayIndex;

    [NonSerialized]
    public bool isRuneChoosingTime;
    private float remainingRuneChooseTime;
    public int CurrentRuneIndex => currentRuneSequence[currentPlayIndex];

    [Header("Coin Settings")]

    [SerializeField]
    private int coinsPerRune = 1;
    [SerializeField]
    private int coinsPerRound = 10;
    [SerializeField]
    private int comboCoinsPerRune = 5;
    [SerializeField]
    private float choseTimeForCombo = 2f;

    [Header("Preview Settings")]
    [SerializeField]
    private GameObject[] spinlights;

    [Header("Powerup Settings")]
    [SerializeField]
    private List<Powerup> powerups;

    [Header("UI references")]
    [SerializeField]
    private TMP_Text coinsAmountText;
    [SerializeField]
    private TMP_Text highScoreText;
    [SerializeField]
    private TMP_Text timeText;
    [SerializeField]
    private TMP_Text livesText;
    [SerializeField]
    private Announcer announcer;

    public Action<int> coinsChanged;
    public Action sequenceCompleted;
    public Action runeActivated;

    public delegate void OnPowerupUpgradedDelegate(PowerupType upgradePowerup, int newLevel);
    public OnPowerupUpgradedDelegate powerupUpgraded;

    public int coinsAmount
    {
        get => saveData.coinsAmount;

        set
        {
            saveData.coinsAmount = value;
            coinsAmountText.text = coinsAmount.ToString();

            // trigger the coins changed action
            coinsChanged?.Invoke(value);
        }
    }
    private int highScore
    {
        get => saveData.highScore;

        set
        {
            saveData.highScore = value;
            highScoreText.text = highScore.ToString();
        }
    }
    private int remainingLives;
    public int RemainingLives
    {
        get => remainingLives;
        private set
        {
            remainingLives = value;
            livesText.text = RemainingLives.ToString();
        }
    }

    public void AddLives()
    {
        
        RemainingLives = RemainingLives + addedLives;
    }

    private int currentRound;
    private SaveData saveData;

    public int GetPowerupLevel(PowerupType powerupType)
    {
        return saveData.GetUpgradablelevel(powerupType);
    }

    public void UpgradePowerup(PowerupType powerupType, int price)
    {
        if (price > coinsAmount)
        {
            throw new Exception("You is broke, can't by this");
        }

        coinsAmount -= price;

        var newLevel = GetPowerupLevel(powerupType) + 1;
        saveData.SetUpgradableLevel(powerupType, newLevel);
        Save();

        powerupUpgraded?.Invoke(powerupType, newLevel);
    }

    void Awake()
    {
        if (Instance != null)
        {
            throw new System.Exception($"Multiple game managers in the scene! {Instance} :: {this}");
        }

        Instance = this;

        LoadSaveData();
        InitializeBoard();
        InitializeSequence();
        InitializeUI();
        PlaySequencePreview(2);
    }

    private void InitializeUI()
    {
        highScoreText.text = saveData.highScore.ToString();
        coinsAmountText.text = coinsAmount.ToString();
    }

    private void Reset()
    {
        for (int i = runesHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(runesHolder.GetChild(i).gameObject);
        }

        availableRunePrefabs.AddRange(instantiatedBoardRunes);

        InitializeBoard();
        InitializeSequence();
    }

    private void AddRandomRuneToBoard()
    {
        var runePrefab = availableRunePrefabs[UnityEngine.Random.Range(0, availableRunePrefabs.Count)];

        availableRunePrefabs.Remove(runePrefab);
        instantiatedBoardRunes.Add(runePrefab);

        var rune = Instantiate(runePrefab, runesHolder);
        rune.SetUp(BoardRunes.Count);
        BoardRunes.Add(rune);
    }

    private void InitializeBoard()
    {
        BoardRunes = new List<Rune>(initialBoardSize);
        instantiatedBoardRunes = new List<Rune>();

        for (int i = 0; i < initialBoardSize; i++)
        {
            AddRandomRuneToBoard();
        }
    }

    public void OnRuneActivated(int index)
    {
        // TODO: prevent rune clicks when sequence is finished

        //if (currentPlayIndex >= currentRuneSequence.Count)
        //{
        //    return;
        //}

        //if(currentRuneSequence[currentPlayIndex] == index)
        //{
        //    CorrectRuneSelected();
        //}
        if (CurrentRuneIndex == index)
        {
            CorrectRuneSelected();
        }
        else
        {
            FailedChoice();
        }

    }

    private void InitializeSequence()
    {
        // initializing the list curretnRuneSequence to be a list of ints with a size of initialSequenceSize = 3
        currentRuneSequence = new List<int>(initialSequenceSize);

        // looping through the currentRuneSequence, and then adding a random rune to the board
        for (int i = 0; i < initialSequenceSize; i++)
        {
            currentRuneSequence.Add(UnityEngine.Random.Range(0, BoardRunes.Count));
        }
    }

    public Coroutine PlaySequencePreview(float startDelay = 1, bool resetPlayindex = true)
    {
        isRuneChoosingTime = false;
        remainingRuneChooseTime = maxTimeToChooseRune;
        if (resetPlayindex)
        {
            currentPlayIndex = 0;
        }
        return StartCoroutine(PlaySequencePreviewCoroutine(startDelay));
    }

    private IEnumerator PlaySequencePreviewCoroutine(float startDelay = 1)
    {
        SetPlayerInteractivity(false);
        yield return new WaitForSeconds(startDelay);

        // TODO: Animate each rune in turn
        EnablePreviewFeedback();

        string sequence = "Sequence: ";
        foreach (var index in currentRuneSequence)
        {
            yield return BoardRunes[index].ActivateRune();
            yield return new WaitForSeconds(delayBetweenRunePreview);

            sequence += $"{index}, ";
        }

        Debug.Log(sequence);

        DisablePreviewFeedback();
        SetPlayerInteractivity(true);

        isRuneChoosingTime = true;
    }

    public void SetPlayerInteractivity(bool interactable)
    {
        foreach (var rune in BoardRunes)
        {
            if (interactable)
            {
                rune.EnableInteraction();
            }
            else
            {
                rune.DisableInteraction();
            }
        }

        foreach (var powerup in powerups)
        {
            powerup.Interactable = interactable;
        }
    }

    /// <summary>
    /// Sequence has finished and is incorrect
    /// </summary>
    private IEnumerator FailedSequence(bool choseWrongRune)
    {
        isRuneChoosingTime = false;
        SetPlayerInteractivity(false);

        if (choseWrongRune)
        {
            announcer.ShowWrongRuneText();
        }
        else
        {
            announcer.ShowFailedByTimeoutText();
        }

        yield return new WaitForSeconds(2);

        if (currentRound > highScore)
        {
            highScore = currentRound;
            announcer.ShowHighScoreText(highScore);
            Save();
            yield return new WaitForSeconds(3);
        }

        Reset();
        currentPlayIndex = 0;
        currentRound = 0;
        yield return PlaySequencePreview(2);
    }

    /// <summary>
    /// When your sequence has finished with no mistakes
    /// </summary>
    private void CompletedSequence()
    {
        Debug.Log("Completed Sequence");
        remainingRuneChooseTime = maxTimeToChooseRune;
        coinsAmount += coinsPerRound;
        
        //currentRound = currentRound +1
        currentRound++;
        Save();


        // add rune after x sequences in a row
        if (currentRound % sequencesToAddRune == 0)
        {
            AddRandomRuneToBoard();
        }

        

        // shuffle board?
        if (currentRound % increaseBoardSizeEveryXSequences == 0)
        {
            AddRandomRuneToBoard();
        }
        if (currentRound % shuffleBoardEveryXSequences == 0)
        {
            ShuffleBoard();
        }

        // trigger the sequence completed action
        sequenceCompleted?.Invoke();

        // creating a new rune sequence that builds off the previous
        currentRuneSequence.Add(UnityEngine.Random.Range(0, BoardRunes.Count));
        currentPlayIndex = 0;
        PlaySequencePreview(2);
    }

    private void ShuffleBoard()
    {
        var newOrder = Enumerable.Range(0, BoardRunes.Count).OrderBy(_ => UnityEngine.Random.value).ToList();

        BoardRunes = BoardRunes.OrderBy(rune => newOrder.FindIndex(order => order == rune.Index)).ToList();

        for (var sequenceIndex = 0; sequenceIndex < currentRuneSequence.Count; sequenceIndex++)
        {
            var runeIndex = currentRuneSequence[sequenceIndex];
            currentRuneSequence[sequenceIndex] = newOrder.FindIndex(order => order == runeIndex);
        }
        for (var index = 0; index < BoardRunes.Count; index++)
        {
            BoardRunes[index].SetUp(index);
        }
    }

    /// <summary>
    /// When the player has selected the correct Rune
    /// </summary>
    private void CorrectRuneSelected()
    {
        // detect your quick rune choice combo
        var combo = remainingRuneChooseTime > maxTimeToChooseRune - choseTimeForCombo && currentPlayIndex > 0;
        // add those coins if the combo was true
        coinsAmount += combo ? comboCoinsPerRune : coinsPerRune;

        remainingRuneChooseTime = maxTimeToChooseRune;
        runeActivated?.Invoke();
        currentPlayIndex++;

        if (currentPlayIndex >= currentRuneSequence.Count)
        {
            CompletedSequence();
        }
        else
        {
            Save();
        }
    }

    private void EnablePreviewFeedback()
    {
        foreach (var spinlight in spinlights)
        {
            spinlight.SetActive(true);
        }

        announcer.ShowPreviewText();
    }

    private void DisablePreviewFeedback()
    {
        foreach (var spinlight in spinlights)
        {
            spinlight.SetActive(false);
        }

        announcer.ShowSequenceText();

    }

    private void Update()
    {
        if (!isRuneChoosingTime)
            return;

        remainingRuneChooseTime -= Time.deltaTime;
        remainingRuneChooseTime = Mathf.Max(0, remainingRuneChooseTime);
        timeText.text = remainingRuneChooseTime.ToString("F1");

        if (Mathf.Approximately(remainingRuneChooseTime, 0))
        {
            FailedChoice(false);
        }
    }

    private void FailedChoice(bool choseWrongRune = true)
    {
        // implement remaining lives
        RemainingLives--;
        if (RemainingLives == 0)
        {
            StartCoroutine(FailedSequence(choseWrongRune));
            return;
        }

        // show announcer text
        if (choseWrongRune)
            announcer.ShowWrongRuneText();
        else
            announcer.ShowTimeoutText();

        // reset the rune timer
        remainingRuneChooseTime = maxTimeToChooseRune;
    }

    private void LoadSaveData()
    {
        if (PlayerPrefs.HasKey(savekey))
        {
            string serializedSaveData = PlayerPrefs.GetString(savekey);
            saveData = SaveData.Deserialize(serializedSaveData);

            return;
        }

        saveData = new SaveData(true);
    }

    private void Save()
    {
        string serializedSaveData = saveData.Serialize();
        PlayerPrefs.SetString(savekey, serializedSaveData);
        Debug.Log(serializedSaveData);
    }
}
