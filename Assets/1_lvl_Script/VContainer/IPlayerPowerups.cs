public interface IPlayerPowerups
{
    void ActivateMagnet(float durationSeconds);
    void ActivateShield(float durationSeconds);

    bool IsMagnetActive { get; }
    bool IsShieldActive { get; }
    float MagnetRemaining { get; }
    float ShieldRemaining { get; }
}
