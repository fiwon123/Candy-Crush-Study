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
                time = 0f;

                // Verifica se o objetivo foi alcançado e não há animação de match ocorrendo
                if (score >= objective && GameGrid.instance.canPlay)
                {
                    Debug.Log("Continue");
                    time = defaultTime;
                    score = 0;
                    objective += 500;
                    level++;
                    PanelGame.instance.NextRound(level);
                    GameGrid.instance.Reset();
                }
                else if (GameGrid.instance.canPlay)
                {
                    GameGrid.instance.DestroyGrid();
                    GameUI.instance.GameOver();
                    startedGame = false;
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
        level = 0;
        objective = defaultObjective;
        startedGame = true;
        GameGrid.instance.Reset();
    }
}
