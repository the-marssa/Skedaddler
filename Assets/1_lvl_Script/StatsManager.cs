using UnityEngine;
using System;

[DisallowMultipleComponent]
public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    [Header("Score Formula")]
    [SerializeField] private int scorePerStar = 1;
    [SerializeField] private int scorePerLetter = 5;
    [SerializeField] private int scorePerHeart = 0;

    public GameSave Save { get; private set; }
    public RunSnapshot CurrentRun { get; private set; }

    public event Action OnRunChanged;
    public event Action<int> OnScoreChanged;

    
    private int _committedStars;
    private int _committedLetters;
    private int _committedHearts;
    private int _committedCheckpoints;

    private void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Save = SaveSystem.LoadOrCreate();
    }

    private void OnApplicationQuit()
    {
        if (Save != null) SaveSystem.Save(Save);
    }

    public void StartRun()
    {
        CurrentRun = new RunSnapshot
        {
            startedAtTicks = DateTime.UtcNow.Ticks
        };

        
        _committedStars = 0;
        _committedLetters = 0;
        _committedHearts = 0;
        _committedCheckpoints = 0;

        OnRunChanged?.Invoke();
        OnScoreChanged?.Invoke(0);
    }

   
    public void EndRun(bool died)
    {
        if (CurrentRun == null) return;

       
        CommitTailIfAny();

        var lt = Save.lifetime;
        lt.totalRuns++;
        if (died) lt.totalDeaths++; 

        if (CurrentRun.score > lt.bestScore) lt.bestScore = CurrentRun.score;
        if (CurrentRun.distance > lt.bestDistance) lt.bestDistance = CurrentRun.distance;

        if (CurrentRun.checkpointsReached > lt.maxCheckpointIndex)
            lt.maxCheckpointIndex = CurrentRun.checkpointsReached;

        lt.lastPlayedTicks = DateTime.UtcNow.Ticks;

        SaveSystem.Save(Save);
        CurrentRun = null;
    }

    public void AddStars(int amount = 1)
    {
        if (CurrentRun == null) return;
        amount = Mathf.Max(0, amount);
        CurrentRun.stars += amount;
        AddScore(amount * scorePerStar);
    }

    public void AddLetters(int amount = 1)
    {
        if (CurrentRun == null) return;
        amount = Mathf.Max(0, amount);
        CurrentRun.letters += amount;
        AddScore(amount * scorePerLetter);
    }

    public void AddHeartPickup(int amount = 1)
    {
        if (CurrentRun == null) return;
        amount = Mathf.Max(0, amount);
        CurrentRun.heartsPicked += amount;
        if (scorePerHeart != 0) AddScore(amount * scorePerHeart);
    }

    public void SetDistance(float meters)
    {
        if (CurrentRun == null) return;
        if (meters > CurrentRun.distance)
        {
            CurrentRun.distance = meters;
            OnRunChanged?.Invoke();
        }
    }

    private void AddScore(int delta)
    {
        CurrentRun.score += delta;
        OnRunChanged?.Invoke();
        OnScoreChanged?.Invoke(CurrentRun.score);
    }

    public int GetScore() => CurrentRun != null ? CurrentRun.score : 0;

    
    public void CommitCheckpoint(int checkpointIndex)
    {
        if (CurrentRun == null) return;

        
        CurrentRun.checkpointsReached = Mathf.Max(CurrentRun.checkpointsReached, checkpointIndex);

       
        int dStars = Mathf.Max(0, CurrentRun.stars - _committedStars);
        int dLetters = Mathf.Max(0, CurrentRun.letters - _committedLetters);
        int dHearts = Mathf.Max(0, CurrentRun.heartsPicked - _committedHearts);

        
        var lt = Save.lifetime;
        lt.totalStars += dStars;
        lt.totalLetters += dLetters;
        lt.totalHeartsPicked += dHearts;

        
        if (checkpointIndex > lt.maxCheckpointIndex)
            lt.maxCheckpointIndex = checkpointIndex;

        lt.lastPlayedTicks = DateTime.UtcNow.Ticks;

        
        _committedStars = CurrentRun.stars;
        _committedLetters = CurrentRun.letters;
        _committedHearts = CurrentRun.heartsPicked;
        _committedCheckpoints = CurrentRun.checkpointsReached;

        SaveSystem.Save(Save);
    }

    
    private void CommitTailIfAny()
    {
        
        int dStars = Mathf.Max(0, CurrentRun.stars - _committedStars);
        int dLetters = Mathf.Max(0, CurrentRun.letters - _committedLetters);
        int dHearts = Mathf.Max(0, CurrentRun.heartsPicked - _committedHearts);

        if (dStars == 0 && dLetters == 0 && dHearts == 0) return;

        var lt = Save.lifetime;
        lt.totalStars += dStars;
        lt.totalLetters += dLetters;
        lt.totalHeartsPicked += dHearts;
        lt.lastPlayedTicks = DateTime.UtcNow.Ticks;

        
        _committedStars = CurrentRun.stars;
        _committedLetters = CurrentRun.letters;
        _committedHearts = CurrentRun.heartsPicked;

        SaveSystem.Save(Save);
    }

    public RunSnapshot LastRunCached { get; private set; }

    public void CacheLastRunForMenu()
    {
        if (CurrentRun == null) return;
        LastRunCached = new RunSnapshot
        {
            startedAtTicks = CurrentRun.startedAtTicks,
            stars = CurrentRun.stars,
            letters = CurrentRun.letters,
            heartsPicked = CurrentRun.heartsPicked,
            distance = CurrentRun.distance,
            score = CurrentRun.score,
            checkpointsReached = CurrentRun.checkpointsReached
        };
    }
}
