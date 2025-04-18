using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Image messagePanel;

    public void ShowMessage(string text, string type)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);

        switch (type)
        {
            case "error":
                messageText.color = new Color32(198, 40, 40, 255);
                if (messagePanel != null) messagePanel.color = new Color(1f, 0f, 0f, 0.05f);
                break;
            case "info":
                messageText.color = new Color32(21, 101, 192, 255);
                if (messagePanel != null) messagePanel.color = new Color(0.3f, 0.6f, 1f, 0.05f);
                break;
            case "success":
                messageText.color = new Color32(46, 125, 50, 255);
                if (messagePanel != null) messagePanel.color = new Color(0.2f, 1f, 0.2f, 0.08f);
                break;
        }
    }

    public void HideMessage()
    {
        messageText.text = "";
        messageText.gameObject.SetActive(false);
        if (messagePanel != null) messagePanel.color = Color.clear;
    }
    public void ShowWrongPassword()
    {
        ShowMessage("Wrong password", "error");
    }

    public void ShowWaitingApproval()
    {
        ShowMessage("Waiting for approval", "info");
    }

    public void ShowRejected()
    {
        ShowMessage("Your registration was rejected", "error");
    }

    public void ShowUserNotFound()
    {
        ShowMessage("User not found", "error");
    }

    public void ShowLoginSuccess(string adminName)
    {
        ShowMessage($"Welcome, {adminName}!", "success");
    }
}
