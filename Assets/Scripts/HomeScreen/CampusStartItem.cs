using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CampusStartItem : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI campusNameText;

    private string campusDocId;

    // ����� ���� ������� ������ �� �� ������
    public void Setup(string name, string docId = "")
    {
        campusNameText.text = name;
        campusDocId = docId;
    }

    // (���������) ����� ������ �� ������
    public void OnCampusClicked()
    {
        Debug.Log("Campus clicked: " + campusNameText.text + " | DocID: " + campusDocId);

        // �� ��� ���� ����� ��� ��� �� ����� �� ��docId:
        // CampusDataHolder.selectedCampusId = campusDocId;
        // SceneManager.LoadScene("CampusDetailsScene");
    }
}