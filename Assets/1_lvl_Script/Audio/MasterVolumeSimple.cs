using UnityEngine;
using UnityEngine.UI;
using JSAM;

[DisallowMultipleComponent]
public class MasterVolumeJSAM : MonoBehaviour
{
    [SerializeField] private Slider slider;

    
    [SerializeField] private bool alsoAffectAudioListener = true;

    private const string KEY_VOL = "volume.master.0_100";
    private const string KEY_MUTE = "volume.master.mute";

    private void Awake()
    {
        if (!slider)
        {
            slider = GetComponent<Slider>() ??
                     GetComponentInParent<Slider>() ??
                     GetComponentInChildren<Slider>(true);
        }

        if (!slider)
        {
            Debug.LogError("[MasterVolumeJSAM] Slider not found near this component");
            enabled = false;
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.wholeNumbers = true;
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void Start()
    {
        int savedV = PlayerPrefs.GetInt(KEY_VOL, Mathf.RoundToInt(Mathf.Clamp01(AudioManager.MasterVolume) * 100f));
        bool muted = PlayerPrefs.GetInt(KEY_MUTE, 0) == 1 || AudioManager.MasterMuted;

        slider.SetValueWithoutNotify(savedV);
        ApplyVolume100(savedV);
        ApplyMute(muted);

        AudioManager.OnMasterVolumeChanged += OnMasterVolumeChanged;
    }

    private void OnDestroy()
    {
        if (slider) slider.onValueChanged.RemoveListener(OnSliderChanged);
        AudioManager.OnMasterVolumeChanged -= OnMasterVolumeChanged;
    }

    private void OnSliderChanged(float v100f)
    {
        ApplyVolume100(Mathf.RoundToInt(v100f));
    }

    private void ApplyVolume100(int v100)
    {
        v100 = Mathf.Clamp(v100, 0, 100);
        float v01 = v100 / 100f;

        AudioManager.MasterMuted = (v100 == 0);
        AudioManager.MasterVolume = v01;

        
        if (alsoAffectAudioListener)
        {
            AudioListener.volume = v01; 
        }

        PlayerPrefs.SetInt(KEY_VOL, v100);
        PlayerPrefs.Save();
    }

    private void ApplyMute(bool muted)
    {
        AudioManager.MasterMuted = muted;
        if (alsoAffectAudioListener) AudioListener.volume = muted ? 0f : Mathf.Clamp01(slider.value / 100f);

        PlayerPrefs.SetInt(KEY_MUTE, muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnMasterVolumeChanged(float v01)
    {
        int v100 = Mathf.RoundToInt(Mathf.Clamp01(v01) * 100f);
        if (slider && Mathf.RoundToInt(slider.value) != v100)
            slider.SetValueWithoutNotify(v100);
    }
}
