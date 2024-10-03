using UnityEngine;

public class Sonar : MonoBehaviour
{
    [SerializeField] private AudioClip m_audioClip;           
    [SerializeField] private AudioSource m_audioSource;       
    [SerializeField] private float m_rayMaxLength = 10f;      
    [SerializeField] private float m_minPingInterval = 0.2f; 
    [SerializeField] private float m_maxPingInterval = 2f;    
    [SerializeField] private AnimationCurve pingIntervalCurve; 

    private float m_nextPingTime = 0f;
    private Camera mainCamera;

    public enum State { On, Off }
    public State state;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Växla sonarens tillstånd med Enter-knappen
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (state == State.On)
            {
                state = State.Off;
                m_audioSource.Stop();
            }
            else
            {
                state = State.On;
                m_nextPingTime = Time.time;
            }
        }

        if (state != State.On) return;

        // Raycast för att upptäcka objekt framför spelaren
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, m_rayMaxLength))
        {
            m_audioSource.Stop();
            return;
        }

        if (hit.collider.GetComponent<IInteractable>() == null)
        {
            m_audioSource.Stop();
            return;
        }

        // Beräkna ping-intervall baserat på avstånd till objektet
        float normalizedDistance = Mathf.InverseLerp(m_rayMaxLength, 0, hit.distance);
        float pingInterval = Mathf.Lerp(
            m_maxPingInterval,
            m_minPingInterval,
            pingIntervalCurve.Evaluate(normalizedDistance)
        );

        // Spela ping-ljud om det är dags
        if (Time.time >= m_nextPingTime)
        {
            m_audioSource.PlayOneShot(m_audioClip);
            m_nextPingTime = Time.time + pingInterval;
        }

        Debug.DrawRay(ray.origin, ray.direction * m_rayMaxLength, Color.green);

        // Om höger Shift är nedtryckt, spela upp förinspelat ljud baserat på objektets namn
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            // Hämta AudioSource från objektet framför
            AudioSource objectAudioSource = hit.collider.GetComponent<AudioSource>();
            if (objectAudioSource != null)
            {
                objectAudioSource.Play();  // Spela upp objektets unika ljudfil
            }
            else
            {
                Debug.LogWarning("Inget AudioSource hittades på objektet: " + hit.collider.gameObject.name);
            }
        }
    }
}
