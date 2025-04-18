using UnityEngine;

using UnityEngine.UI;

using TMPro;

using UnityEngine.SceneManagement;

using Firebase.Firestore;

using Firebase.Extensions;

using System.Collections;
 
public class BuildingItem : MonoBehaviour

{

    [Header("UI References")]

    public TextMeshProUGUI buildingNameText;

    public TextMeshProUGUI buildingDescriptionText;

    public Image buildingImage;    // optional display image

    public Button editButton;

    public Button deleteButton;
 
    // ▼ חדש: אפשרות להציג/לערוך סטטוס דרך Dropdown.

    // אם לא תרצה Dropdown, אפשר להשתמש במקום זה ב־TextMeshProUGUI בלבד (כמו name/desc).

    [Header("Status")]

    public TMP_Dropdown statusDropdown; 

    public Button statusUpdateButton; // כפתור נפרד לעדכן את הסטטוס (אם רוצים)
 
    private string docId; // Firestore doc ID

    private string currentStatus; // נשמור כאן את הסטטוס מהדאטה
 
    /// <summary>

    /// נקרא אחרי שמייצרים את הפריט מ־BuildingListManager

    /// </summary>

    public void Setup(string documentId, string name, string desc, string imageUrl, string status = "")

    {

        docId = documentId;

        //מילוי טקסטים

        if (buildingNameText) buildingNameText.text = name;

        if (buildingDescriptionText) buildingDescriptionText.text = desc;
 
        currentStatus = status;
 
        // מילוי Dropdown לסטטוס 

        // (נניח שב-Inspector הוספת 0=Active,1=Pending,2=Reject – או מה שמתאים)

        if (statusDropdown != null)

        {

            int idx = GetDropdownIndexForStatus(currentStatus);

            statusDropdown.value = idx;

        }
 
        // אופציונלי: טעינת תמונה

        StartCoroutine(LoadImage(imageUrl, buildingImage));
 
        // טיפול בכפתור Edit

        if (editButton != null)

        {

            editButton.onClick.RemoveAllListeners();

            editButton.onClick.AddListener(OnEditClicked);

        }
 
        // טיפול בכפתור Delete

        if (deleteButton != null)

        {

            deleteButton.onClick.RemoveAllListeners();

            deleteButton.onClick.AddListener(OnDeleteClicked);

        }
 
        // טיפול בכפתור עדכון הסטטוס (אם תרצה לאחד את זה עם OnEditClicked – אפשר)

        if (statusUpdateButton != null)

        {

            statusUpdateButton.onClick.RemoveAllListeners();

            statusUpdateButton.onClick.AddListener(OnStatusUpdateClicked);

        }

    }
 
    private void OnDeleteClicked()

    {

        Debug.Log($"Deleting building docId={docId}...");

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        DocumentReference docRef = db.Collection("Buildings").Document(docId);
 
        docRef.DeleteAsync().ContinueWithOnMainThread(task =>

        {

            if (task.IsFaulted || task.IsCanceled)

            {

                Debug.LogError("Error deleting document: " + task.Exception);

            }

            else

            {

                Debug.Log("Building deleted successfully!");

                // השמדת הפריט מה-UI

                Destroy(this.gameObject);
 
                // אפשר לרענן את הרשימה הראשית, אם BuildingListManager1 שם

                var manager = FindObjectOfType<BuildingListManager1>();

                if (manager != null)

                {

                    manager.LoadAllBuildingsFromFirestore();

                }

            }

        });

    }
 
    private void OnEditClicked()

    {

        Debug.Log($"OnEditClicked: docId = {docId}");

        BuildingDataHolder.docId = docId;

        // טוען סצנה אחרת לעריכה

        SceneManager.LoadScene("EditBuildingpage1");

    }
 
    private void OnStatusUpdateClicked()

    {

        // קוראים מה-Dropdown את הסטטוס החדש

        string newStatus = GetStatusFromDropdown();

        Debug.Log($"Updating building status to: {newStatus}");
 
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        DocumentReference docRef = db.Collection("Buildings").Document(docId);
 
        docRef.UpdateAsync(new { status = newStatus })

        .ContinueWithOnMainThread(task =>

        {

            if (task.IsFaulted || task.IsCanceled)

            {

                Debug.LogError("Error updating status: " + task.Exception);

            }

            else

            {

                Debug.Log("Status updated successfully in Firestore!");

                currentStatus = newStatus; // עדכון מקומי, אם צריך

            }

        });

    }
 
    // פונקציה שממירה “Active” / “Pending” / “Reject” ל־index ב-Dropdown

    private int GetDropdownIndexForStatus(string s)

    {

        switch (s.ToLower())

        {

            case "active":  return 0;

            case "pending": return 1;

            case "reject":  return 2;

            default:        return 1; // ברירת מחדל Pending

        }

    }
 
    // לוקח את הערך מה-Dropdown ומחזיר מחרוזת

    private string GetStatusFromDropdown()

    {

        if (statusDropdown == null) return "pending";

        int val = statusDropdown.value;

        // לפי ההגדרות ב-Inspector:

        // 0=Active,1=Pending,2=Reject

        if (val == 0) return "active";

        else if (val == 2) return "reject";

        else return "pending";

    }
 
    // טעינת תמונה מהאינטרנט

    private IEnumerator LoadImage(string url, Image image)

    {

        if (string.IsNullOrEmpty(url)) yield break;
 
        using (UnityEngine.Networking.UnityWebRequest request 

               = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))

        {

            yield return request.SendWebRequest();
 
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)

            {

                Texture2D texture 

                    = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;

                image.sprite = Sprite.Create(

                    texture,

                    new Rect(0, 0, texture.width, texture.height),

                    new Vector2(0.5f, 0.5f)

                );

                image.preserveAspect = true;

            }

            else

            {

                Debug.LogError("Failed to load image: " + request.error);

            }

        }

    }

}

 