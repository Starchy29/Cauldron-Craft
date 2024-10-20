using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sounds {
    None,
    WaterPlunk,
    Roar,
    Crystallize,
    SoulHeal,
    Ritual,
    Shield,
    Slash,
    Bash,
    Hex,
    Aura,
    Haunt,
    Pierce,
    Whoosh,
    ButtonHover,
    ButtonClick,
    BackMenu,
    TileHover,
    Sand,
    Death,
    Bone,
    SludgeToss,
    SludgeHit,
    SludgePuddle,
    Capture,
    CaptureLoss,
    PlantGrowth,
    StretchFlesh,
    Bubbles,
    Pop,
    Start,
    FireSpawn,
    FireHit,
    Pause,
    MushroomGrow,
    VineLash,
    Bump,
    Spawn,
}

public class SoundManager : MonoBehaviour
{
    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonHover;
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip backMenu;
    [SerializeField] private AudioClip tileHover;
    [SerializeField] private AudioClip start;
    [SerializeField] private AudioClip pause;

    [Header("Gameplay Sounds")]
    [SerializeField] private AudioClip waterPlunk;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip capture;
    [SerializeField] private AudioClip captureLoss;
    [SerializeField] private AudioClip pop;
    [SerializeField] private AudioClip spawn;

    [Header("Move Sounds")]
    [SerializeField] private AudioClip roar;
    [SerializeField] private AudioClip crystallize;
    [SerializeField] private AudioClip soulHeal;
    [SerializeField] private AudioClip ritual;
    [SerializeField] private AudioClip shield;
    [SerializeField] private AudioClip slash;
    [SerializeField] private AudioClip bash;
    [SerializeField] private AudioClip hex;
    [SerializeField] private AudioClip aura;
    [SerializeField] private AudioClip haunt;
    [SerializeField] private AudioClip pierce;
    [SerializeField] private AudioClip whoosh;
    [SerializeField] private AudioClip sand;
    [SerializeField] private AudioClip bone;
    [SerializeField] private AudioClip sludgeToss;
    [SerializeField] private AudioClip sludgeHit;
    [SerializeField] private AudioClip sludgePuddle;
    [SerializeField] private AudioClip plantGrowth;
    [SerializeField] private AudioClip stretchFlesh;
    [SerializeField] private AudioClip bubbles;
    [SerializeField] private AudioClip fireSpawn;
    [SerializeField] private AudioClip fireHit;
    [SerializeField] private AudioClip shroomGrow;
    [SerializeField] private AudioClip vineLash;
    [SerializeField] private AudioClip bump;
    [SerializeField] private AudioClip[] walks;

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

        AudioListener.volume = MAX_VOL * volumePercent;

        Instance = this;
        DontDestroyOnLoad(gameObject);

        soundClips = new Dictionary<Sounds, AudioClip> {
            { Sounds.WaterPlunk, waterPlunk },
            { Sounds.Roar, roar },
            { Sounds.Crystallize, crystallize },
            { Sounds.SoulHeal, soulHeal },
            { Sounds.Ritual, ritual },
            { Sounds.Shield, shield },
            { Sounds.Slash, slash },
            { Sounds.Bash, bash },
            { Sounds.Hex, hex },
            { Sounds.Aura, aura },
            { Sounds.Haunt, haunt },
            { Sounds.Pierce, pierce },
            { Sounds.Whoosh, whoosh },
            { Sounds.ButtonHover, buttonHover },
            { Sounds.ButtonClick, buttonClick },
            { Sounds.BackMenu, backMenu },
            { Sounds.TileHover, tileHover },
            { Sounds.Sand, sand },
            { Sounds.Death, death },
            { Sounds.Bone, bone },
            { Sounds.SludgeToss, sludgeToss },
            { Sounds.SludgeHit, sludgeHit },
            { Sounds.SludgePuddle, sludgePuddle },
            { Sounds.Capture, capture },
            { Sounds.CaptureLoss, captureLoss },
            { Sounds.PlantGrowth, plantGrowth },
            { Sounds.StretchFlesh, stretchFlesh },
            { Sounds.Bubbles, bubbles },
            { Sounds.Pop, pop },
            { Sounds.Start, start },
            { Sounds.FireHit, fireHit },
            { Sounds.FireSpawn, fireSpawn },
            { Sounds.Pause, pause },
            { Sounds.MushroomGrow, shroomGrow },
            { Sounds.VineLash, vineLash },
            { Sounds.Bump, bump },
            { Sounds.Spawn, spawn },
        };
    }

    void Update() {
        for(int i = activeSounds.Count - 1; i >= 0; i--) {
            if(!activeSounds[i].isPlaying) {
                Destroy(activeSounds[i]);
                activeSounds.RemoveAt(i);
            }
        }
    }

    public void PlaySound(Sounds sound, float pitch = 1f) {
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        activeSounds.Add(audio);
        audio.clip = soundClips[sound];
        audio.pitch = pitch;
        audio.Play();
    }

    public void PlayWalkSound() {
        AudioClip sound = walks[Random.Range(0, walks.Length - 1)];
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        activeSounds.Add(audio);
        audio.clip = sound;
        audio.Play();
    }

    public void StopSound(Sounds sound) {
        int index = activeSounds.FindIndex((AudioSource source) => source.clip == soundClips[sound]);
        if(index < 0) {
            return;
        }
        activeSounds[index].Stop();
        activeSounds.RemoveAt(index);
    }
}
