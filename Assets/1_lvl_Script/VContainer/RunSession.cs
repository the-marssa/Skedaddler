public sealed class RunSession : IRunSession
{
    public int Stars { get; private set; }
    public int Letters { get; private set; }

    public void AddStars(int amount = 1) { if (amount > 0) Stars += amount; }
    public void AddLetters(int amount = 1) { if (amount > 0) Letters += amount; }
    public void Reset() { Stars = 0; Letters = 0; }
}
