using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridItem : MonoBehaviour
{
    [Tooltip("Pontuação por item no match")]
    public float score = 50;
    [Tooltip("Som de quando seleciona o item")]
    public AudioSource selectAudio;

    public int x
    {
        get;
        private set;
    }

    public int y
    {
        get;
        private set;
    }

    [HideInInspector]
    public int id;

    public void OnItemPositionChanged(int newX, int newY)
    {
        x = newX;
        y = newY;
        gameObject.name = string.Format("Sprite [{0}][{1}]", x, y);
    }

    void OnMouseDown()
    {
        if (OnMouseOverItemEventHandler != null)
        {
            selectAudio.Play();
            OnMouseSelectedItemEventHandler(this);
        }
    }

    void OnMouseUp() {
        OnMouseSelectedItemEventHandler(null);
    }

    void OnMouseOver() {
        OnMouseOverItemEventHandler(this);
    }

    public delegate void OnMouseOverItem(GridItem item);
    public delegate void OnMouseSelectedItem(GridItem item);
    public static event OnMouseOverItem OnMouseOverItemEventHandler;
    public static event OnMouseSelectedItem OnMouseSelectedItemEventHandler;
    
}
