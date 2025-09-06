using UnityEngine;
using TMPro;
using VContainer;

public class HUDRunStats : MonoBehaviour
{
    [SerializeField] private TMP_Text starsText;
    [SerializeField] private TMP_Text lettersText;

    private IRunSessionProvider _provider;

    [Inject] public void Construct(IRunSessionProvider provider) => _provider = provider;

    private int _lastStars = int.MinValue;
    private int _lastLetters = int.MinValue;

    private void Update()
    {
        var s = _provider?.Current;
        if (s == null) return;

        if (s.Stars != _lastStars)
        {
            _lastStars = s.Stars;
            if (starsText) starsText.text = _lastStars.ToString();
        }

        if (s.Letters != _lastLetters)
        {
            _lastLetters = s.Letters;
            if (lettersText) lettersText.text = _lastLetters.ToString();
        }
    }
}
