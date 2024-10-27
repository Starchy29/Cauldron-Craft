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
    Heal,
    Victory
}

public class SoundManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip menuSong;
    [SerializeField] private AudioClip battleSong;
    [SerializeField] private AudioClip winStinger;

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
    [SerializeField] private AudioClip heal;
    [SerializeField] private AudioClip[] walks;

    public const float MAX_VOL = 0.2f;
    public static float volPercent;

    public static SoundManager Instance { get; private set; }

    private static Dictionary<Sounds, AudioClip> soundClips;

    private AudioSource menuSongPlayer;
    private AudioSource battleSongPlayer;

    private enum Fader {
        None,
        Menu,
        Battle
    }
    private Fader fading;

    private List<AudioSource> activeSounds = new List<AudioSource>();

    void Awake() {
        if(Instance != null) {
            Destroy(gameObject);
            return;
        }
        volPercent = VolumeSlider.START_PERCENT;
        AudioListener.volume = volPercent * SoundManager.MAX_VOL;
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
            { Sounds.Heal, heal },
            { Sounds.Victory, winStinger }
        };

        menuSongPlayer = gameObject.AddComponent<AudioSource>();
        menuSongPlayer.clip = menuSong;
        menuSongPlayer.loop = true;

        battleSongPlayer = gameObject.AddComponent<AudioSource>();
        battleSongPlayer.clip = battleSong;
        battleSongPlayer.loop = true;

        PlaySong(true);
    }

    void Update() {
        for(int i = activeSounds.Count - 1; i >= 0; i--) {
            if(!activeSounds[i].isPlaying) {
                Destroy(activeSounds[i]);
                activeSounds.RemoveAt(i);
            }
        }

        if(fading != Fader.None) {
            AudioSource fader = fading == Fader.Menu ? menuSongPlayer : battleSongPlayer;
            fader.volume -= Time.deltaTime / 2f;
            if(fader.volume <= 0f) {
                fader.volume = 0f;
                fader.Stop();
                fading = Fader.None;
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

    public void PlaySong(bool menu) {
        AudioSource chosen = menu ? menuSongPlayer : battleSongPlayer;
        chosen.volume = 1f;
        chosen.Play();
    }

    public void StopSong(bool menu, bool immediate = false) {
        if(fading != Fader.None) {
            AudioSource fader = fading == Fader.Menu ? menuSongPlayer : battleSongPlayer;
            fader.Stop();
            fader.volume = 0f;
        }

        if(immediate) {
            AudioSource stopper = menu ? menuSongPlayer : battleSongPlayer;
            stopper.Stop();
        } else {
            fading = menu ? Fader.Menu : Fader.Battle;
        }
    }
}
