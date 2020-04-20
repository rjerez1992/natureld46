using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static PlayerController;

public class ActionManager : MonoBehaviour
{
    public PlayerAction Action;
    public string ActionName = "Example action";
    public string ActionDescription = "A very short description for the example action";
    public int ActionCost = 100;
    public float ScaleIncreaseOnMouseOver = 1.25f;

    public TextMeshProUGUI ActionNameText;
    public TextMeshProUGUI ActionDescriptionText;
    public TextMeshProUGUI ActionCostText;

    public PlayerController Player;
    private RectTransform Rect;
    public AudioOnClick Audio;

    private void Awake()
    {
        Rect = GetComponent<RectTransform>();
        ActionNameText.text = ActionName;
        ActionDescriptionText.text = ActionDescription;
        ActionCostText.text = ActionCost.ToString();
    }

    void Start()
    {
        
    }

    private void OnMouseEnter()
    {
        Rect.localScale = new Vector3(1 * ScaleIncreaseOnMouseOver, 1 * ScaleIncreaseOnMouseOver, 1);
        ShowData();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Player.SetIntent(Action, this))
            {
                Audio.PlayClick();
            }
            else {
                Audio.PlayClick2();
            }
        }
    }

    private void OnMouseExit()
    {
        Rect.localScale = new Vector3(1, 1, 1);
        HideData();
    }

    public void ShowData() {
        ActionNameText.gameObject.SetActive(true);
        ActionDescriptionText.gameObject.SetActive(true);
        
    }

    public void HideData() {
        ActionNameText.gameObject.SetActive(false);
        ActionDescriptionText.gameObject.SetActive(false);
    }
}
