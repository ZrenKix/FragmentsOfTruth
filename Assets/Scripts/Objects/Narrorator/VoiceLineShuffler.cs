using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class VoiceLineShuffler : MonoBehaviour
{
    [SerializeField] private AudioClip[] m_voiceLines;
    [SerializeField] private float minTimeBetweenVoiceLines = 30f;
    [SerializeField] private float maxTimeBetweenVoiceLines = 55f;

    private AudioManager m_audioManager;

    private List<AudioClip> m_shuffledVoiceLines;
    private int m_currentIndex = 0;

    private bool isShuffling = false;

    private void Start()
    {
        m_audioManager = GetComponent<AudioManager>();

        // Initiera den blandade listan med röstlinjer
        m_shuffledVoiceLines = new List<AudioClip>(m_voiceLines);

        // Blanda listan asynkront
        ShuffleListAsync(m_shuffledVoiceLines);

        // Starta koroutinen för att spela röstlinjer
        StartCoroutine(PlayVoiceLines());
    }

    private void Update()
    {
        if (isShuffling)
        {
            // Kör inte Update-koden medan shuffling pågår
            return;
        }

        // Placera eventuell Update-logik här
    }

    private async void ShuffleListAsync<T>(List<T> list)
    {
        isShuffling = true;

        await Task.Run(() =>
        {
            System.Random rng = new System.Random();

            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int r = rng.Next(i, list.Count);

                list[i] = list[r];
                list[r] = temp;
            }
        });

        isShuffling = false;
    }

    private IEnumerator PlayVoiceLines()
    {
        while (true)
        {
            // Vänta tills shuffling är klar
            while (isShuffling)
            {
                yield return null;
            }

            // Spela den aktuella röstlinjen
            AudioClip clip = m_shuffledVoiceLines[m_currentIndex];
            m_audioManager.PlayVoiceLine(clip);

            // Öka indexet
            m_currentIndex++;

            // Om vi har nått slutet av listan, blanda om och återställ indexet
            if (m_currentIndex >= m_shuffledVoiceLines.Count)
            {
                m_currentIndex = 0;
                // Blanda listan asynkront
                ShuffleListAsync(m_shuffledVoiceLines);
            }

            // Vänta en slumpmässig tid mellan min och max
            float waitTime = Random.Range(minTimeBetweenVoiceLines, maxTimeBetweenVoiceLines);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
