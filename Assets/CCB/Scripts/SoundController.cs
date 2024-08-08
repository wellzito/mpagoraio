using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController instance;

    public AudioSource soundRPC;

    public AudioClip countDown;
    public AudioClip whistle;
    public AudioClip goal;
    public AudioClip loss;
    public AudioClip fan;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 0 = countDown, 1 = whistle, 2 = goal, 3 = loss, 4 = fan
    /// </summary>
    public void SoundType(int num)
    {
        switch (num)
        {
            case 0:
                soundRPC.PlayOneShot(countDown);
                break;
            case 1:
                soundRPC.PlayOneShot(whistle);
                break;
            case 2:
                soundRPC.PlayOneShot(goal);
                break;
            case 3:
                soundRPC.PlayOneShot(loss);
                break;
            case 4:
                soundRPC.PlayOneShot(fan);
                break;
        }
    }
}
