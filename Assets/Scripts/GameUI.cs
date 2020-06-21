using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public Animator anim;

    public static GameUI instance;

    void Awake()
    {
        instance = this;
    }

    public void ClickPlay(){
        anim.Play("Play");
    }

    public void ClickPlayAgain(){
        anim.Play("PlayAgain");
    }

    public void ClickMainMenu(){
        anim.Play("MainMenu");
    }

    public void GameOver(){
        anim.Play("GameOver");
    }

    public void StartGame(){
        GameManager.instance.PlayGame();
    }
}
