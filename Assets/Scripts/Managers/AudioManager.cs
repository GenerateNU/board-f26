using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // so everything else can grab it via AudioManager.Instance
    public static AudioManager Instance { get; private set; }

    // these 3 should be adjustable in settings later (managers from top to bottom: sfx, music, ambience)
    [Header("AUDIO SOURCES")]
    [SerializeField] private AudioSource sfxSource; //for sfx, can overlap (Play on Awake, DOESN'T LOOP)
    [SerializeField] private AudioSource musicSource; //for music, loops
    [SerializeField] private AudioSource ambienceSource; //ambience, also loops

    [Header("LOOPABLE ONES")]
    [SerializeField] private AudioClip AmbianceLoop; //currently placeholder 40s loop
    [SerializeField] private AudioClip BGMLoop; //currently placeholder royalty free smooth jazz from youtube

    [Header("FROG SFX")]
    [SerializeField] private AudioClip[] hopSFX; //LIST BC theres 4 variants
    [SerializeField] private AudioClip[] landSFX; //LIST BC theres 3 variants
    [SerializeField] private AudioClip croakSFX; //1 variant
    [SerializeField] private AudioClip layEggSFX; //1 variant
    [SerializeField] private AudioClip eggDestroyedSound; //1 variant

    [Header("EVIL GUYS SFX")]
    [SerializeField] private AudioClip fishCronchSFX; //1 variant; plays on kill
    [SerializeField] private AudioClip fishMove1SFX; //1 variant 
    [SerializeField] private AudioClip fishMove2SFX; //1 variant 
    [SerializeField] private AudioClip alliWokeSFX; //PLACEHOLDER; plays when alligator is awoken
    [SerializeField] private AudioClip alliSleepSFX; //PLACEHOLDER; plays when alligator falls back asleep
    //need to determine if chomp is same SFX as egg eaten or fish eaten or if it's the same (based on if gators have anim)

    [Header("PICKUPS ALSO TURTLES SFX")]
    [SerializeField] private AudioClip flyCollectSFX; //1 variant 
    [SerializeField] private AudioClip flyScaredSFX; //1 variant 
    [SerializeField] private AudioClip turtleMoveSFX; //PLACEHOLDER; no SFX yet
    [SerializeField] private AudioClip turtleWakeSFX; //PLACEHOLDER; no SFX yet

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip turnChangeSound; //PLACEHOLDER; no SFX yet
    //TO ADD: newspaper sfx

    //quick safety thing
    private void Awake()
    {
        // if theres any more audio sources, kill it
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // START POND AND BGM immediately when level starts
    private void Start()
    {
        if (AmbianceLoop != null)
        {
            PlayAmbience(AmbianceLoop, 0.4f); //0.4 as default rn
        }

        if (BGMLoop != null)
        {
            PlayMusic(BGMLoop, 0.5f); //0.5 as default rn
        }
    }


    //plays audio clip and also its random
    public void PlaySFX(AudioClip clip, float volume = 1f, bool randomizePitch = true) //default 1f vol, pitch gets +/- 5% shifted
    {
        if (clip == null || sfxSource == null) return; //safety

        // 5% clamped range pitch shift so it sounds less repetitive
        sfxSource.pitch = randomizePitch ? Random.Range(0.95f, 1.05f) : 1f;
        sfxSource.PlayOneShot(clip, volume);
    }

    //picks rando SFX from the array
    public void PlayRandomSFX(AudioClip[] clips, float volume = 1f, bool randomizePitch = true)
    {
        if (clips == null || clips.Length == 0) return; //safety
        AudioClip selected = clips[Random.Range(0, clips.Length)];
        PlaySFX(selected, volume, randomizePitch); //calls above method
    }

    //loops ambiance
    public void PlayAmbience(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null || ambienceSource == null) return; //safety
        ambienceSource.clip = clip;
        ambienceSource.volume = volume;
        ambienceSource.loop = true;
        ambienceSource.Play();
    }

    //loops music
    public void PlayMusic(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null || musicSource == null) return; //safety
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    //TO DO: make a quick method that plays SFX while dampening/stopping the BGM (not ambience)
    //for stuff like fish kill and winning/losing

    //MACRO 1 LINERS TO CALL BELOW

/*
    if u want to add more:
- 1. define a [SerializeField] above
- 2. add a 1 liner below
- 3. drag the clip into the audio manager
- 4. call the 1 liner somewhere

        public void PlayHop()
        {
            PlayRandomSFX(hopSFX);
        }

- in other words, => is { ... }

- call using AudioManager.Instance?.PlayHop();

^ which is a short way to check if AudioManager.Instance exists,
then PlayRandomSFX checks if the hopSFX array actually exists,
then it does Random.Range for pitch shifting,
then PlaySFX checks if chosen file/speaker actually exists,
then it plays the SFX

*/

    //frog
    public void PlayHop() => PlayRandomSFX(hopSFX);
    public void PlayLand() => PlayRandomSFX(landSFX);
    public void PlayCroak() => PlaySFX(croakSFX, 1.2f, false); // false bc it shouldnt pitch shift; 1.2x to test loudness
    public void PlayLayEgg() => PlaySFX(layEggSFX);
    public void PlayEggDestroyed() => PlaySFX(eggDestroyedSound);

    //fishe
    public void PlayFishChomp() => PlaySFX(fishCronchSFX, 1.3f, false);
    public void PlayFishMove1() => PlaySFX(fishMove1SFX);
    public void PlayFishMove2() => PlaySFX(fishMove2SFX);

    //alligators
    public void PlayAlligatorAngry() => PlaySFX(alliWokeSFX);
    public void PlayAlligatorSleep() => PlaySFX(alliSleepSFX);

    //flies
    public void PlayFlyCollect() => PlaySFX(flyCollectSFX);
    public void PlayFlyScared() => PlaySFX(flyScaredSFX);

    //turtles
    public void PlayTurtleMove() => PlaySFX(turtleMoveSFX);
    public void PlayTurtleWake() => PlaySFX(turtleWakeSFX);

    //UI stuff
    public void PlayButtonClick() => PlaySFX(buttonClickSound, 0.8f, false);
    public void PlayTurnChange() => PlaySFX(turnChangeSound, 0.7f, false);

    //for when we have a settings screen
    public void SetSFXVolume(float volume) => sfxSource.volume = Mathf.Clamp01(volume);
    public void SetMusicVolume(float volume) => musicSource.volume = Mathf.Clamp01(volume);
    public void SetAmbienceVolume(float volume) => ambienceSource.volume = Mathf.Clamp01(volume);
}
