using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Config/Game Config", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [System.Serializable]
    public class RunnerSettings
    {
        [Header("Lateral")]
        [Range(0.5f, 20f)] public float lateralSpeed = 4f;
        [Range(0.02f, 0.3f)] public float xSmoothTime = 0.10f;
        [Range(0f, 0.3f)] public float inputDeadZone = 0.08f;

        [Header("Jump")]
        [Range(0.5f, 4f)] public float jumpHeight = 1.9f;

    }

    public enum CollectibleKind { Star, Letter, Heart }

    [System.Serializable]
    public class Collectible
    {
        public CollectibleKind kind;
        [Tooltip("Score")] public int scoreValue = 1;
        [Tooltip("HP")] public int healAmount = 1;
        [Tooltip("SFX")] public AudioClip sfx;
        [Tooltip("VFX")] public GameObject vfxPrefab;
        public Sprite icon;
    }

    [System.Serializable]
    public class FeedbackSettings
    {
        [Header("Camera Shake (Hit)")]
        [Range(0f, 1f)] public float shakeDuration = 0.25f;
        [Range(0f, 10f)] public float shakeAmplitude = 2f;

        [Header("UI Bounce (Jump)")]
        [Range(0f, 100f)] public float uiJumpOffset = 12f;
        [Range(0.01f, 1f)] public float uiJumpTime = 0.15f;
    }

    [Header("Runner")]
    public RunnerSettings runner = new RunnerSettings();

    [Header("Collectibles")]
    public List<Collectible> collectibles = new List<Collectible>()
    {
        new Collectible(){ kind = CollectibleKind.Star,   scoreValue = 1 },
        new Collectible(){ kind = CollectibleKind.Letter, scoreValue = 0 },
        new Collectible(){ kind = CollectibleKind.Heart,  healAmount = 1 },
    };

    [Header("Feedback")]
    public FeedbackSettings feedback = new FeedbackSettings();

    public Collectible GetCollectible(CollectibleKind k) => collectibles.Find(c => c.kind == k);
}
