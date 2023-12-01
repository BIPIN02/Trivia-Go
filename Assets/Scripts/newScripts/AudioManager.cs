using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AudioManager : MySingleton<AudioManager>
{
    public AudioSource musicSource;
    public AudioSource answerSource;
    public AudioSource timerSource;
    public AudioSource winningSource;
    public AudioSource buttonsound;
    public AudioClip[] musicClips;
    public AudioClip[] answerClips;
    public AudioClip[] timerClips;
    public AudioClip[] winningClips;
    public Slider musicSlider;
    public Slider audioSlider;
  
    public void SetMusic()
    {
        PlayerPrefs.SetFloat("music", musicSlider.value);
        musicSource.volume = musicSlider.value;
    }
    public void SetAudio()
    {
        PlayerPrefs.SetFloat("audio", audioSlider.value);
        answerSource.volume = audioSlider.value;
        timerSource.volume = audioSlider.value;
        winningSource.volume = audioSlider.value;
        buttonsound.volume = audioSlider.value;
    }
    private void Start()
    {
        float audio = PlayerPrefs.GetFloat("audio", 1);
        float music = PlayerPrefs.GetFloat("music", 1);
        musicSource.volume = music;
        answerSource.volume = audio;
        timerSource.volume = audio;
        winningSource.volume = audio;
        buttonsound.volume = audio;
        audioSlider.value = audio;
        musicSlider.value = music;
        PlayMusic(0);
    }
    public void PlayMusic(int id)
    {
        musicSource.clip = musicClips[id];
        musicSource.Play();
    }
    public void PlayAnswer(int id)
    {
        answerSource.clip = answerClips[id];
        answerSource.PlayOneShot(answerSource.clip);
    }
    public void PlayTimer(int id)
    {
        timerSource.clip = timerClips[id];
        timerSource.PlayOneShot(timerSource.clip);
    }
    public void PlayWinning(int id)
    {
        winningSource.clip = winningClips[id];
        winningSource.PlayOneShot(winningSource.clip);
    }
}

