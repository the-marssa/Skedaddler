using System;

public sealed class RunSession : IRunSession
{
    int stars, letters;

    public int Stars => stars;
    public int Letters => letters;

    public event Action<int> StarsChanged;
    public event Action<int> LettersChanged;

    public void AddStars(int amount = 1)
    {
        if (amount <= 0) return;
        stars += amount;
        StarsChanged?.Invoke(stars);
    }

    public void AddLetters(int amount = 1)
    {
        if (amount <= 0) return;
        letters += amount;
        LettersChanged?.Invoke(letters);
    }

    public void Reset()
    {
        stars = 0;
        letters = 0;
        StarsChanged?.Invoke(stars);
        LettersChanged?.Invoke(letters);
    }
}
