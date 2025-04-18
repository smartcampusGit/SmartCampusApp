using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;

public class CampusesList : MonoBehaviour
{
    [Header("UI")]
    public GameObject campusPrefab;
    public Transform contentParent;
    public TMP_InputField searchInput;

    private FirebaseFirestore db;
    private List<GameObject> campusItems = new List<GameObject>();

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                LoadCampuses();
            }
            else
            {
                Debug.LogError("Firebase not ready: " + task.Result);
            }
        });

        searchInput.onValueChanged.AddListener(FilterList);
    }

    void LoadCampuses()
    {
        db.Collection("Campuses").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to load campuses: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Dictionary<string, object> data = doc.ToDictionary();

                if (data.ContainsKey("name"))
                {
                    string campusName = data["name"].ToString();
                    GameObject go = Instantiate(campusPrefab, contentParent);
                    TextMeshProUGUI textComponent = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null)
                        textComponent.text = campusName;

                    campusItems.Add(go);
                }
            }
        });
    }

    void FilterList(string searchText)
    {
        searchText = searchText.ToLower();

        foreach (GameObject item in campusItems)
        {
            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
            bool shouldShow = text != null && text.text.ToLower().Contains(searchText);
            item.SetActive(shouldShow);
        }
    }
}
