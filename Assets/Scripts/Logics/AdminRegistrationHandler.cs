using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;

public class AdminRegistrationHandler : MonoBehaviour
{
    [Header("Admin Info Inputs")]
    public TMP_InputField adminNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField roleInput;

    [Header("Campus Info Inputs")]
    public TMP_InputField campusNameInput;
    public TMP_InputField cityInput;
    public TMP_InputField countryInput;

    [Header("File Path Displays (TMP_Texts)")]
    public TMP_Text logoPathText;
    public TMP_Text mapPathText;
    public TMP_Text approvalPathText;

    [Header("Loading Spinner")]
    public GameObject loadingSpinner;

    private string logoFilePath;
    private string mapFilePath;
    private string approvalFilePath;

    // Setters called by FileUploadHandler
    public void SetLogoPath(string path) => logoFilePath = path;
    public void SetMapPath(string path) => mapFilePath = path;
    public void SetApprovalPath(string path) => approvalFilePath = path;

    public void OnSubmitClicked()
    {
        if (!FirebaseInitializer.IsReady)
        {
            Debug.LogWarning("Firebase is not ready!");
            return;
        }

        if (string.IsNullOrWhiteSpace(emailInput.text))
        {
            Debug.LogWarning("Email is required.");
            emailInput.Select();
            return;
        }

        StartCoroutine(HandleRegistrationFlow());
    }

    private IEnumerator HandleRegistrationFlow()
    {
        loadingSpinner.SetActive(true);

        var auth = FirebaseInitializer.Auth;
        var firestore = FirebaseInitializer.Firestore;
        var storage = FirebaseInitializer.Storage;

        // STEP 1: Create Firebase Auth user
        var regTask = auth.CreateUserWithEmailAndPasswordAsync(emailInput.text, passwordInput.text);
        yield return new WaitUntil(() => regTask.IsCompleted);

        if (regTask.Exception != null)
        {
            Debug.LogError("Firebase Auth failed: " + regTask.Exception);
            loadingSpinner.SetActive(false);
            yield break;
        }

        var user = regTask.Result;
        string uid = user.User.UserId;
        string campusId = Guid.NewGuid().ToString();

        Uri logoUrl = null, mapUrl = null, approvalUrl = null;
        bool uploadFailed = false;

        // STEP 2: Upload all files
        yield return StartCoroutine(UploadFileCoroutine(logoFilePath, storage.GetReference($"campuses/{campusId}/logo.png"), uri => logoUrl = uri, err => {
            Debug.LogError("Logo upload failed: " + err);
            uploadFailed = true;
        }));

        yield return StartCoroutine(UploadFileCoroutine(mapFilePath, storage.GetReference($"campuses/{campusId}/map.png"), uri => mapUrl = uri, err => {
            Debug.LogError("Map upload failed: " + err);
            uploadFailed = true;
        }));

        yield return StartCoroutine(UploadFileCoroutine(approvalFilePath, storage.GetReference($"admins/{uid}/approval.pdf"), uri => approvalUrl = uri, err => {
            Debug.LogError("Approval file upload failed: " + err);
            uploadFailed = true;
        }));

        if (uploadFailed)
        {
            loadingSpinner.SetActive(false);
            yield break;
        }

        // STEP 3: Save to Firestore
        var now = Timestamp.GetCurrentTimestamp();
        DocumentReference campusDoc = firestore.Collection("Campuses").Document(campusId);
        DocumentReference adminDoc = firestore.Collection("Admin_Profiles").Document(uid);

        var campusData = new
        {
            name = campusNameInput.text,
            logoURL = logoUrl?.ToString(),
            mapImageURL = mapUrl?.ToString(),
            city = cityInput.text,
            country = countryInput.text,
            createdAt = now,
            adminId = new[] { uid }
        };

        var adminData = new
        {
            adminName = adminNameInput.text,
            email = emailInput.text,
            role = roleInput.text,
            status = "pending",
            createdAt = now,
            campusId = campusId,
            employeeApprovalFileURL = approvalUrl?.ToString()
        };

        Task campusWrite = campusDoc.SetAsync(campusData);
        Task adminWrite = adminDoc.SetAsync(adminData);
        yield return new WaitUntil(() => campusWrite.IsCompleted && adminWrite.IsCompleted);

        loadingSpinner.SetActive(false);
        Debug.Log("Registration complete!");
    }

    private IEnumerator UploadFileCoroutine(string filePath, StorageReference storageRef, Action<Uri> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            onError?.Invoke("Missing file path");
            yield break;
        }

        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
        if (fileBytes.Length == 0)
        {
            onError?.Invoke("File is empty");
            yield break;
        }

        var uploadTask = storageRef.PutBytesAsync(fileBytes);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.IsFaulted || uploadTask.Exception != null)
        {
            onError?.Invoke(uploadTask.Exception?.Message ?? "Unknown upload error");
            yield break;
        }

        var getUrlTask = storageRef.GetDownloadUrlAsync();
        yield return new WaitUntil(() => getUrlTask.IsCompleted);

        if (getUrlTask.IsFaulted || getUrlTask.Exception != null)
        {
            onError?.Invoke(getUrlTask.Exception?.Message ?? "Failed to get download URL");
            yield break;
        }

        onSuccess?.Invoke(getUrlTask.Result);
    }
}
