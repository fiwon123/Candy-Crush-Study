using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PanelInfo : MonoBehaviour
{
    public TMP_Text objectiveText;
    public TMP_Text scoreText;
    public Image fillScore;
    public TMP_Text timeText;

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        objectiveText.text = GameManager.instance.objective.ToString("F0");
        fillScore.fillAmount = GameManager.instance.score/GameManager.instance.objective;
        scoreText.text = GameManager.instance.score.ToString("F0"); 
        timeText.text = GameManager.instance.time.ToString("F0");
    }
    
}
