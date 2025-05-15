using UnityEngine;

public class Meteor : MonoBehaviour
{
    [Header("Movement")]
    public float fallSpeed = 20f;

    [Header("Explosion")]
    public float explosionRadius = 5f;
    public int damage = 50;
    public GameObject explosionEffect;
    public AudioClip explosionSound;

    [Header("Settings")]
    public LayerMask damageMask; // What layers take damage

    private bool hasExploded = false;

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        hasExploded = true;

        Explode();
    }

    void Explode()
    {
        // Visual & sound effects
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        if (explosionSound != null)
        {
            GameObject audioGO = new GameObject("Explosion Sound");
            AudioSource audioSource = audioGO.AddComponent<AudioSource>();
            audioSource.clip = explosionSound;
            audioSource.spatialBlend = 1f;
            audioSource.Play();
            Destroy(audioGO, 3f);
        }

        // Damage nearby objects
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, damageMask);
        foreach (Collider hit in hits)
        {
            PlayerHealth target = hit.GetComponent<PlayerHealth>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
