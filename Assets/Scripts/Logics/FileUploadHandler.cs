using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB; // StandaloneFileBrowser
using System.IO;

public class FileUploadHandler : MonoBehaviour
{
    
    public void OpenFileBrowser(TMP_Text fileNameText)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        var paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", "", false);
        
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string fileName = Path.GetFileName(paths[0]);
            fileNameText.text = "Selected File: " + fileName;
            FindObjectOfType<AdminRegistrationHandler>().SetLogoPath(paths[0]);
        }
        else
        {
            fileNameText.text = "No file chosen";
        }
#endif
    }
}
