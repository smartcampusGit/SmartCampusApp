using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CampusStartItem : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI campusNameText;

    private string campusDocId;

    // מאפשר לקוד החיצוני להגדיר את שם הקמפוס
    public void Setup(string name, string docId = "")
    {
        campusNameText.text = name;
        campusDocId = docId;
    }

    // (אופציונלי) פעולה בלחיצה על הקמפוס
    public void OnCampusClicked()
    {
        Debug.Log("Campus clicked: " + campusNameText.text + " | DocID: " + campusDocId);

        // אם אתה רוצה לפתוח מסך אחר או לשמור את ה־docId:
        // CampusDataHolder.selectedCampusId = campusDocId;
        // SceneManager.LoadScene("CampusDetailsScene");
    }
}