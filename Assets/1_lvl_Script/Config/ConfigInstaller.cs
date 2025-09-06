using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class ConfigInstaller : LifetimeScope
{
    [Header("Configs")]
    [SerializeField] private PlayerMovementConfig _movement;
    [SerializeField] private RunRewardsConfig _rewards;
    [SerializeField] private HitFeedbackConfig _hit;

    [Header("Scene Components")]
    [SerializeField] private PlayerHealth _playerHealth;      
    [SerializeField] private RunSessionProvider _runProvider; 
    [SerializeField] private PlayerPowerups _powerups;         

    protected override void Configure(IContainerBuilder b)
    {
        if (_movement != null) b.RegisterInstance(_movement).AsSelf();
        if (_rewards != null) b.RegisterInstance(_rewards).AsSelf();
        if (_hit != null) b.RegisterInstance(_hit).AsSelf();

        if (_playerHealth != null) b.RegisterComponent(_playerHealth).As<IPlayerHealth>();
        if (_runProvider != null) b.RegisterComponent(_runProvider).As<IRunSessionProvider>();
        if (_powerups != null) b.RegisterComponent(_powerups).As<IPlayerPowerups>();

        
        b.Register<RunSession>(Lifetime.Transient).AsSelf();
    }
}
