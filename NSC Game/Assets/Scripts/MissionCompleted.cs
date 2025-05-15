using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionCompleted : MonoBehaviour
{
    public MonoBehaviour[] scriptToDisable;
    public string sceneToLoad;

    void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        foreach (MonoBehaviour script in scriptToDisable)
        {
            if (script != null)
            {
                script.enabled = false;
            }
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Continue()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
