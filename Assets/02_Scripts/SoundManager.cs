using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource[] destroyNoise;

    public AudioSource BGM;

    public void PlayRandomDestoryNoise()
    {
        //Choos a random number
        int clipToPlay = 0;//Random.Range(0, destroyNoise.Length);
        //play that clip
        destroyNoise[clipToPlay].Play();
    }

    public void playBGM()
    {
        BGM.Play();
    }
}
