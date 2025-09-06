using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class RunSessionProvider : MonoBehaviour, IRunSessionProvider
{
    private IObjectResolver _resolver;
    public IRunSession Current { get; private set; }

    [Inject] public void Construct(IObjectResolver resolver) => _resolver = resolver;

    public void StartNew()
    {
        Current = _resolver.Resolve<RunSession>();
        Current.Reset();
    }
}
