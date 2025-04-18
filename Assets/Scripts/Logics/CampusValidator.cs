using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CampusValidator : MonoBehaviour
{
    [Header("Required Inputs")]
    public TMP_InputField campusNameInput;
    public TMP_InputField descriptionInput;
    public TMP_InputField cityInput;
    public TMP_InputField countryInput;

    [Header("File Handlers")]
    public FileUploadHandler logoUploader;
    public FileUploadHandler mapUploader;

    [Header("Error Texts")]
    public TMP_Text campusNameError;
    public TMP_Text descriptionError;
    public TMP_Text cityError;
    public TMP_Text countryError;
    public TMP_Text logoFileError;
    public TMP_Text mapFileError;

    [Header("UI References")]
    public Button submitButton;

    // Track if the user tried uploading a file
    private bool logoTouched = false;
    private bool mapTouched = false;

    private void Start()
    {
        AddInputListeners();
        UpdateSubmitButton();
    }

    private void AddInputListeners()
    {
        AddListeners(campusNameInput, campusNameError, ValidateCampusName);
        AddListeners(descriptionInput, descriptionError, ValidateDescription);
        AddListeners(cityInput, cityError, ValidateCity);
        AddListeners(countryInput, countryError, ValidateCountry);
    }

    private void AddListeners(TMP_InputField input, TMP_Text errorText, System.Action validator)
    {
        input.onEndEdit.AddListener(_ => validator());
        input.onValueChanged.AddListener(value =>
        {
            if (!string.IsNullOrWhiteSpace(value))
                errorText.gameObject.SetActive(false);
            UpdateSubmitButton();
        });
        input.onSelect.AddListener(_ => errorText.gameObject.SetActive(false));
    }

    public void UpdateSubmitButton()
    {
        submitButton.interactable = IsFormValid();

        // Only show file errors after user interacted
        logoFileError.gameObject.SetActive(logoTouched && !logoUploader.validFileSelected);
        mapFileError.gameObject.SetActive(mapTouched && !mapUploader.validFileSelected);
    }

    private bool IsFormValid()
    {
        return
            !string.IsNullOrWhiteSpace(campusNameInput.text) &&
            !string.IsNullOrWhiteSpace(descriptionInput.text) &&
            !string.IsNullOrWhiteSpace(cityInput.text) &&
            !string.IsNullOrWhiteSpace(countryInput.text) &&
            logoUploader.validFileSelected &&
            mapUploader.validFileSelected;
    }

    // Field Validators
    private void ValidateCampusName()
    {
        campusNameError.gameObject.SetActive(string.IsNullOrWhiteSpace(campusNameInput.text));
    }

    private void ValidateDescription()
    {
        descriptionError.gameObject.SetActive(string.IsNullOrWhiteSpace(descriptionInput.text));
    }

    private void ValidateCity()
    {
        cityError.gameObject.SetActive(string.IsNullOrWhiteSpace(cityInput.text));
    }

    private void ValidateCountry()
    {
        countryError.gameObject.SetActive(string.IsNullOrWhiteSpace(countryInput.text));
    }

    // Called from FileUploadHandler UnityEvents
    public void MarkLogoTouched()
    {
        logoTouched = true;
        UpdateSubmitButton();
    }

    public void MarkMapTouched()
    {
        mapTouched = true;
        UpdateSubmitButton();
    }
}
