using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public const float START_PERCENT = 0.5f;
    [SerializeField] private Slider slider;

    // Start is called before the first frame update
    void Start() {
        slider.value = SoundManager.volPercent;
        slider.onValueChanged.AddListener(delegate { SetVolume(); });
    }

    public void SetVolume() {
        SoundManager.volPercent = slider.value;
        AudioListener.volume = SoundManager.volPercent * SoundManager.MAX_VOL;
    }
}
