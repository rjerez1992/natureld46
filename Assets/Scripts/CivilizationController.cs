using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CivilizationController : MonoBehaviour
{
    public enum Stage { OldAge, MiddleAge, ModernAge }

    [Header("Stats")]
    public float Population = 10;
    public float Food = 10;
    public float Resources = 10;
    public float Technology = 0;

    [Header("Tweak multipliers")]
    public float PopulationBaseIncreaseRate = 1f;
    public float PopulationBaseDecreaseRate = 1f;
    public float AnimalFoodBaseIncreaseRate = 1f;
    public float WoodResourceBaseIncreaseRate = 1f;
    public float RockResourceBaseIncreaseRate = 1f;
    public float TechnologyBaseIncreaseRate = 1f;
    public float PollutionGenerationBaseRate = 1f;
    public float FoodConsumptionBaseRate = 1f;
    public float ResourceConsumptionBaseRate = 1f;
    public float ExpansionBaseProbability = 1f;
    public float VirusMortalityBaseRate = 1f;
    public float BacteriaMortalityBaseRate = 1f;
    public float EarthquakeMortalityBaseRate = 1f;
    public float WildfireMortalityBaseRate = 1f;
    public float StormMortalityBaseRate = 1f;
   

    [Header("UI Sources")]
    public TextMeshProUGUI StageText;
    public GameObject OldStageBanner;
    public GameObject MiddleStageBanner;
    public GameObject ModernStageBanner;
    public TextMeshProUGUI PopulationText;
    public TextMeshProUGUI FoodText;
    public TextMeshProUGUI ResourcesText;
    public TextMeshProUGUI TechnologyText;
    public GameObject BacteriaIndicator;
    public GameObject VirusIndicator;

    [Header("Other")]
    public Stage CurrentStage;
    public float TimeBetweenProgress = 1f;
    public int CivilizationSize = 1;

    //Stage stats
    private float AgePopulationIncrease;
    private float AgePopulationDecrease;
    private float AgePopulationStarvingIncreaseMultiplier;
    private float AgePopulationStarvingDecreaseMultiplier;
    private float AgePopulationBacteriaDecreaseMultiplier;
    private float AgePopulationVirusDecreaseMultiplier;
    private float AgeFoodConsumption;
    private float AgeFoodConsumptionSickMultiplier;
    private float AgeResourceConsumption;
    private float AgeResourceConsumptionSickMultiplier;
    private float AgeResourceFoodConsumption;
    private float AgeResourceFoodProduction;
    private float AgeResourceWoodConsumption;
    private float AgeResourceWoodProduction;
    private float AgeResourceRockConsumption;
    private float AgeResourceRockProduction;
    private float AgeTechnologyIncrease;
    private float AgeExpansionRequiredPeople;
    private float AgeExpansionRequiredResources;
    private float AgeExpansionRequiredTechnology;
    private float AgeExpansionMaxQuantity;
    private float AgePollutionGeneration;
    private float AgeBacteriaMediaDuration;
    private float AgeVirusMediaDuration;
    private float AgeAdvanceRequerimentPopulation;
    private float AgeAdvanceRequerimentFood;
    private float AgeAdvanceResourceRequeriment;
    private float AgeAdvanceTechnologyRequeriment;
    private float AgeAdvanceYearRequeriment;
    private float AgePopulationMaximumPerCity;

    //NOTE: Calculated from base and environment
    private float PopulationIncrease;
    private float PopulationDecrease;
    private float ResourceConsupmtion;
    private float FoodConsumption;
    private float TechnologyChange;
    private float PollutionGeneration;

    //Infection
    private float RemainingBacteria;
    private float RemainingVirus;

    //Flags
    private bool HasVirus = false;
    private bool HasBacteria = false;
    private bool IsStarving = false;
    private bool OutOfResources = false;
    private bool IsOverPopulated = false;

    //Others
    private float CurrentElapsedTime = 0;
    public GameController Game;
    public PlayerController Player;
    private List<TileController> GatheringTiles;
    private TileController InitialTile;
    private List<TileController> CivilizationTiles;

    private void Awake()
    {
        int Difficulty = PlayerPrefs.GetInt("Difficulty", 1);
        if (Difficulty == 0) {
             PopulationBaseIncreaseRate = 1.25f;
             PopulationBaseDecreaseRate = 0.75f;
             AnimalFoodBaseIncreaseRate = 1.75f;
             WoodResourceBaseIncreaseRate = 2f;
             RockResourceBaseIncreaseRate = 2f;
             TechnologyBaseIncreaseRate = 3f;
             PollutionGenerationBaseRate = 0.1f;
             FoodConsumptionBaseRate = 0.5f;
             ResourceConsumptionBaseRate = 0.5f;
             ExpansionBaseProbability = 3f;
             VirusMortalityBaseRate = 0.5f;
             BacteriaMortalityBaseRate = 0.5f;
             EarthquakeMortalityBaseRate = 0.25f;
             WildfireMortalityBaseRate = 0.25f;
             StormMortalityBaseRate = 0.25f;
        }
    }

    void Start()
    {
        GatheringTiles = new List<TileController>();
        CivilizationTiles = new List<TileController>();
        SetStage(Stage.OldAge);
        SetInitialTile();
        SetDistanceFromCenter();
        UpdateUI();
    }

    void Update()
    {
        CurrentElapsedTime += Time.deltaTime;
        if (CurrentElapsedTime >= TimeBetweenProgress) {
            CurrentElapsedTime = CurrentElapsedTime-TimeBetweenProgress;
            DoProgress();
        }

        if (HasBacteria) {
            RemainingBacteria -= Time.deltaTime;
            if (RemainingBacteria <= 0) {
                Game.Display("The bacteria has dissapeared! People survived");
                HasBacteria = false;
                BacteriaIndicator.SetActive(false);
            }
        }

        if (HasVirus)
        {
            RemainingVirus -= Time.deltaTime;
            if (RemainingVirus <= 0)
            {
                Game.Display("The virus has dissapeared! People survived");
                HasVirus = false;
                VirusIndicator.SetActive(false);
            }
        }
    }

    public void SetInitialTile() {
        TileController tile;
        do
        {
            tile = Game.Tiles[UnityEngine.Random.Range(0, Game.Tiles.Count)];
        } while (tile.Row < 2 || tile.Row > 4 || tile.Column < 6 || tile.Column > 9);
        tile.SetAsCity(true);
        InitialTile = tile;
        CivilizationTiles.Add(tile);
    }

    void UpdateUI() {
        PopulationText.text = ((int)Population).ToString();
        FoodText.text = ((int)Food).ToString();
        ResourcesText.text = ((int)Resources).ToString();
        TechnologyText.text = ((int)(Technology*100)).ToString()+"%";
    }

    void DoProgress() {
        ConsumeFood();
        ConsumeResources();

        UpdatePopulation();
        UpdateFood();
        UpdateResources();
        UpdateTechnology();
        Grow();
        Pollute();

        Advance();
        UpdateUI();
    }

    private void ConsumeFood()
    {
        //NOTE: Each person consumes X. Modified by temperature
        FoodConsumption = (Population * AgeFoodConsumption) * Game.TemperatureFoodConsumptionMultiplier;

        if (IsOverPopulated)
        {
            FoodConsumption *= 5f;
        }

        //NOTE: Sick people needs more food
        if (HasBacteria || HasVirus) {
            FoodConsumption *= AgeFoodConsumptionSickMultiplier;
        }

        Food = Food - (FoodConsumption * FoodConsumptionBaseRate);
        if (Food <= 0)
        {
            Food = 0;
            IsStarving = true;
            Game.Display("People is starving. Act quickly!");
        }
        else
        {
            IsStarving = false;
        }
    }

    private void ConsumeResources()
    {
        //NOTE: Each person consumes X resources.
        ResourceConsupmtion = (Population * AgeResourceConsumption);
        //NOTE: Bigger civilizations require more resources
        ResourceConsupmtion *= CivilizationSize;
        //NOTE: Sick civilizations consume more resources
        if (HasBacteria || HasVirus) {
            ResourceConsupmtion *= AgeResourceConsumptionSickMultiplier;
        }

        Resources = Resources - (ResourceConsupmtion * ResourceConsumptionBaseRate);
        if (Resources <= 0)
        {
            Resources = 0;
            OutOfResources = true;
        }
        else
        {
            OutOfResources = false;
        }
    }

    void UpdatePopulation()
    {
        float IncreaseModifier = 1f;
        float DecreaseModifier = 1f;

        //NOTE: Each couple can have a child with an X percent of change
        PopulationIncrease = (Population / 2) * AgePopulationIncrease * Game.TemperaturePopulationIncreaseMultiplier;
        //NOTE: Each person have a X change of dying
        PopulationDecrease = Population * AgePopulationDecrease * Game.TemperaturePopulationDecreaseMultiplier;

        if (IsStarving)
        {
            IncreaseModifier *= AgePopulationStarvingIncreaseMultiplier;
            DecreaseModifier *= AgePopulationStarvingDecreaseMultiplier;
        }
        else if (Population >= Food) {
            IncreaseModifier *= 0.25f;
        }
        if (HasBacteria)
        {
            DecreaseModifier *= AgePopulationBacteriaDecreaseMultiplier;
        }
        if (HasVirus)
        {
            switch (Game.CurrentTemperatureRange)
            {
                case GameController.TemperatureRange.VeryCold:
                    DecreaseModifier *= (AgePopulationVirusDecreaseMultiplier * 2f);
                    break;
                case GameController.TemperatureRange.Cold:
                    DecreaseModifier *= (AgePopulationVirusDecreaseMultiplier * 1.25f);
                    break;
                case GameController.TemperatureRange.Normal:
                    DecreaseModifier *= (AgePopulationVirusDecreaseMultiplier);
                    break;
                case GameController.TemperatureRange.Hot:
                    DecreaseModifier *= (AgePopulationVirusDecreaseMultiplier * 0.7f);
                    break;
                case GameController.TemperatureRange.VeryHot:
                    DecreaseModifier *= (AgePopulationVirusDecreaseMultiplier * 0.3f);
                    break;
            }
        }

        Population = Population + IncreaseModifier * (PopulationIncrease * PopulationBaseIncreaseRate);
        Population = Population - DecreaseModifier * (PopulationDecrease * PopulationBaseDecreaseRate);

        if (Population < 1)
        {
            Game.End(GameController.EndTrigger.CivilizationDeath);
        }

        if (Population > CivilizationSize * AgePopulationMaximumPerCity)
        {
            if (!IsOverPopulated) {
                Game.Display("City is over populated. Snap those fingers!");
            }
            IsOverPopulated = true;
        }
        else {
            IsOverPopulated = false;
        }
    }

    private void UpdateFood()
    {
        //TODO: Technology improves the resource consumption
        float ConsumptionModifier = 1f;

        ResourceConsupmtion = AgeResourceFoodConsumption * ConsumptionModifier;

        TileController[] TargetTiles = GatheringTiles.Where(x => x.Type == TileController.TileType.Animals).ToArray();
        foreach (TileController tile in TargetTiles)
        {
            Food = Food + (AgeResourceFoodProduction * (Population / (GatheringTiles.Count * 1.15f)) * GatheringTiles.Count * tile.DistanceMultiplier);
            tile.GetResources(ResourceConsupmtion*tile.DistanceMultiplier);
        }
    }

    private void UpdateResources()
    {
        //TODO: Technology improves the resource consumption
        float ConsumptionModifier = 1f;

        ResourceConsupmtion = AgeResourceWoodConsumption * ConsumptionModifier;
        TileController[] TargetTiles = GatheringTiles.Where(x => x.Type == TileController.TileType.Forest).ToArray();
        foreach (TileController tile in TargetTiles) {
            Resources = Resources + (AgeResourceWoodProduction * (Population / (GatheringTiles.Count * 1f)) * GatheringTiles.Count) * tile.DistanceMultiplier;
            tile.GetResources(ResourceConsupmtion * tile.DistanceMultiplier);
        }

        ResourceConsupmtion = AgeResourceRockConsumption * ConsumptionModifier;
        TargetTiles = GatheringTiles.Where(x => x.Type == TileController.TileType.Rock).ToArray();
        foreach (TileController tile in TargetTiles)
        {
            Resources = Resources + (AgeResourceRockProduction * (Population/(GatheringTiles.Count*0.75f)) * GatheringTiles.Count) * tile.DistanceMultiplier;
            tile.GetResources(ResourceConsupmtion * tile.DistanceMultiplier);
        }
    }

    private void UpdateTechnology()
    {
        //NOTE: Technology changes by X for each person
        TechnologyChange = (Population * AgeTechnologyIncrease);

        if (!IsStarving && !OutOfResources)
        {
            Technology = Technology + (TechnologyChange * TechnologyBaseIncreaseRate);
        } 
        else {
            //NOTE: Technology can't develop if we don't have food and resources
            Technology = Technology - (0.001f * (TechnologyChange * TechnologyBaseIncreaseRate));
        }

        if (Technology < 0)
        {
            Technology = 0;
        }
        else if (Technology > 1) {
            Technology = 1;
        }
    }
    
    private void Grow()
    {
        TileController[] emptyResources = Game.Tiles.Where(x => x.IsResource && !x.HasWorkersOn).ToArray();
        foreach (TileController tile in emptyResources) {
            //TODO:Schedule the start of the gathering by distance
            GatheringTiles.Add(tile);
            tile.SetWorkers(true);
        }

        if (Population  >= AgeExpansionRequiredPeople * CivilizationSize &&
            Resources  >= AgeExpansionRequiredResources * CivilizationSize &&
            Technology  >= AgeExpansionRequiredTechnology * CivilizationSize &&
            CivilizationSize < AgeExpansionMaxQuantity) {
            TileController targetTile = GetNerbyEmptyTile();
            if (targetTile != null) {
                targetTile.SetAsCity(false);
                CivilizationTiles.Add(targetTile);
                Game.Display("The civilization has expanded! Is this good?");
                Resources -= AgeExpansionRequiredResources * CivilizationSize;
                CivilizationSize++;
            }
            
        }
    }

    private void Pollute() {
        int TreeTiles = GatheringTiles.Where(x => x.Type == TileController.TileType.Forest).ToList().Count;
        PollutionGeneration = ((Population/(TreeTiles+1)) * AgePollutionGeneration * Technology);

        //NOTE:Lesser pollution doesn't affect the environment
        if (PollutionGeneration >= 0.0001f)
        {
            //NOTE: Sets the max pollution increase
            if (PollutionGeneration >= 0.0025f) { 
                PollutionGeneration = 0.0025f;
            }
            Player.IncreasePollution(PollutionGeneration);
        }
    }

    public void Advance() {
        if (CurrentStage == Stage.ModernAge) {
            return;
        }

        if (Population >= AgeAdvanceRequerimentPopulation &&
            Food >= AgeAdvanceRequerimentFood &&
            Resources >= AgeAdvanceResourceRequeriment &&
            Technology >= AgeAdvanceTechnologyRequeriment &&
            Game.CurrentYear >= AgeAdvanceYearRequeriment)
        {
            if (CurrentStage == Stage.OldAge)
            {
                SetStage(Stage.MiddleAge);
            }
            else if (CurrentStage == Stage.MiddleAge)
            {
                SetStage(Stage.ModernAge);
            }
            Food -= AgeAdvanceRequerimentFood;
            Resources -= AgeAdvanceResourceRequeriment;
            Technology = 0f;
        }
    }

    public void ActivateBacteria() {
        Game.Display("A new bacteria is infecting the people. Many will die!");
        BacteriaIndicator.SetActive(true);
        HasBacteria = true;
        RemainingBacteria = AgeBacteriaMediaDuration + UnityEngine.Random.Range(-3, 4);

    }

    public void ActivateVirus() {
        Game.Display("A new virus is infecting the people. Many will die!");
        VirusIndicator.SetActive(true);
        HasVirus = true;
        RemainingVirus = AgeVirusMediaDuration + UnityEngine.Random.Range(-3, 4);
    }

    void SetStage(Stage stage) {
        switch (stage)
        {
            case Stage.OldAge:
                Game.Display("A little settlement. They seem to need food", 5f);
                CurrentStage = stage;
                UpdateTilesOnAgeChange();
                OldStageBanner.SetActive(true);
                MiddleStageBanner.SetActive(false);
                ModernStageBanner.SetActive(false);
                StageText.text = "Old\nAge";
                AgePopulationIncrease = 0.04f;
                AgePopulationDecrease = 0.005f;
                AgePopulationStarvingIncreaseMultiplier = 0.25f;
                AgePopulationStarvingDecreaseMultiplier = 25f;
                AgePopulationBacteriaDecreaseMultiplier = 12f;
                AgePopulationVirusDecreaseMultiplier = 2f;
                AgeFoodConsumption = 0.025f;
                AgeFoodConsumptionSickMultiplier = 2f;
                AgeResourceConsumption = 0.01f;
                AgeResourceConsumptionSickMultiplier = 1.25f;
                AgeResourceFoodConsumption = 0.0075f;
                AgeResourceFoodProduction = 0.01f;
                AgeResourceWoodConsumption = 0.009f;
                AgeResourceWoodProduction = 0.007f;
                AgeResourceRockConsumption = 0.001f;
                AgeResourceRockProduction = 0.005f;
                AgeTechnologyIncrease = 0.0000025f;
                AgeExpansionRequiredPeople = 500;
                AgeExpansionRequiredResources = 1000;
                AgeExpansionRequiredTechnology = 0.1f;
                AgeExpansionMaxQuantity = 6;
                AgePollutionGeneration = 0.00000025f;
                AgeBacteriaMediaDuration = 12;
                AgeVirusMediaDuration = 5;
                AgeAdvanceRequerimentPopulation = 300; 
                AgeAdvanceRequerimentFood = 300;  
                AgeAdvanceResourceRequeriment = 300; 
                AgeAdvanceTechnologyRequeriment = 0.03f;
                AgeAdvanceYearRequeriment = 10; 
                AgePopulationMaximumPerCity = 1000;
                break;
            case Stage.MiddleAge:
                Game.Display("The people has avanced. The game has changed!");
                CurrentStage = stage;
                UpdateTilesOnAgeChange();
                OldStageBanner.SetActive(false);
                MiddleStageBanner.SetActive(true);
                ModernStageBanner.SetActive(false);
                StageText.text = "Middle\nAge";
                AgePopulationIncrease = 0.03f;
                AgePopulationDecrease = 0.006f;
                AgePopulationStarvingIncreaseMultiplier = 0.33f;
                AgePopulationStarvingDecreaseMultiplier = 20f;
                AgePopulationBacteriaDecreaseMultiplier = 10f;
                AgePopulationVirusDecreaseMultiplier = 5f;
                AgeFoodConsumption = 0.03f;
                AgeFoodConsumptionSickMultiplier = 1.5f;
                AgeResourceConsumption = 0.015f;
                AgeResourceConsumptionSickMultiplier = 1.25f;
                AgeResourceFoodConsumption = 0.005f;
                AgeResourceFoodProduction = 0.01f;
                AgeResourceWoodConsumption = 0.007f;
                AgeResourceWoodProduction = 0.007f;
                AgeResourceRockConsumption = 0.001f;
                AgeResourceRockProduction = 0.009f;
                AgeTechnologyIncrease = 0.0000033f;
                AgeExpansionRequiredPeople = 3000;
                AgeExpansionRequiredResources = 3000;
                AgeExpansionRequiredTechnology = 0.5f;
                AgeExpansionMaxQuantity = 9;
                AgePollutionGeneration = 0.0000005f;
                AgeBacteriaMediaDuration = 12;
                AgeVirusMediaDuration = 7;
                AgeAdvanceRequerimentPopulation = 10000;
                AgeAdvanceRequerimentFood = 10000;
                AgeAdvanceResourceRequeriment = 10000;
                AgeAdvanceTechnologyRequeriment = 0.5f;
                AgeAdvanceYearRequeriment = 40;
                AgePopulationMaximumPerCity = 3000;
                break;
            case Stage.ModernAge:
                Game.Display("The future is now. Things go crazy out of nothing");
                CurrentStage = stage;
                UpdateTilesOnAgeChange();
                OldStageBanner.SetActive(false);
                MiddleStageBanner.SetActive(false);
                ModernStageBanner.SetActive(true);
                StageText.text = "Modern\nAge";
                AgePopulationIncrease = 0.02f;
                AgePopulationDecrease = 0.009f;
                AgePopulationStarvingIncreaseMultiplier = 0.5f;
                AgePopulationStarvingDecreaseMultiplier = 15f;
                AgePopulationBacteriaDecreaseMultiplier = 7f;
                AgePopulationVirusDecreaseMultiplier = 15f;
                AgeFoodConsumption = 0.02f;
                AgeFoodConsumptionSickMultiplier = 1.25f;
                AgeResourceConsumption = 0.03f;
                AgeResourceConsumptionSickMultiplier = 1.1f;
                AgeResourceFoodConsumption = 0.001f;
                AgeResourceFoodProduction = 0.015f;
                AgeResourceWoodConsumption = 0.005f;
                AgeResourceWoodProduction = 0.009f;
                AgeResourceRockConsumption = 0.001f;
                AgeResourceRockProduction = 0.0095f;
                AgeTechnologyIncrease = 0.0000040f;
                AgeExpansionRequiredPeople = 15000;
                AgeExpansionRequiredResources = 10000;
                AgeExpansionRequiredTechnology = 0.75f;
                AgeExpansionMaxQuantity = 12;
                AgePollutionGeneration = 0.00000075f;
                AgeBacteriaMediaDuration = 7;
                AgeVirusMediaDuration = 20;
                AgeAdvanceRequerimentPopulation = 0;
                AgeAdvanceRequerimentFood = 0;
                AgeAdvanceResourceRequeriment = 0;
                AgeAdvanceTechnologyRequeriment =0;
                AgeAdvanceYearRequeriment = 0;
                AgePopulationMaximumPerCity = 20000;
                break;
        }
    }

    public void UpdateTilesOnAgeChange() {
        foreach (TileController tile in CivilizationTiles) {
            tile.UpdateAge();
        }
    }

    public TileController GetNerbyEmptyTile() {
        int Tries = 5;
        do
        {
            TileController targetTile = Game.Tiles[UnityEngine.Random.Range(0, Game.Tiles.Count)];
            if (targetTile.Type != TileController.TileType.Empty) {
                Tries--;
                continue;
            }
            foreach (TileController t in CivilizationTiles)
            {
                if (t.GetDistance(targetTile) <= 1)
                {
                    return targetTile;
                }
            }
            Tries--; 
        } while (Tries > 0);
        return null;
    }

    public void SetDistanceFromCenter() {
        foreach (TileController t in Game.Tiles) {
            t.SetDistance(InitialTile);
        }
    }

    public void OnTileChange(TileController tile) {
        if (!tile.IsResource) {
            GatheringTiles.Remove(tile);
        }
    }
}
