using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicFade : MonoBehaviour
{

    public float duration;
    public float targetVolume;

    AudioSource audioSource;

    public AudioClip bossclip;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0;

        StartCoroutine(StartFade());
    }

    IEnumerator StartFade()
    {
        audioSource.volume = 0;
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
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
        float start = audioSource.volume;
        while (currentTime < 2f)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, 0, currentTime / 2f);
            yield return null;
        }
        yield break;
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
