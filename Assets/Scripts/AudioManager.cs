using UnityEngine.Audio;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public Sounds[] sounds;
    public AudioMixerGroup masterMixtureGroup;

    public static AudioManager instance;

    private Dictionary<string, Coroutine> fadeRoutines = new();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sounds s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.outputAudioMixerGroup = masterMixtureGroup;
            s.source.pitch = s.pinch;
            s.source.loop = s.loop;
        }
    }

    // =======================
    // PLAY
    // =======================

    public void play(string name, float fadeDuration = 0f)
    {
        Sounds s = GetAudio(name);
        if (s == null) return;

        StopFade(name);

        if (fadeDuration <= 0f)
        {
            s.source.volume = s.volume;
            s.source.Play();
        }
        else
        {
            fadeRoutines[name] = StartCoroutine(FadeInRoutine(s, fadeDuration));
        }
    }

    // =======================
    // STOP
    // =======================

    public void stop(string name, float fadeDuration = 0f)
    {
        Sounds s = GetAudio(name);
        if (s == null) return;

        StopFade(name);

        if (fadeDuration <= 0f)
        {
            s.source.Stop();
        }
        else
        {
            fadeRoutines[name] = StartCoroutine(FadeOutRoutine(s, fadeDuration));
        }
    }

    // =======================
    // CORE
    // =======================

    public Sounds GetAudio(string name)
    {
        Sounds s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            Debug.LogWarning("Sound: " + name + " not found");

        return s;
    }

    // =======================
    // FADE ROUTINES
    // =======================

    private IEnumerator FadeInRoutine(Sounds s, float duration)
    {
        s.source.volume = 0f;

        if (!s.source.isPlaying)
            s.source.Play();

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            s.source.volume = Mathf.Lerp(0f, s.volume, t / duration);
            yield return null;
        }

        s.source.volume = s.volume;
        fadeRoutines.Remove(s.name);
    }

    private IEnumerator FadeOutRoutine(Sounds s, float duration)
    {
        float startVolume = s.source.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            s.source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        s.source.Stop();
        s.source.volume = s.volume; // reset
        fadeRoutines.Remove(s.name);
    }

    private void StopFade(string name)
    {
        if (fadeRoutines.TryGetValue(name, out Coroutine routine))
        {
            StopCoroutine(routine);
            fadeRoutines.Remove(name);
        }
    }
}
