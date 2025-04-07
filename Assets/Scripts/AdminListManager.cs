using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
 
public class AdminListManager : MonoBehaviour
{
    [Header("Prefab + Content")]
    public GameObject adminPrefab;     // Prefab להצגת פרטי אדמין
    public Transform contentParent;    // הורה לכל האדמינים ב-UI (ScrollView Content)
 
    [Header("Search")]
    public TMP_InputField searchInput; // שדה חיפוש (TMP)
 
    private FirebaseFirestore db;
 
    // רשימה מקומית של כל האדמינים
    private List<AdminDataLocal> allAdmins = new List<AdminDataLocal>();
 
    void Start()
    {
        // אתחול Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firestore ready!");
 
                // חיבור מאזין לשדה חיפוש
                if (searchInput != null)
                {
                    searchInput.onValueChanged.AddListener(OnSearchValueChanged);
                }
 
                // טוען נתוני אדמינים
                LoadAllAdminsFromFirestore();
            }
            else
            {
                Debug.LogError("Firebase dependencies not resolved: " + task.Result);
            }
        });
    }
 
    /// <summary>
    /// מושך את כל המסמכים מאוסף "Admin_Profiles"
    /// ושומר ב-allAdmins, ואז מציג ב-UI.
    /// </summary>
    public void LoadAllAdminsFromFirestore()
    {
        db.Collection("Admin_Profiles").GetSnapshotAsync().ContinueWithOnMainThread(snapshotTask =>
        {
            if (snapshotTask.IsFaulted)
            {
                Debug.LogError("Failed to load admin data: " + snapshotTask.Exception);
                return;
            }
 
            QuerySnapshot snapshot = snapshotTask.Result;
            allAdmins.Clear();
 
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Dictionary<string, object> data = doc.ToDictionary();
 
                // קריאת שדות
                string docId   = doc.Id;
                string name    = data.ContainsKey("adminName")  ? data["adminName"].ToString()  : "";
                string email   = data.ContainsKey("email")      ? data["email"].ToString()      : "";
                string campus  = data.ContainsKey("campusId")   ? data["campusId"].ToString()   : "";
                string role    = data.ContainsKey("role")       ? data["role"].ToString()       : "";
                string status  = data.ContainsKey("status")     ? data["status"].ToString()     : "";
                string fileUrl = data.ContainsKey("employeeApprovalFileURL") 
                                    ? data["employeeApprovalFileURL"].ToString() : "";
                string photo   = data.ContainsKey("adminPhotoURL")
                                    ? data["adminPhotoURL"].ToString() : "";
 
                AdminDataLocal admin = new AdminDataLocal()
                {
                    docId = docId,
                    adminName = name,
                    email = email,
                    campusId = campus,
                    role = role,
                    status = status,
                    verificationFileURL = fileUrl,
                    photoURL = photo
                };
 
                allAdmins.Add(admin);
            }
 
            RefreshUI(allAdmins);
        });
    }
 
    /// <summary>
    /// מציג את הרשימה ב-UI, ע"י יצירת Prefab לכל אדמין.
    /// </summary>
    private void RefreshUI(List<AdminDataLocal> adminsToShow)
    {
        // מנקה UI ישן
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
 
        // יוצר אובייקט עבור כל אדמין
        foreach (var admin in adminsToShow)
        {
            GameObject itemGO = Instantiate(adminPrefab, contentParent);
            AdminItem item = itemGO.GetComponent<AdminItem>();
            if (item != null)
            {
                // ממלא את השדות ב-AdminItem
                item.Setup(this, admin);
            }
            else
            {
                Debug.LogError("No AdminItem component found on prefab!");
            }
        }
    }
 
    /// <summary>
    /// סינון רשימת האדמינים לפי שם/אימייל
    /// </summary>
    private void OnSearchValueChanged(string userInput)
    {
        string lower = userInput.ToLower();
 
        List<AdminDataLocal> filtered = allAdmins.FindAll(a =>
            a.adminName.ToLower().Contains(lower) ||
            a.email.ToLower().Contains(lower)
        );
 
        RefreshUI(filtered);
    }
 
    /// <summary>
    /// פונקציה שתיקרא מ-AdminItem, למשל לעדכן סטטוס האדמין בפיירסטור
    /// </summary>
    public void UpdateAdminStatus(string docId, string newStatus)
    {
        DocumentReference docRef = db.Collection("Admin_Profiles").Document(docId);
 
        docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "status", newStatus }
        })
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to update status: " + task.Exception);
            }
            else
            {
                Debug.Log("Status updated successfully!");
                // אפשר לרענן UI: LoadAllAdminsFromFirestore();
            }
        });
    }
}