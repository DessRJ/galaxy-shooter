using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public TextMeshProUGUI texto_score;

    void Update()
    {
        texto_score.SetText("" + GameManagerScript.score);
    }
}
