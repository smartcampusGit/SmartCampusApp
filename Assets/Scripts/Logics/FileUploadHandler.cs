using UnityEngine;
using TMPro;
using SFB;
using System.IO;
using System.Linq;
using UnityEngine.Events;

public class FileUploadHandler : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text fileNameText;

    [Header("Behavior")]
    public bool showFileName = true;

    [Header("Accepted Extensions (e.g., .pdf, .jpg, .png)")]
    public string[] acceptedExtensions;

    [Header("Callback")]
    public UnityEvent<string> onFileSelected;

    [Header("Validator Notifier")]
    public UnityEvent onValidationUpdate;

    [HideInInspector]
    public bool validFileSelected = false;

    public void OpenFileBrowser()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        var paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", "", false);

        if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
        {
            if (showFileName && fileNameText != null)
                fileNameText.text = "No file chosen";
            validFileSelected = false;
            onValidationUpdate?.Invoke();
            return;
        }

        string selectedPath = paths[0];
        string ext = Path.GetExtension(selectedPath).ToLowerInvariant();

        FileInfo fileInfo = new FileInfo(selectedPath);
        if (!fileInfo.Exists || fileInfo.Length == 0)
        {
            if (showFileName && fileNameText != null)    
                fileNameText.text = "File is empty or missing";
            validFileSelected = false;
            onValidationUpdate?.Invoke();
            return;
        }

        if (!acceptedExtensions.Contains(ext))
        {
            if (showFileName && fileNameText != null)
                fileNameText.text = "Invalid file format";
            validFileSelected = false;
            onValidationUpdate?.Invoke();
            return;
        }

        // Valid file
        validFileSelected = true;
        if (showFileName && fileNameText != null)
            fileNameText.text = "Selected File: " + Path.GetFileName(selectedPath);
        
        onFileSelected?.Invoke(selectedPath);
        onValidationUpdate?.Invoke();
#endif
    }
}
