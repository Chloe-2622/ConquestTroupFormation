using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static VolumeManager;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance;

    private string playerName;
    public int chosenArena;

    [Header("General Volume")]
    [SerializeField] float generalVolume = 0.5f;
    [SerializeField] bool isGeneralOn = true;

    [Header("Music Volume")]
    [SerializeField] float musicVolume = 0.5f;
    [SerializeField] bool isMusicOn = true;

    [Header("Effects Volume")]
    [SerializeField] float effectsVolume = 0.5f;
    [SerializeField] bool isEffectsOn = true;

    
    public enum GraphismQuality
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
    public enum Resolution
    {
        R_1920x1080 = 0,
        R_800x600 = 1
    }
    /*
    [Header("Graphism")]
    [SerializeField] GraphismQuality graphismQuality = GraphismQuality.Medium;
    [SerializeField] Resolution resolution = Resolution.R_1920x1080;
    */

    private string previousScene;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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

    // Previous Scene
    public string getPreviousScene() { return previousScene; }
    public void setPreviousScene(string previousScene) { this.previousScene = previousScene; }


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
}
