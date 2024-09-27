using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStove : MonoBehaviour, IInteractable
{
    //Av Linn Li


    [SerializeField] bool isBurning = true;
    [SerializeField] private AudioClip fireExtinguishAC;
    [SerializeField] private AudioSource fireStoveAS;

    private Bucket bucketScript;

    public string InteractionPrompt { get; }

    // Start is called before the first frame update
    void Start()
    {
        bucketScript = FindObjectOfType<Bucket>();
        fireStoveAS.clip = fireExtinguishAC;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Interact(Interactor interactor)
    {
        if(HasWaterBucket() == true)
        {
            PutOutFire();
            return true;
        }
        else
        {
            //play detective voice line that says there's something in the firestove, but the fire is in the way
            return false;
        }
        
    }

    private bool HasWaterBucket()
    {
        return bucketScript.hasWater;
    }

    private void PutOutFire()
    {
        isBurning = false;
        fireStoveAS.Play();
    }
}
