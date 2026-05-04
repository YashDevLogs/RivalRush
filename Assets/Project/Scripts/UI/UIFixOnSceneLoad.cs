using UnityEngine;
using UnityEngine.EventSystems;

public class UIFixOnSceneLoad : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogError("❌ Missing EventSystem in scene");
        }
    }
}