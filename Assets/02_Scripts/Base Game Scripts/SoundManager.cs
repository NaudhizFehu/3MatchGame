using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager soundManager = null;

    public static SoundManager instance
    {
        get
        {
            if(soundManager == null)
            {
                SoundManager tmpinst = FindObjectOfType(typeof(SoundManager)) as SoundManager;
                soundManager = tmpinst;
            }

            return soundManager;
        }
    }

    public AudioSource[] destroyNoise;

    public AudioSource backgroundMusic;

    public void PlayRandomDestoryNoise()
    {
        if(PlayerPrefs.HasKey("Sound"))
        {
            if(PlayerPrefs.GetInt("Sound") == 1)
            {
                //Choos a random number
                int clipToPlay = 0;//Random.Range(0, destroyNoise.Length);
                                   //play that clip
                destroyNoise[clipToPlay].Play();
            }
        }
        else
        {
            //Choos a random number
            int clipToPlay = 0;//Random.Range(0, destroyNoise.Length);
                               //play that clip
            destroyNoise[clipToPlay].Play();
        }
    }

    private void Start()
    {
        if(PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                backgroundMusic.Play();
                backgroundMusic.volume = 0;
            }
            else
            {
                backgroundMusic.Play();
                backgroundMusic.volume = 1;
            }
        }
        else
        {
            backgroundMusic.Play();
            backgroundMusic.volume = 1;
        }
    }

    public void adjustVolume()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
                backgroundMusic.Stop();
            else
                backgroundMusic.Play();
        }
    }
}
