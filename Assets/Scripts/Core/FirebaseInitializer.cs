using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Extensions; // Needed for ContinueWithOnMainThread

public class FirebaseInitializer : MonoBehaviour
{
    public static bool IsReady { get; private set; } = false;

    public static FirebaseAuth Auth;
    public static FirebaseFirestore Firestore;
    public static FirebaseStorage Storage;

    void Awake()
    {
        Debug.Log("FirebaseInitializer Awake called");
        DontDestroyOnLoad(gameObject);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.Exception != null)
            {
                Debug.LogError("Firebase dependency check failed: " + task.Exception);
                return;
            }

            var dependencyStatus = task.Result;
            
            if (dependencyStatus == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                Firestore = FirebaseFirestore.DefaultInstance;
                Storage = FirebaseStorage.DefaultInstance;

                IsReady = true;
                Debug.Log("Firebase initialized and ready!");
            }
            else
            {
                Debug.LogError("Firebase dependencies missing: " + dependencyStatus);
            }
        });
    }



}
