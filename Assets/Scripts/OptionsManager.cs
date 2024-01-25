    using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.UI;
using static VolumeField;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance { get; private set; }
    public UniversalRenderPipelineAsset urpAsset;
    public int currentQualitySelection;

    private string playerName;
    [HideInInspector] public int chosenArena;

    [Header("General Volume")]
    [SerializeField] float generalVolume = 0.5f;
    [SerializeField] bool isGeneralOn = true;

    [Header("Music Volume")]
    [SerializeField] float musicVolume = 0.5f;
    [SerializeField] bool isMusicOn = true;

    [Header("Effects Volume")]
    [SerializeField] float effectsVolume = 0.5f;
    [SerializeField] bool isEffectsOn = true;


    public enum Resolution
    {
        R_1920x1080 = 0,
        R_800x600 = 1
    }
    /*
    [Header("Graphism")]
    [SerializeField] Resolution resolution = Resolution.R_1920x1080;
    */

    private void Awake()
    {
        currentQualitySelection = 2;
        

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ChangeQuality(currentQualitySelection);
    }


    // Volume
    public bool isVolumeOn(VolumeType volumType)
    {
        if (volumType == VolumeType.General) { return isGeneralOn; }
        if (volumType == VolumeType.Music) { return isMusicOn; }
        if (volumType == VolumeType.Effects) { return isEffectsOn; }
        return false;
    }
    public void setVolumeOn(VolumeType volumType, bool isOn)
    {
        if (volumType == VolumeType.General) { isGeneralOn = isOn; }
        if (volumType == VolumeType.Music) { isMusicOn = isOn; }
        if (volumType == VolumeType.Effects) { isEffectsOn = isOn; }
    }

    public float getVolume(VolumeType volumType)
    {
        if (volumType == VolumeType.General) { return generalVolume; }
        if (volumType == VolumeType.Music) { return musicVolume; }
        if (volumType == VolumeType.Effects) { return effectsVolume; }
        return 0f;
    }

    public void setVolume(VolumeType volumType, float volume)
    {
        if (volumType == VolumeType.General) { generalVolume = volume; }
        if (volumType == VolumeType.Music) { musicVolume = volume; }
        if (volumType == VolumeType.Effects) { effectsVolume = volume; }
    }

    // Player Name
    public string getPlayerName()
    {
        if ( playerName == null || playerName == "")
        {
            return "Player 1";
        }
        return playerName;
    }
    public void setPlayerName(string name) { playerName = name; }

    public bool isReady()
    {
        if (chosenArena != 0)
        {
            return true;
        }
        else
        {
            Debug.Log("Select arena");
        }
        return false;
    }

    // Graphic Quality
    public void ChangeQuality(int qualityValue)
    {
        currentQualitySelection = qualityValue;

        switch (qualityValue)
        {
            case 0:
                urpAsset.renderScale = 0.1f;
                break;
            case 1:
                urpAsset.renderScale = 0.5f;
                break;
            case 2:
                urpAsset.renderScale = 1f;
                break;
            case 3:
                urpAsset.renderScale = 1.5f;
                break;
            case 4:
                urpAsset.renderScale = 2f;
                break;
        }
    }
}
