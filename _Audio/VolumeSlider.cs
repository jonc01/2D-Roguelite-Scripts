using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private bool isMusic;
    [SerializeField] private TextMeshProUGUI sliderTextDisplay;

    void Start()
    {
        if(isMusic) MusicSlider();
        else SFXSlider();
    }

    void MusicSlider()
    {
        slider.onValueChanged.AddListener(val => AudioManager.Instance.ChangeMusicVolume(val));
        slider.onValueChanged.AddListener(val => sliderTextDisplay.text = (val * 100).ToString("N0"));
    }

    void SFXSlider()
    {
        slider.onValueChanged.AddListener(val => AudioManager.Instance.ChangeSFXVolume(val));
        slider.onValueChanged.AddListener(val => sliderTextDisplay.text = (val * 100).ToString("N0"));
    }
}
