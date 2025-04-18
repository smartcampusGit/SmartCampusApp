using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class RegistrationValidator : MonoBehaviour
{
    [Header("Profile Picture")]
    public Image profilePictureFrame;
    public TMP_Text photoErrorText;
    private bool profilePictureSelected = false;

    [Header("Required Inputs")]
    public TMP_InputField adminNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField ensurePasswordInput;
    public TMP_InputField roleInput;

    [Header("File Upload")]
    public FileUploadHandler approvalUploader;

    [Header("Error Texts")]
    public TMP_Text adminNameError;
    public TMP_Text emailError;
    public TMP_Text passwordError;
    public TMP_Text ensurePasswordError;
    public TMP_Text roleError;
    public TMP_Text approvalFileError;

    [Header("UI References")]
    public Button nextButton;

    private bool approvalTouched = false;

    private void Start()
    {
        AddInputListeners();
        UpdateNextButtonState();
    }

    private void AddInputListeners()
    {
        AddFieldListeners(adminNameInput, adminNameError, ValidateAdminName);
        AddFieldListeners(emailInput, emailError, ValidateEmail);
        AddFieldListeners(passwordInput, passwordError, ValidatePassword);
        AddFieldListeners(ensurePasswordInput, ensurePasswordError, ValidateEnsurePassword);
        AddFieldListeners(roleInput, roleError, ValidateRole);
    }

    private void AddFieldListeners(TMP_InputField input, TMP_Text errorText, System.Action validator)
    {
        input.onEndEdit.AddListener(_ => validator());
        input.onValueChanged.AddListener(value =>
        {
            if (!string.IsNullOrWhiteSpace(value))
                errorText.gameObject.SetActive(false);
            UpdateNextButtonState();
        });
        input.onSelect.AddListener(_ => errorText.gameObject.SetActive(false));
    }

    public void DisplayProfilePicture(string path)
    {

        byte[] imageBytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes);

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );

        profilePictureFrame.sprite = sprite;
        profilePictureSelected = true;

        if (photoErrorText != null)
            photoErrorText.gameObject.SetActive(false);

        UpdateNextButtonState();
    }


    public void CheckProfilePictureRequirement()
    {
        if (!profilePictureSelected && photoErrorText != null)
            photoErrorText.gameObject.SetActive(true);

        UpdateNextButtonState();
    }

    public void MarkApprovalTouched()
    {
        approvalTouched = true;
        ValidateApprovalFile();
        UpdateNextButtonState();
    }

    public void UpdateNextButtonState()
    {
        nextButton.interactable = IsFormValid();
        approvalFileError.gameObject.SetActive(approvalTouched && !approvalUploader.validFileSelected);
    }

    private bool IsFormValid()
    {
        return
            !string.IsNullOrWhiteSpace(adminNameInput.text) &&
            !string.IsNullOrWhiteSpace(emailInput.text) &&
            emailInput.text.Contains("@") &&
            !string.IsNullOrWhiteSpace(passwordInput.text) &&
            passwordInput.text == ensurePasswordInput.text &&
            !string.IsNullOrWhiteSpace(roleInput.text) &&
            approvalUploader.validFileSelected &&
            profilePictureSelected;
    }

    private void ValidateAdminName() => adminNameError.gameObject.SetActive(string.IsNullOrWhiteSpace(adminNameInput.text));
    private void ValidateEmail() => emailError.gameObject.SetActive(string.IsNullOrWhiteSpace(emailInput.text) || !emailInput.text.Contains("@"));
    private void ValidatePassword() => passwordError.gameObject.SetActive(string.IsNullOrWhiteSpace(passwordInput.text));
    private void ValidateEnsurePassword()
    {
        bool isValid = ensurePasswordInput.text == passwordInput.text &&
                       !string.IsNullOrWhiteSpace(ensurePasswordInput.text);
        ensurePasswordError.gameObject.SetActive(!isValid);
    }
    private void ValidateRole() => roleError.gameObject.SetActive(string.IsNullOrWhiteSpace(roleInput.text));
    private void ValidateApprovalFile() => approvalFileError.gameObject.SetActive(!approvalUploader.validFileSelected);
}
