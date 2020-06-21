using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Editor Config")]
    public int defaultLevel = 1;
    public float defaultObjective = 3000;
    public float upObjectiveDefault = 500;
    public float defaultTime = 120;

    [Header("Runtime Config")]
    public int level = 1;
    public float score;
    public float objective = 3000;
    public float time = 120;

    [HideInInspector]
    public bool startedGame;

    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

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
                    NextLevel();
                }
                else if (GameGrid.instance.canPlay)
                {
                    GameOver();
                }
            }
        }
    }

    public void UpScore(float score)
    {
        this.score += score;
    }

    void NextLevel()
    {
        Debug.Log("Continue");
        time = defaultTime;
        score = 0;
        objective += upObjectiveDefault;
        level++;
        PanelGame.instance.NextRound(level);
        GameGrid.instance.Reset();
    }

    void GameOver()
    {
        Debug.Log("Game Over");
        GameGrid.instance.DestroyGrid();
        GameUI.instance.GameOver();
        startedGame = false;
    }

    public void PlayGame()
    {
        PanelGame.instance.NextRound(level);
        time = defaultTime;
        score = 0;
        level = defaultLevel;
        objective = defaultObjective;
        startedGame = true;
        GameGrid.instance.Reset();
    }
}
