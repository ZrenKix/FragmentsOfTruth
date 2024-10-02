using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonar : MonoBehaviour
{
    [SerializeField] private AudioClip m_audioClip;
    [SerializeField] private AudioSource m_audioSource;

    [SerializeField] private float m_rayMaxLength = 10f;
    [SerializeField] private float m_minPingInterval = 0.2f;  // Minimum time between pings
    [SerializeField] private float m_maxPingInterval = 2f;    // Maximum time between pings

    [SerializeField] private AnimationCurve pingIntervalCurve; // Customizable curve in the editor

    private int count = 0;
    private float m_nextPingTime = 0f;  // Time when the next ping will occur

    private void Start()
    {
        m_audioSource.clip = m_audioClip;
    }

    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.P))
        {
            count++;
            if (count > 9)
            {
                count = 0;
            }
            Debug.Log(count);
        }


        Vector3 rayDirection = new Vector3(0.5f, 0.5f, 0);
        Ray ray = Camera.main.ViewportPointToRay(rayDirection);
        Debug.DrawRay(ray.origin, ray.direction * m_rayMaxLength, Color.red);
        RaycastHit hit;

        if(count%2 == 0)
        {

            if (Physics.Raycast(ray, out hit, m_rayMaxLength))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    // Normalize the distance between 0 (far) and 1 (close)
                    float normalizedDistance = Mathf.InverseLerp(m_rayMaxLength, 0, hit.distance);

                    // Evaluate the ping interval from the animation curve
                    float curveValue = pingIntervalCurve.Evaluate(normalizedDistance);

                    // Use the curve value to control the ping interval
                    float pingInterval = Mathf.Lerp(m_maxPingInterval, m_minPingInterval, curveValue);

                    // Check if it's time to play the next ping sound
                    if (Time.time >= m_nextPingTime)
                    {
                        m_audioSource.PlayOneShot(m_audioClip);  // Play the ping sound
                        m_nextPingTime = Time.time + pingInterval;  // Set the next time the ping sound should play
                    }

                    Debug.DrawRay(ray.origin, ray.direction * m_rayMaxLength, Color.green);
                }
                else
                {
                    m_audioSource.Stop();  // Stop the audio if no interactable is hit
                }
            }
            else
            {
                m_audioSource.Stop();  // Stop the audio if no hit is detected
            }

        }
    }
}
