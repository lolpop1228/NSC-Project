using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int totalZones;
    public int restoredZones = 0;
    public Slider restorationSlider;
    public GameObject completeScreen;

    void Awake() => instance = this;

    public void AddRestorationPoint()
    {
        restoredZones++;
        restorationSlider.value = (float)restoredZones / totalZones;

        if (restoredZones >= totalZones)
        {
            Debug.Log("Forest fully restored!");
            // Trigger next level, cutscene, etc.
            if (completeScreen != null)
            {
                completeScreen.SetActive(true);
            }
        }
    }
}
