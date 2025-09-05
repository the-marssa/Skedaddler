using System;

public interface IRunSession
{
    int Stars { get; }
    int Letters { get; }

    event Action<int> StarsChanged;
    event Action<int> LettersChanged;

    void AddStars(int amount = 1);
    void AddLetters(int amount = 1);
    void Reset();
}
