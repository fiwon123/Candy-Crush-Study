using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PanelGame : MonoBehaviour
{

    public TMP_Text roundText;
    public Animator anim;
    public static PanelGame instance;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void NextRound(int level){
        roundText.text = "Round " + level;
        anim.Play("Round");
    }
    
    public void Shuffle(){
        anim.Play("Shuffle");
    }
}
