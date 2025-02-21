using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class BuildingListManager : MonoBehaviour
{
    private DatabaseReference databaseReference; // Firebase database reference

    public GameObject buildingPrefab; // Prefab for displaying building info
    public Transform contentParent;  // Parent object of the ScrollView content

    void Start()
    {
        // Initialize Firebase and resolve dependencies
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Ensure Firebase is initialized
                FirebaseApp firebaseApp = FirebaseApp.DefaultInstance;

                // Explicitly set the database URL
                firebaseApp.Options.DatabaseUrl = new System.Uri("https://smart-campus-navigation-105e9-default-rtdb.firebaseio.com/");

                Debug.Log("Firebase is ready!");

                // Get the database reference
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                // Load building data from Firebase
                LoadBuildingData();
            }
            else
            {
                Debug.LogError($"Firebase dependencies could not be resolved: {task.Result}");
            }
        });
    }

    void LoadBuildingData()
    {
        // Fetch data from the "buildings" node in Firebase
        databaseReference.Child("buildings").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                // Iterate over each building and populate the UI
                foreach (var building in snapshot.Children)
                {
                    IDictionary<string, object> buildingData = (IDictionary<string, object>)building.Value;

                    // Extract building details
                    string name = buildingData["name"].ToString();
                    string photoUrl = buildingData["photoUrl"].ToString();
                    string type = buildingData["type"].ToString();

                   AddBuildingToList(name, photoUrl, type);
                   
                }
            }
            else
            {
                Debug.LogError("Failed to load data: " + task.Exception);
            }
        });
    }


void AddBuildingToList(string name, string photoUrl, string type)
{
    // Instantiate the prefab in the ScrollView content area
    GameObject newBuilding = Instantiate(buildingPrefab, contentParent);

    // Find and update the TextMeshPro components
    TextMeshProUGUI buildingNameText = newBuilding.transform.Find("Panel/Building Name").GetComponent<TextMeshProUGUI>();
    TextMeshProUGUI buildingTypeText = newBuilding.transform.Find("Panel/Building Type").GetComponent<TextMeshProUGUI>();

    // Set the building name and type
    buildingNameText.text = name;
    buildingTypeText.text = type;

    // Find and update the Image component
    Image buildingPhotoImage = newBuilding.transform.Find("Building Photo").GetComponent<Image>();
    StartCoroutine(LoadImage(photoUrl, buildingPhotoImage));
}



    System.Collections.IEnumerator LoadImage(string url, Image image)
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                // Get the downloaded texture
                Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;

                // Create a sprite from the texture
                image.sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height), // Full texture
                    new Vector2(0.5f, 0.5f) // Pivot at the center
                );

                // Adjust the image RectTransform to fill the parent space
                RectTransform rectTransform = image.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 200); // Fixed size or dynamic value
                image.preserveAspect = true;

            }

            else
            {
                Debug.LogError("Failed to load image: " + request.error);
            }
        }
    }


}
