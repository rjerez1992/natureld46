using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AgeIndicatorController : MonoBehaviour
{
    public TextMeshProUGUI TextIndicator;
    private RectTransform Rect;
    private float ScaleIncreaseOnMouseOver = 1.25f;

    void Start()
    {
        Rect = GetComponent<RectTransform>();
        DisplayData(false);
    }

    void Update()
    {

    }

    public void SetText(string text) {
        TextIndicator.text = text;
    }

    private void OnMouseEnter()
    {
        Rect.localScale = new Vector3(1 * ScaleIncreaseOnMouseOver, 1 * ScaleIncreaseOnMouseOver, 1);
        DisplayData(true);
    }

    private void OnMouseExit()
    {
        Rect.localScale = new Vector3(1, 1, 1);
        DisplayData(false);
    }

    public void DisplayData(bool state)
    {
        TextIndicator.gameObject.SetActive(state);
    }
}
