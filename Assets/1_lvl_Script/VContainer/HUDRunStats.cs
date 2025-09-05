using TMPro;
using UnityEngine;
using VContainer;

public class HUDRunStats : MonoBehaviour
{
    [SerializeField] TMP_Text starsText;
    [SerializeField] TMP_Text lettersText;

    [Inject] IRunSession run;

    void OnEnable()
    {
        run.StarsChanged += OnStars;
        run.LettersChanged += OnLetters;
        OnStars(run.Stars);
        OnLetters(run.Letters);
    }

    void OnDisable()
    {
        run.StarsChanged -= OnStars;
        run.LettersChanged -= OnLetters;
    }

    void OnStars(int v) => starsText.text = v.ToString();
    void OnLetters(int v) => lettersText.text = v.ToString();
}
