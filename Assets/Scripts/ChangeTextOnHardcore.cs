using TMPro;
using UnityEngine;

public class ChangeTextOnHardcore : MonoBehaviour
{
    public string textToChangeTo = "";

    void Start()
    {
        if (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore)
        {
            GetComponent<TextMeshProUGUI>().text = textToChangeTo;
        }
    }
}
