using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sounds {
    None,
    WaterPlunk,
    Roar,
    Crystallize,
    Spook,
    Ritual,
    Shield,
    Slash,
    Bash,
    Magic,
    Static,
    Screech,
    Pierce
}

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip waterPlunk;
    [SerializeField] private AudioClip roar;
    [SerializeField] private AudioClip crystallize;
    [SerializeField] private AudioClip spook;
    [SerializeField] private AudioClip ritual;
    [SerializeField] private AudioClip shield;
    [SerializeField] private AudioClip slash;
    [SerializeField] private AudioClip bash;
    [SerializeField] private AudioClip magic;
    [SerializeField] private AudioClip staticBuzz;
    [SerializeField] private AudioClip screech;
    [SerializeField] private AudioClip pierce;

    private const float MAX_VOL = 0.1f;

    public static SoundManager Instance { get; private set; }

    private static Dictionary<Sounds, AudioClip> soundClips;

    private List<AudioSource> activeSounds = new List<AudioSource>();
    private float volumePercent = 1.0f;

    void Start() {
        if(Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        soundClips = new Dictionary<Sounds, AudioClip> {
            { Sounds.WaterPlunk, waterPlunk },
            { Sounds.Roar, roar },
            { Sounds.Crystallize, crystallize },
            { Sounds.Spook, spook },
            { Sounds.Ritual, ritual },
            { Sounds.Shield, shield },
            { Sounds.Slash, slash },
            { Sounds.Bash, bash },
            { Sounds.Magic, magic },
            { Sounds.Static, staticBuzz },
            { Sounds.Screech, screech },
            { Sounds.Pierce, pierce }
        };
    }

    void Update() {
        for(int i = activeSounds.Count - 1; i >= 0; i--) {
            if(!activeSounds[i].isPlaying) {
                activeSounds.RemoveAt(i);
            }
        }
    }

    public void PlaySound(Sounds sound, float pitch = 1f) {
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        activeSounds.Add(audio);
        audio.clip = soundClips[sound];
        audio.volume = volumePercent * MAX_VOL;
        audio.pitch = pitch;
        audio.Play();
    }
}
