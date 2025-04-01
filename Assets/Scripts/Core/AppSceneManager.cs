using UnityEngine;
using UnityEngine.SceneManagement;

public class AppSceneManager : MonoBehaviour
{
    // Singleton instance
    public static AppSceneManager Instance;

    void Awake()
    {
        // If no instance exists, set this as the instance and persist between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Loads a scene given its name
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Optionally, you can add transitions or checks here
    public void LoadSceneWithDelay(string sceneName, float delay)
    {
        StartCoroutine(LoadSceneAfterDelay(sceneName, delay));
    }

    private System.Collections.IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
