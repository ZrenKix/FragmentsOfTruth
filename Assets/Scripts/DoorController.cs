using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public void OpenDoor()
    {
        Debug.Log("Door is disappearing!");
        gameObject.SetActive(false);  // Disable the door to make it disappear
    }
}