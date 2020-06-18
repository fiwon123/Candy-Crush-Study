using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public Animator anim;

    public void ClickPlay(){
        anim.Play("Play");
    }

    public void StartGame(){
        GameManager.instance.PlayGame();
    }
}
