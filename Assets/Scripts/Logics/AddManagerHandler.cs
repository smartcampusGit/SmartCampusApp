using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

/// <summary>
/// Sends “Add Manager” data (plus optional files) to the Cloud Function `/addManager`.
/// • CampusId is read from SessionData.Instance.CampusId.
/// • No Authorization header is added — the CF must allow unauthenticated calls while you test.
/// </summary>
public class AddManagerHandler : MonoBehaviour
{
    /* ────────── INPUT FIELDS ────────── */
    [Header("Admin Info Inputs")]
    public TMP_InputField adminNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField roleInput;

    /* ────────── FILE PATHS ────────── */
    private string approvalFilePath;
    private string profilePicturePath;

    public void SetApprovalPath(string path) => approvalFilePath = path;
    public void SetProfilePhotoPath(string path) => profilePicturePath = path;

    /* ────────── UI FEEDBACK ────────── */
    [Header("UI Feedback")]
    public GameObject loadingSpinner;
    [SerializeField] private GameObject successPopupOverlay;
    [SerializeField] private TMP_Text successText;
    [SerializeField] private Button okButton;

    /* ────────── CONFIG ────────── */
    private const string cloudFunctionUrl =
        "https://us-central1-smart-campus-navigation-105e9.cloudfunctions.net/addManager";

    private void Awake()
    {
        okButton.onClick.AddListener(() => StartCoroutine(FadeOutPopup()));     
        var btnColors = okButton.colors;
        btnColors.normalColor = new Color32(46, 204, 113, 255);
        btnColors.highlightedColor = new Color32(39, 174, 96, 255);
        okButton.colors = btnColors;
    }

    /* ---------- called by the SEND button ---------- */
    public void OnSubmitClicked() => StartCoroutine(SendAddManagerRequest());

    /* ────────── main coroutine ────────── */
    private IEnumerator SendAddManagerRequest()
    {
        /* basic validation */
        if (string.IsNullOrWhiteSpace(adminNameInput.text) ||
            string.IsNullOrWhiteSpace(emailInput.text) ||
            string.IsNullOrWhiteSpace(passwordInput.text) ||
            string.IsNullOrWhiteSpace(roleInput.text))
        {
            Debug.LogWarning("Fill all required fields.");
            yield break;
        }
        if (string.IsNullOrWhiteSpace(SessionData.Instance.CampusId))
        {
            Debug.LogError("CampusId not set in SessionData.");
            yield break;
        }

        loadingSpinner.SetActive(true);

        /* build form */
        WWWForm form = new WWWForm();
        form.AddField("campusId", SessionData.Instance.CampusId);
        form.AddField("email", emailInput.text.Trim());
        form.AddField("password", passwordInput.text);
        form.AddField("adminName", adminNameInput.text.Trim());
        form.AddField("role", roleInput.text.Trim());

        /* optional files */
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
            AddFileToForm(form, "profilePhoto", profilePicturePath, mime);
        }

        /* send */
        using (UnityWebRequest request = UnityWebRequest.Post(cloudFunctionUrl, form))
        {
            // (no Authorization header for now)
            yield return request.SendWebRequest();
            loadingSpinner.SetActive(false);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AddManager failed: " + request.error);
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("AddManager success: " + request.downloadHandler.text);
                ShowPopup(adminNameInput.text);
            }
        }
    }

    /* ────────── helpers ────────── */

    private void AddFileToForm(WWWForm form, string field, string filePath, string mime)
    {
        byte[] data = File.ReadAllBytes(filePath);
        string name = Path.GetFileName(filePath);
        form.AddBinaryData(field, data, name, mime);
    }

    private void ShowPopup(string adminName)
    {
        successText.text = $"<b>{adminName}</b> added successfully!";
        StartCoroutine(FadeInPopup());
    }

    private IEnumerator FadeInPopup()
    {
        CanvasGroup cg = successPopupOverlay.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        successPopupOverlay.SetActive(true);
        while (cg.alpha < 1f) { cg.alpha += Time.deltaTime * 2; yield return null; }
    }

    private IEnumerator FadeOutPopup()
    {
        CanvasGroup cg = successPopupOverlay.GetComponent<CanvasGroup>();
        while (cg.alpha > 0f) { cg.alpha -= Time.deltaTime * 2; yield return null; }
        successPopupOverlay.SetActive(false);
    }
}
