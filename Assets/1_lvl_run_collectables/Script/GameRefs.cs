using UnityEngine;

public static class GameRefs
{
    public static PlayerHealth PlayerHealth;
    public static PlayerShield PlayerShield;
    public static PlayerMagnet PlayerMagnet;
    public static PlayerDeath PlayerDeath;
    public static ScoreManager Score => ScoreManager.Instance;
}