using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit : MonoBehaviour
{
    public AudioClip[] HitAudios;

    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = HitAudios[Random.Range(0, HitAudios.Length)];

        audioSource.Play();
    }
}
