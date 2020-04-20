using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public enum TileType { Empty, Forest, Rock, Animals, City, Initial }

    public int Row = 0;
    public int Column = 0;
    public TileType Type = TileType.Empty;

    public SpriteRenderer Renderer;
    public TextMeshProUGUI TileStat;
    public GameObject TileCanvasGameObject;
    public GameObject WorkersAnimation;

    private static Color NormalColor = new Color(1f, 1f, 1f);
    private static Color OverlayColor = new Color(0.8f, 0.8f, 0.8f);
    public PlayerController Player;
    public CivilizationController Civilization;
    public GameController Game;

    public bool IsResource;
    public bool HasWorkersOn;
    //NOTE:Represents a percentage
    public float RemainingResources;
    public float DistanceMultiplier = 0.0f;

    private void Awake()
    {
        Renderer = GetComponent<SpriteRenderer>();
        Renderer.sortingOrder = (int)(((transform.position.x + 25) * 250) + ((-transform.position.y + 25) * 10));
        SetRowAndColumn();
        IsResource = false;
    }

    void Start()
    {
        
    }

    void Update()
    {

    }

    private void SetRowAndColumn() {
        //NOTE: Based on objects names
        Row = int.Parse(gameObject.transform.parent.gameObject.name.Split(' ')[1].Replace("(", "").Replace(")", ""));
        Column = int.Parse(gameObject.name.Split(' ')[1].Replace("(", "").Replace(")", ""));
    }

    public void PlayerInteract() {
        if (Type != TileType.Empty) {
            Game.Display("Target tile is not empty");
            return;
        }

        switch (Player.CurrentIntent)
        {
            case PlayerController.PlayerAction.None:
                break;
            case PlayerController.PlayerAction.GenerateTrees:
                Player.ConsumeIntent();
                GenerateTrees();
                break;
            case PlayerController.PlayerAction.GenerateRocks:
                Player.ConsumeIntent();
                GenerateRocks();
                break;
            case PlayerController.PlayerAction.GenerateAnimals:
                Player.ConsumeIntent();
                GenerateAnimals();
                break;
        }
    }

    public void GenerateTrees() {
        Type = TileType.Forest;
        ChangeGraphicsByName("Forest");
        IsResource = true;
        HasWorkersOn = false;
        RemainingResources = 1;
    }

    public void GenerateRocks() {
        Type = TileType.Rock;
        ChangeGraphicsByName("Rocks");
        IsResource = true;
        HasWorkersOn = false;
        RemainingResources = 1;
    }

    public void GenerateAnimals() {
        Type = TileType.Animals;
        ChangeGraphicsByName("Animals");
        IsResource = true;
        HasWorkersOn = false;
        RemainingResources = 1;
    }

    public void GetResources(float quantity) {
        RemainingResources -= quantity;
        if (RemainingResources <= 0) {
            Type = TileType.Empty;
            ChangeGraphicsByName("None");
            IsResource = false;
            SetWorkers(false);
            PropagateTileChange();
        }
    }

    public void SetAsCity(bool IsInitial) {
        if (IsInitial)
        {
            Type = TileType.Initial;
        }
        else {
            Type = TileType.City;
        }
        UpdateAge();
    }

    public void SetWorkers(bool state) {
        if (state)
        {
            HasWorkersOn = true;
            WorkersAnimation.SetActive(true);
        }
        else {
            HasWorkersOn = false;
            WorkersAnimation.SetActive(false);
        }
    }

    public void UpdateAge() {
        if (Type == TileType.City || Type == TileType.Initial)
        {
            switch (Civilization.CurrentStage)
            {
                case CivilizationController.Stage.OldAge:
                    ChangeGraphicsByName("Tents");
                    break;
                case CivilizationController.Stage.MiddleAge:
                    ChangeGraphicsByName("Castle");
                    break;
                case CivilizationController.Stage.ModernAge:
                    ChangeGraphicsByName("City");
                    break;
                default:
                    break;
            }
        }
    }

    public void ChangeGraphicsByName(string name) {
        foreach (Transform child in transform) {
            if (child.gameObject.name == name)
            {
                child.gameObject.SetActive(true);
            }
            else {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void PropagateTileChange()
    {
        Civilization.OnTileChange(this);
    }

    public void ShowTileStats() {
        TileCanvasGameObject.SetActive(true);
        switch (Type)
        {
            case TileType.Empty:
                TileStat.text = "Empty";
                break;
            case TileType.Forest:
                TileStat.text = "Forest\n"+((int)(RemainingResources * 100))+"%";
                break;
            case TileType.Rock:
                TileStat.text = "Rocks\n" + ((int)(RemainingResources * 100)) + "%";
                break;
            case TileType.Animals:
                TileStat.text = "Animals\n" + ((int)(RemainingResources * 100)) + "%";
                break;
            default:
                TileStat.text = "City";
                break;
        }
    }

    public void HideTileStats() {
        TileCanvasGameObject.SetActive(false);
    }

    public void SetDistance(TileController tile)
    {
        DistanceMultiplier = (1 - (((float)(Mathf.Abs(Row - tile.Row) + (float)Mathf.Abs(Column - tile.Column))) / 13) / 1.5f)+0.1f;
        if (DistanceMultiplier > 1) {
            DistanceMultiplier = 1;
        }
    }

    public int GetDistance(TileController tile)
    {
        return Mathf.Abs(Row - tile.Row) + Mathf.Abs(Column - tile.Column);
    }

    private void OnMouseEnter()
    {
        ShowTileStats();
        Renderer.color = OverlayColor;
    }

    private void OnMouseOver()
    {
        ShowTileStats();
        if (Input.GetMouseButtonDown(0)) {
            PlayerInteract();
        }
    }

    private void OnMouseExit()
    {
        Renderer.color = NormalColor;
        HideTileStats();
    }
}
