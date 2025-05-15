using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionFail : MonoBehaviour
{
    public MonoBehaviour[] scriptToDisable;

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
}
