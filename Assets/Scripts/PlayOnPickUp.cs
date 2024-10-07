using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playAudioOnInteract : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip clip;
    [SerializeField] Key keyItem;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        clip = GetComponent<AudioClip>();
        keyItem = GetComponent<Key>();
        source.clip = clip;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool hasBeenPickedUp()
    {
        

        return false;
    }


}
