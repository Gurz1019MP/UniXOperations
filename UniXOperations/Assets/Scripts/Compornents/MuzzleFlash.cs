using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject Effect;
    public AudioSource AudioSource;
    public float EffectdestroyTime;
    public float DestroyTime;

    public AudioClip FireAudio { get; set; }

    void Start()
    {
        Destroy(Effect, EffectdestroyTime);
        Destroy(gameObject, DestroyTime);

        AudioSource.clip = FireAudio;
        AudioSource.Play();
    }
}
