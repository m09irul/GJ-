using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class AudioManager : MonoBehaviour
{
    public Sounds[] sounds;

    public AudioClip gameOverClip;

    public static AudioManager instance;

    string[] level_music_names = {AllStringConstant.LEVEL_BG_1, AllStringConstant.LEVEL_BG_2, AllStringConstant.LEVEL_BG_3,
        AllStringConstant.LEVEL_BG_4, AllStringConstant.LEVEL_BG_5, AllStringConstant.LEVEL_BG_6, AllStringConstant.LEVEL_BG_7 };

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        //DontDestroyOnLoad(gameObject);

        foreach(Sounds s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.outputAudioMixerGroup = s.masterMixtureGroup;
            s.source.pitch = s.pinch;
            s.source.loop = s.loop;
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
            play(AllStringConstant.MAIN_BG_MUSIC);
        else
        {
            int r = UnityEngine.Random.Range(0, level_music_names.Length);

            play(level_music_names[r]);
        }
    }

    public void play(string name)
    {
        Sounds s = Array.Find(sounds, sound => sound.name == name);
        if(s==null)
        {
            Debug.LogWarning("sound: " + name + " not found");
            return;
        }
        s.source.Play();
    }

    public void stop(string name)
    {
        Sounds s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("sound: " + name + " not found");
            return;
        }
        s.source.Stop();
    }

}
