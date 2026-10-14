using UnityEngine;
using TMPro;

public class MaxScript : MonoBehaviour
{
    public TextMeshProUGUI max_text;

    void Update()
    {
        max_text.SetText("" + PlayerPrefs.GetInt("max"));
    }
}
