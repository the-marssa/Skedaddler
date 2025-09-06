public interface IRunSession
{
    int Stars { get; }
    int Letters { get; }

    void AddStars(int amount = 1);
    void AddLetters(int amount = 1);
    void Reset();
}
