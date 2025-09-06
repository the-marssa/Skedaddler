public interface IRunSessionProvider
{
    IRunSession Current { get; }
    void StartNew(); 
}
