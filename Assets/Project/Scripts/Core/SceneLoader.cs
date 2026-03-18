using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void LoadMenu()
    {
        if (isLoading) return;

        StartCoroutine(LoadSceneAsync("MainMenu"));
    }


        public void LoadGame()
    {
        if (isLoading) return;
        isLoading = true;

        StartCoroutine(LoadSceneAsync("LevelPrototype"));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }
    }
}