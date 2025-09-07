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
    [SerializeField] private PlayerPowerups _playerPowerups;   
    [SerializeField] private RunSessionProvider _runProvider;   

    protected override void Configure(IContainerBuilder b)
    {
        
        if (_movement) b.RegisterInstance(_movement).AsSelf();
        if (_rewards) b.RegisterInstance(_rewards).AsSelf();
        if (_hit) b.RegisterInstance(_hit).AsSelf();

        
        if (_playerHealth) b.RegisterComponent(_playerHealth).As<IPlayerHealth>().AsSelf();
        else b.RegisterComponentInHierarchy<PlayerHealth>().As<IPlayerHealth>().AsSelf();

        if (_playerPowerups) b.RegisterComponent(_playerPowerups).As<IPlayerPowerups>().AsSelf();
        else b.RegisterComponentInHierarchy<PlayerPowerups>().As<IPlayerPowerups>().AsSelf();

        if (_runProvider) b.RegisterComponent(_runProvider).As<IRunSessionProvider>().AsSelf();
        else b.RegisterComponentInHierarchy<RunSessionProvider>().As<IRunSessionProvider>().AsSelf();

    
        b.Register<RunSession>(Lifetime.Scoped);
    }
}
