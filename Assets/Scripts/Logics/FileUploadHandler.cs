using UnityEngine;
using TMPro;
using SFB;
using System.IO;
using UnityEngine.Events;

public class FileUploadHandler : MonoBehaviour
{
    public TMP_Text fileNameText;
    public UnityEvent<string> onFileSelected;

    public void OpenFileBrowser()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        var paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", "", false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string fileName = Path.GetFileName(paths[0]);
            fileNameText.text = "Selected File: " + fileName;

            onFileSelected.Invoke(paths[0]);
        }
        else
        {
            fileNameText.text = "No file chosen";
        }
#endif
    }
}
