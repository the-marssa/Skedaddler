using UnityEngine;

public static class GameRefs
{
    public static PlayerController PlayerController;
    public static PlayerHealth PlayerHealth;
    public static PlayerShield PlayerShield;
    public static PlayerMagnet PlayerMagnet;
    public static PlayerDeath PlayerDeath;
    public static ScoreManager Score => ScoreManager.Instance;
}