using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeField : MonoBehaviour
{
    [SerializeField] Sprite lowVolume;
    [SerializeField] Sprite mediumVolume;
    [SerializeField] Sprite highVolume;
    [SerializeField] Sprite noVolume;
    public enum VolumeType
    {
        General = 0,
        Music = 1,
        Effects = 2
    }
    public VolumeType volumeType;

    private Slider slider;
    private Image soundIcon;

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponentInChildren<Slider>();
        soundIcon = transform.Find("ShortButton").GetChild(0).GetComponentInChildren<Image>();

        slider.SetValueWithoutNotify(OptionsManager.Instance.getVolume(volumeType));
        updateSoundIcon();
    }

    public void setVolume()
    {
        OptionsManager.Instance.setVolume(volumeType, slider.value);
        if (!OptionsManager.Instance.isVolumeOn(volumeType)) { setVolumeOn(); }
        updateSoundIcon();
    }
    public void setVolumeOn()
    {
        OptionsManager.Instance.setVolumeOn(volumeType, !OptionsManager.Instance.isVolumeOn(volumeType));
        updateSoundIcon();
    }

    public void updateSoundIcon()
    {
        if (OptionsManager.Instance.isVolumeOn(volumeType))
        {
            float volume = OptionsManager.Instance.getVolume(volumeType);
            //Debug.Log(volumeType);
            //Debug.Log(volume);

            if (volume == 0) { soundIcon.sprite = noVolume; }
            else if (volume <= 0.33f) { soundIcon.sprite = lowVolume; }
            else if (volume >= 0.66f) { soundIcon.sprite = highVolume; }
            else { soundIcon.sprite = mediumVolume; }
        }
        else
        {
            soundIcon.sprite = noVolume;
        }
    }
}
