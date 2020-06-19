using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int level = 1;
    public float score;
    public float objective = 3000;
    public float defaultObjective = 3000;
    public float time = 120;
    public float defaultTime = 120;

    public bool startedGame;

    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (startedGame)
        {
            time -= Time.deltaTime;

            if (time <= 0f)
            {
                if (score >= objective)
                {
                    Debug.Log("Continue");
                    time = defaultTime;
                    score = 0;
                    objective += 500;
                    level++;
                    PanelGame.instance.NextRound(level);
                    GameGrid.instance.Reset();
                }
                else
                {
                    GameGrid.instance.DestroyGrid();
                    GameUI.instance.GameOver();
                    startedGame = false;
                    time = 0f;
                }
            }
        }
    }

    public void UpScore(int score)
    {
        this.score += score;
    }

    public void PlayGame()
    {
        PanelGame.instance.NextRound(level);
        time = defaultTime;
        score = 0;
        objective = defaultObjective;
        startedGame = true;
        GameGrid.instance.Reset();
    }
}
