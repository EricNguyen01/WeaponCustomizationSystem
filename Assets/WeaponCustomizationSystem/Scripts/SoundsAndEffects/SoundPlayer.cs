using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private bool disableSoundPlay = false;
    [SerializeField] private bool playSoundOnEnable = false;
    [SerializeField] private AudioClip audioClipToPlay;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        if (audioClipToPlay == null) Debug.LogWarning("AudioClipToPlay is not assigned on sound player component: " + name);
    }

    private void OnEnable()
    {
        if (playSoundOnEnable)
        {
            if (disableSoundPlay || audioClipToPlay == null) return;
            audioSource.PlayOneShot(audioClipToPlay);
        }
    }

    public void SetAudioClipToPlay(AudioClip clip)
    {
        audioClipToPlay = clip;
    }

    public void PlayAudio()
    {
        if (disableSoundPlay || audioClipToPlay == null) return;

        if (!audioSource.isPlaying) audioSource.PlayOneShot(audioClipToPlay);
        else StartCoroutine(PlayAfterLastAudioFinishedPlaying());

    }

    public void StopAudio()
    {
        StopCoroutine(PlayAfterLastAudioFinishedPlaying());
        audioSource.Stop();
    }

    public void DisableSoundPlay(bool disabled)
    {
        disableSoundPlay = disabled;
        if (!disableSoundPlay) StopAudio();
    }

    private IEnumerator PlayAfterLastAudioFinishedPlaying()
    {
        yield return new WaitUntil(() => !audioSource.isPlaying);
        audioSource.PlayOneShot(audioClipToPlay);
    }
}
