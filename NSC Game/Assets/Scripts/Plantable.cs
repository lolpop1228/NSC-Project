using UnityEngine;

public class Plantable : MonoBehaviour
{
    [Header("Tree Prefabs to Choose From")]
    public GameObject[] treePrefabs;

    [Header("Tree Spawn Settings")]
    public int numberOfTrees = 5;
    public float maxSpreadRadius = 5f;
    public GameObject dirtParticle;

    [Header("Audio")]
    public AudioClip restoreSound;

    private bool isRestored = false;

    public void Restore()
    {
        if (isRestored || treePrefabs.Length == 0) return;

        // Play 2D sound using a temporary AudioSource GameObject
        if (restoreSound != null)
        {
            GameObject audioGO = new GameObject("2D Restore Sound");
            AudioSource audioSource = audioGO.AddComponent<AudioSource>();

            audioSource.clip = restoreSound;
            audioSource.volume = 1f;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.Play();

            Destroy(audioGO, 2f); // Destroy after playback
        }

        // Spawn trees
        for (int i = 0; i < numberOfTrees; i++)
        {
            int index = Random.Range(0, treePrefabs.Length);
            GameObject chosenTree = treePrefabs[index];
            Vector3 randomOffset = Random.insideUnitSphere * maxSpreadRadius;
            randomOffset.y = 0f;

            Instantiate(chosenTree, transform.position + randomOffset, Quaternion.identity);

            if (dirtParticle != null)
            {
                GameObject particle = Instantiate(dirtParticle, transform.position + randomOffset, Quaternion.identity);
                Destroy(particle, 2f);
            }
        }

        isRestored = true;
        gameObject.SetActive(false); // Hide this object (like stump or dirt)
        GameManager.instance.AddRestorationPoint();
    }
}
