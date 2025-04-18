using UnityEngine;

public class SessionData : MonoBehaviour
{
    public static SessionData Instance { get; private set; }

    public string CampusId { get; set; }

    private void Awake()
    {
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
}