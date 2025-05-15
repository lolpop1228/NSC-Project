using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerDisplay : MonoBehaviour
{
    public TextMeshProUGUI timerText;             // Assign this in the Inspector
    public float timerDuration = 60f;  // Set the starting countdown time (in seconds)
    public GameObject failScreen;

    private float timer;
    private bool timerActive = true;

    void Start()
    {
        timer = timerDuration;
        UpdateTimerDisplay(timer);
        if (failScreen != null)
        {
            failScreen.SetActive(false);
        }
    }

    void Update()
    {
        if (!timerActive) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            timerActive = false;
            OnTimerEnd(); // Call custom method when time ends
        }

        UpdateTimerDisplay(timer);
    }

    void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000);

        timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    void OnTimerEnd()
    {
        Debug.Log("Timer finished!");
        if (failScreen != null)
        {
            failScreen.SetActive(true);
        }
    }
}
