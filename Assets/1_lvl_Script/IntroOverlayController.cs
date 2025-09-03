using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class IntroOverlayController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] VideoPlayer player;
    [SerializeField] RawImage overlay;
    [SerializeField] GameObject[] menuRoots;

    [Header("Behaviour")]
    [SerializeField] float minSkipDelay = 0.35f;

    float _timer;
    bool _finished;
    bool _canSkip;

    void Awake()
    {
        if (!player) player = GetComponentInChildren<VideoPlayer>();
        if (!overlay) overlay = GetComponentInChildren<RawImage>(true);
    }

    void OnEnable()
    {
        _timer = 0f;
        _canSkip = false;
        _finished = false;
    }

    void Start()
    {
        if (AppFlow.SkipNextIntro)
        {
            RevealMenuAndDie();
#if !UNITY_EDITOR
            AppFlow.SkipNextIntro = false;
#endif
            return;
        }

        SetMenuVisible(false);
        if (overlay) overlay.gameObject.SetActive(true);

        if (player)
        {
            player.loopPointReached += _ => Finish();
            if (!player.isPrepared) player.Prepare();
            player.Play();
        }
        else
        {
            StartCoroutine(FinishAfterDelay(0.7f));
        }
    }

    System.Collections.IEnumerator FinishAfterDelay(float s)
    {
        float t = 0f;
        while (t < s)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        Finish();
    }

    void Update()
    {
        if (_finished) return;

        if (!_canSkip)
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= minSkipDelay) _canSkip = true;
            else return;
        }

        if (AnyInputPressed()) Finish();
    }

    bool AnyInputPressed()
    {
        var k = Keyboard.current;
        if (k != null && k.anyKey.wasPressedThisFrame) return true;

        var m = Mouse.current;
        if (m != null && (m.leftButton.wasPressedThisFrame ||
                          m.rightButton.wasPressedThisFrame ||
                          m.middleButton.wasPressedThisFrame)) return true;

        var t = Touchscreen.current;
        if (t != null && t.primaryTouch.press.wasPressedThisFrame) return true;

        var gp = Gamepad.current;
        if (gp != null && (gp.startButton.wasPressedThisFrame ||
                           gp.selectButton.wasPressedThisFrame ||
                           gp.buttonSouth.wasPressedThisFrame)) return true;

        return false;
    }

    void Finish()
    {
        if (_finished) return;
        _finished = true;
        if (player) player.Stop();
        RevealMenuAndDie();
    }

    void RevealMenuAndDie()
    {
        SetMenuVisible(true);
        if (overlay) overlay.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    void SetMenuVisible(bool v)
    {
        if (menuRoots == null) return;
        foreach (var go in menuRoots) if (go) go.SetActive(v);
    }
}
