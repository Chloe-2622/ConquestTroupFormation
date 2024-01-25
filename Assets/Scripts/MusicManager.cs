using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio sources")]
    [SerializeField] private AudioSource MainMenuMusic;
    [SerializeField] private AudioSource ArenaMusic;
    [SerializeField] private AudioClip Sword;
    [SerializeField] private AudioClip BowLoad;
    [SerializeField] private AudioClip BowShoot;
    [SerializeField] private AudioClip Hammer1;
    [SerializeField] private AudioClip Hammer2;

    public static MusicManager Instance;

    public enum SoundEffect
    {
        Sword, BowLoad, BowShoot, Hammer1, Hammer2
    }

    private OptionsManager optionManager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Assurez-vous qu'il n'y a qu'une seule instance du GameManager
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Gardez le GameManager lors des changements de scène
        }
        else
        {
            Destroy(gameObject); // Détruisez les doublons
        }
    }

    private void Start()
    {
        optionManager = OptionsManager.Instance;
    }

    private void OnEnable()
    {
        int level = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("------ " + optionManager);

        Debug.Log("Level : " + level);
        if (level < 4 && !MainMenuMusic.isPlaying)
        {
            MainMenuMusic.Play();
            ArenaMusic.Stop();
            Debug.Log("Playing Main Menu music");
        }
        if (level >= 4 && !ArenaMusic.isPlaying)
        {
            ArenaMusic.Play();
            MainMenuMusic.Stop();
            Debug.Log("Playing Arena music");
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        Debug.Log("Level : " + level + " menu " + !MainMenuMusic.isPlaying + " arena " + !ArenaMusic.isPlaying);
        if (level < 4 && !MainMenuMusic.isPlaying)
        {
            MainMenuMusic.Play();
            ArenaMusic.Stop();
            Debug.Log("Playing Main Menu music");
        }
        if (level >= 4 && !ArenaMusic.isPlaying)
        {
            ArenaMusic.Play();
            MainMenuMusic.Stop();
            Debug.Log("Playing Arena music");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (optionManager != null)
        {
            float musicVolume = optionManager.isVolumeOn(VolumeField.VolumeType.Music) ? optionManager.getVolume(VolumeField.VolumeType.Music) * optionManager.getVolume(VolumeField.VolumeType.General) : 0f;
            MainMenuMusic.volume = musicVolume;
            ArenaMusic.volume = musicVolume;
        }
        
    }

    public void PlaySound(SoundEffect soundEffect, Vector3 position)
    {
        float volume = optionManager.isVolumeOn(VolumeField.VolumeType.Effects) ? optionManager.getVolume(VolumeField.VolumeType.Effects) * optionManager.getVolume(VolumeField.VolumeType.General) : 0f;

        if (soundEffect == SoundEffect.Sword)
        {
            AudioSource.PlayClipAtPoint(Sword, position, volume);
        }
        if (soundEffect == SoundEffect.BowLoad)
        {
            AudioSource.PlayClipAtPoint(BowLoad, position, volume);
        }
        if (soundEffect == SoundEffect.BowShoot)
        {
            AudioSource.PlayClipAtPoint(BowShoot, position, volume);
        }
        if (soundEffect == SoundEffect.Hammer1)
        {
            AudioSource.PlayClipAtPoint(Hammer1, position, volume);
        }
        if (soundEffect == SoundEffect.Hammer2)
        {
            AudioSource.PlayClipAtPoint(Hammer2, position, volume);
        }


    }
}
