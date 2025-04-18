using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class AdminRegistrationHandler : MonoBehaviour
{
    [Header("Admin Info Inputs")]
    public TMP_InputField adminNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField roleInput;

    [Header("Campus Info Inputs")]
    public TMP_InputField campusNameInput;
    public TMP_InputField descriptionInput;
    public TMP_InputField cityInput;
    public TMP_InputField countryInput;

    private string logoFilePath;
    private string mapFilePath;
    private string approvalFilePath;
    private string profilePicturePath;

    [Header("UI Feedback")]
    public GameObject loadingSpinner;

    [SerializeField] private GameObject successPopupOverlay;
    [SerializeField] private TMP_Text welcomeText;
    [SerializeField] private Button okButton;

    // Cloud Function URL
    private string cloudFunctionUrl = "https://us-central1-smart-campus-navigation-105e9.cloudfunctions.net/registerAdmin/registerAdmin";

    public void SetLogoPath(string path) => logoFilePath = path;
    public void SetMapPath(string path) => mapFilePath = path;
    public void SetApprovalPath(string path) => approvalFilePath = path;
    public void SetProfilePhotoPath(string path) => profilePicturePath = path;

    private void Awake()
    {
        okButton.onClick.AddListener(() => StartCoroutine(FadeOutPopup()));

        var btnColors = okButton.colors;
        btnColors.normalColor = new Color32(46, 204, 113, 255);
        btnColors.highlightedColor = new Color32(39, 174, 96, 255);
        okButton.colors = btnColors;
    }

    public void OnSubmitClicked()
    {
        StartCoroutine(SendRegistrationRequest());
    }

    private IEnumerator SendRegistrationRequest()
    {
        loadingSpinner.SetActive(true);

        WWWForm form = new WWWForm();
        form.AddField("email", emailInput.text);
        form.AddField("password", passwordInput.text);
        form.AddField("adminName", adminNameInput.text);
        form.AddField("role", roleInput.text);
        form.AddField("campusName", campusNameInput.text);
        form.AddField("description", descriptionInput.text);
        form.AddField("city", cityInput.text);
        form.AddField("country", countryInput.text);

        if (File.Exists(logoFilePath))
        {
            AddFileToForm(form, "logo", logoFilePath, "image/jpeg");
        }

        if (File.Exists(mapFilePath))
        {
            AddFileToForm(form, "map", mapFilePath, "image/jpeg");
        }

        if (File.Exists(approvalFilePath))
        {
            string ext = Path.GetExtension(approvalFilePath).ToLower();
            string mime = ext == ".pdf" ? "application/pdf" : "application/octet-stream";
            AddFileToForm(form, "approval", approvalFilePath, mime);
        }

        if (File.Exists(profilePicturePath))
        {
            string ext = Path.GetExtension(profilePicturePath).ToLower();
            string mime = ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };
            AddFileToForm(form, "profilePicture", profilePicturePath, mime);
        }

        using (UnityWebRequest request = UnityWebRequest.Post(cloudFunctionUrl, form))
        {
            yield return request.SendWebRequest();
            loadingSpinner.SetActive(false);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Upload failed: " + request.error);
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("Upload success: " + request.downloadHandler.text);
                ShowPopup(adminNameInput.text);
            }
        }
    }

    private void AddFileToForm(WWWForm form, string fieldName, string filePath, string mimeType)
    {
        byte[] data = File.ReadAllBytes(filePath);
        string name = Path.GetFileName(filePath);
        form.AddBinaryData(fieldName, data, name, mimeType);
    }

    private void ShowPopup(string adminName)
    {
        welcomeText.text = $"Welcome, <b>{adminName}</b>";
        StartCoroutine(FadeInPopup());
    }

    private IEnumerator FadeInPopup()
    {
        CanvasGroup cg = successPopupOverlay.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        successPopupOverlay.SetActive(true);
        while (cg.alpha < 1f)
        {
            cg.alpha += Time.deltaTime * 2;
            yield return null;
        }
    }

    private IEnumerator FadeOutPopup()
    {
        CanvasGroup cg = successPopupOverlay.GetComponent<CanvasGroup>();
        while (cg.alpha > 0f)
        {
            cg.alpha -= Time.deltaTime * 2;
            yield return null;
        }
        successPopupOverlay.SetActive(false);
    }
}
