using System;

[Serializable]
public class RunSnapshot
{
    public int stars;
    public int letters;
    public int heartsPicked;
    public int score;
    public float distance;
    public int checkpointsReached;
    public long startedAtTicks; 
}

[Serializable]
public class LifetimeStats
{
    public long totalStars;
    public long totalLetters;
    public long totalHeartsPicked;
    public long totalRuns;
    public long totalDeaths;
    public int bestScore;
    public float bestDistance;
    public int maxCheckpointIndex;
    public long lastPlayedTicks;
}

[Serializable]
public class GameSave
{
    public LifetimeStats lifetime = new LifetimeStats();
}
