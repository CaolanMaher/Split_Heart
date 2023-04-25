using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BackgroundMusicFade : MonoBehaviour
{

    float duration = 2f;
    float targetMusicVolume;
    float targetSFXVolume;

    AudioSource audioSource;

    public AudioClip bossclip;

    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;

    [SerializeField] AudioMixer backgroundAudio;
    [SerializeField] AudioMixer sfxAudio;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //audioSource.volume = 0;
        backgroundAudio.SetFloat("music", 0);

        //PlayerPrefs.SetFloat("MusicVolume", 1);
        //PlayerPrefs.SetFloat("SFXVolume", 1);

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            targetMusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        }
        else
        {
            targetMusicVolume = 1;
        }

        musicVolumeSlider.value = targetMusicVolume;

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            targetSFXVolume = PlayerPrefs.GetFloat("SFXVolume");
        }
        else
        {
            targetSFXVolume = 1;
        }

        sfxVolumeSlider.value = targetSFXVolume;

        StartCoroutine(StartFade());
    }

    IEnumerator StartFade()
    {
        //audioSource.volume = 0;
        backgroundAudio.SetFloat("music", 0);
        float currentTime = 0;
        //float start = audioSource.volume;
        float start;
        bool result = backgroundAudio.GetFloat("music", out start);
        if (result)
        {
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                //audioSource.volume = Mathf.Lerp(start, targetMusicVolume, currentTime / duration);
                backgroundAudio.SetFloat("music", Mathf.Log10(Mathf.Lerp(start, targetMusicVolume, currentTime / duration)) * 20);
                yield return null;
            }
        }
        yield break;
    }

    public void FadeOut()
    {
        StartCoroutine(StartFadeOut());
    }

    public IEnumerator StartFadeOut()
    {
        float currentTime = 0;
        //float start = audioSource.volume;
        float start;
        bool result = backgroundAudio.GetFloat("music", out start);
        if (result)
        {
            while (currentTime < 2f)
            {
                currentTime += Time.deltaTime;
                //audioSource.volume = Mathf.Lerp(start, 0, currentTime / 2f);
                backgroundAudio.SetFloat("music", Mathf.Lerp(start, -80f, currentTime / 2f));
                yield return null;
            }
        }
        yield break;
    }

    public void ChangeMusicVolume()
    {
        //AudioListener.volume = volumeSlider.value;
        backgroundAudio.SetFloat("music", Mathf.Log10(musicVolumeSlider.value) * 20);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
    }

    public void ChangeSFXVolume()
    {
        //AudioListener.volume = volumeSlider.value;
        sfxAudio.SetFloat("sfx", Mathf.Log10(sfxVolumeSlider.value) * 20);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
    }

    public void StartBossFight()
    {
        StartCoroutine(BossFightMusic());
    }

    IEnumerator BossFightMusic()
    {
        StartCoroutine(StartFadeOut());

        yield return new WaitForSeconds(2f);

        audioSource.clip = bossclip;
        audioSource.Play();

        StartCoroutine(StartFade());
    }
}
