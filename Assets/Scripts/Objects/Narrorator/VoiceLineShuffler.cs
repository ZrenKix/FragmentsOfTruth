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

        // Initiera den blandade listan med r�stlinjer
        m_shuffledVoiceLines = new List<AudioClip>(m_voiceLines);

        // Blanda listan asynkront
        ShuffleListAsync(m_shuffledVoiceLines);

        // Starta koroutinen f�r att spela r�stlinjer
        StartCoroutine(PlayVoiceLines());
    }

    private void Update()
    {
        if (isShuffling)
        {
            // K�r inte Update-koden medan shuffling p�g�r
            return;
        }

        // Placera eventuell Update-logik h�r
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
            // V�nta tills shuffling �r klar
            while (isShuffling)
            {
                yield return null;
            }

            // Spela den aktuella r�stlinjen
            AudioClip clip = m_shuffledVoiceLines[m_currentIndex];
            m_audioManager.PlayVoiceLine(clip);

            // �ka indexet
            m_currentIndex++;

            // Om vi har n�tt slutet av listan, blanda om och �terst�ll indexet
            if (m_currentIndex >= m_shuffledVoiceLines.Count)
            {
                m_currentIndex = 0;
                // Blanda listan asynkront
                ShuffleListAsync(m_shuffledVoiceLines);
            }

            // V�nta en slumpm�ssig tid mellan min och max
            float waitTime = Random.Range(minTimeBetweenVoiceLines, maxTimeBetweenVoiceLines);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
