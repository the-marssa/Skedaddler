using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<RunSession>(Lifetime.Scoped).As<IRunSession>();

        builder.RegisterComponentInHierarchy<PlayerHealth>().As<IPlayerHealth>();
        builder.RegisterComponentInHierarchy<HUDRunStats>();
        builder.RegisterComponentInHierarchy<ForeverRestart>();
    }
}
