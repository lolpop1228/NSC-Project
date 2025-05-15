using UnityEngine;

public class RestorationTool : MonoBehaviour
{
    public float range = 10f;
    public Camera fpsCam;
    public float requiredHoldTime = 2.0f;
    public Color beamColor = Color.green;
    public float beamWidth = 0.05f;
    public Transform gunTip;
    
    [Tooltip("Optional - if not assigned, a LineRenderer will be created automatically")]
    public LineRenderer lineRenderer;
    
    private float currentHoldTime = 0f;
    private bool isHolding = false;
    private Plantable targetPlantable = null;
    private Vector3 hitPoint;
    
    public AudioSource audioSource;
    public AudioClip beamSound;

    void Awake()
    {
        if (gunTip == null)
        {
            Debug.LogWarning("Gun tip transform not assigned! Using camera position as fallback.");
            GameObject tipObj = new GameObject("DefaultGunTip");
            gunTip = tipObj.transform;
            gunTip.SetParent(fpsCam.transform);
            gunTip.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
        }

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = beamWidth;
            lineRenderer.endWidth = beamWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = beamColor;
            lineRenderer.endColor = beamColor;
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.clip = beamSound;
    }

    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit hit;

            if (!isHolding)
            {
                if (Physics.Raycast(ray, out hit, range))
                {
                    targetPlantable = hit.collider.GetComponent<Plantable>();
                    if (targetPlantable != null)
                    {
                        isHolding = true;
                        currentHoldTime = 0f;
                        hitPoint = hit.point;

                        lineRenderer.enabled = true;
                        lineRenderer.SetPosition(0, gunTip.position);
                        lineRenderer.SetPosition(1, hitPoint);

                        // Play beam sound
                        if (!audioSource.isPlaying)
                            audioSource.Play();
                    }
                }
            }
            else if (targetPlantable != null)
            {
                currentHoldTime += Time.deltaTime;

                if (Physics.Raycast(ray, out hit, range))
                {
                    if (hit.collider.GetComponent<Plantable>() == targetPlantable)
                    {
                        hitPoint = hit.point;
                    }
                }

                lineRenderer.SetPosition(0, gunTip.position);
                lineRenderer.SetPosition(1, hitPoint);

                if (currentHoldTime >= requiredHoldTime)
                {
                    targetPlantable.Restore();
                    ResetHoldState();
                }
            }
        }
        else if (isHolding)
        {
            ResetHoldState();
        }
    }

    private void ResetHoldState()
    {
        isHolding = false;
        currentHoldTime = 0f;
        targetPlantable = null;
        lineRenderer.enabled = false;

        // Stop beam sound
        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}
