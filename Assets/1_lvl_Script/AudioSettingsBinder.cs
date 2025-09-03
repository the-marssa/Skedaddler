using UnityEngine;
using UnityEngine.UI;
using JSAM;

public class AudioSettingsBinder : MonoBehaviour
{
    [SerializeField] Slider musicSlider; 
    [SerializeField] Slider sfxSlider;  

    const string MK = "musicVol", SK = "sfxVol";

    void Awake()
    {
        float m = PlayerPrefs.GetFloat(MK, 0.8f);
        float s = PlayerPrefs.GetFloat(SK, 0.8f);
        if (musicSlider) musicSlider.SetValueWithoutNotify(m);
        if (sfxSlider) sfxSlider.SetValueWithoutNotify(s);

        AudioManager.MusicMuted = AudioManager.SoundMuted = AudioManager.MasterMuted = false;
        AudioManager.MusicVolume = musicSlider ? musicSlider.value : 0.8f;
        AudioManager.SoundVolume = sfxSlider ? sfxSlider.value : 0.8f;

        if (musicSlider) musicSlider.onValueChanged.AddListener(v => { AudioManager.MusicVolume = v; PlayerPrefs.SetFloat(MK, v); });
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(v => { AudioManager.SoundVolume = v; PlayerPrefs.SetFloat(SK, v); });
    }
    void OnDisable() => PlayerPrefs.Save();
}
