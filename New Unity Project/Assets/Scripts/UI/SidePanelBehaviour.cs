using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SidePanelBehaviour : MonoBehaviour
{
    RectTransform rect;
    bool opened;
    public Vector2 openedPos;
    public Vector2 closedPos;
    [Range(0f,1f)]
    public float LerpSpeed = 0.5f;
    
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    public void TogglePanel (bool open){
        opened = open;
    }

    void Update()
    {
        if (opened) {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, openedPos, LerpSpeed);
        } else {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, closedPos, LerpSpeed);
        }
    }
}
