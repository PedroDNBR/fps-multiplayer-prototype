using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomSoundEffect : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] audios;

    public Transform TransformPosition;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if(audioSource)
        {
            audioSource.clip = audios[Random.Range(0, audios.Length - 1)];
            audioSource.Play();
        }
        else
        {
            AudioSource.PlayClipAtPoint(audios[Random.Range(0, audios.Length - 1)], TransformPosition.position);
        }
    }
}
