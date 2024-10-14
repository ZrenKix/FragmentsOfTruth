using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDoor : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] AudioClip clip;
    // Start is called before the first frame update
    void Start()
    {
        if(!source) source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player")) { PlayAudio(); }
    }

    void PlayAudio()
    {
        source.clip = clip;
        source.PlayOneShot(clip);
    }
}
