using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public enum EndTrigger { Win, Pollution, CivilizationDeath, TimeRanOut, Default }

    public enum Weather { None, Rain, Snow }
    public enum TemperatureRange { VeryCold, Cold, Normal, Hot, VeryHot }

    public bool IsRunning = true;
    public int YearDuration = 15;
    public int GameEndYear = 100;
    public int CurrentYear = 0;
    public float CurrentTemperature = 20f;

    public TextMeshProUGUI YearCounter;
    public TextMeshProUGUI TemperatureText;

    public bool IsDisplayingText = false;
    public TextMeshProUGUI DisplayText;
    public float RemainingTextDuration;

    private float CurrentYearRemaining;

    public Image TemperatureFilterImage;
    public TemperatureRange CurrentTemperatureRange;

    [Header("Temperature multipliers")]
    public float TemperaturePopulationIncreaseMultiplier = 1f;
    public float TemperatureFoodConsumptionMultiplier = 1f;
    public float TemperaturePopulationDecreaseMultiplier = 1f;

    public GameObject InformationPanel;
    public GameObject EndgamePanel;
    public TextMeshProUGUI ResultTitle;
    public TextMeshProUGUI ResultDescription;

    public List<TileController> Tiles;
    public CivilizationController Civilization;

    void Awake()
    {
        Tiles = FindObjectsOfType<TileController>().OfType<TileController>().ToList();
        CurrentYearRemaining = YearDuration;
        YearCounter.text = CurrentYear.ToString();
        TemperatureFoodConsumptionMultiplier = 1f;
        TemperaturePopulationIncreaseMultiplier = 1f;
        TemperaturePopulationDecreaseMultiplier = 1f;
    }

    void Start()
    {
        Time.timeScale = 0;
        InformationPanel.SetActive(true);
    }

    void Update()
    {
        CurrentYearRemaining -= Time.deltaTime;
        if (CurrentYearRemaining <= 0)
        {
            CurrentYearRemaining = YearDuration;
            CurrentYear++;
            YearCounter.text = CurrentYear.ToString();
            if (CurrentYear >= GameEndYear)
            {
                CurrentYear = GameEndYear;
                CheckGameEnding();
            }
        }
        TemperatureText.text = CurrentTemperature + "°";
        if (IsDisplayingText) {
            RemainingTextDuration -= Time.deltaTime;
            if (RemainingTextDuration <= 0) {
                IsDisplayingText = false;
                DisplayText.gameObject.SetActive(false);
            }
        }
    }

    public void SetTemperature(int change) {
        CurrentTemperature += change;
        if (CurrentTemperature <= 5)
        {
            //TODO: Add snow effect on sustained lower temperatures
            TemperatureFilterImage.color = new Color(0, 0.6f, 1, 0.05f + 0.12f * (1 - ((CurrentTemperature - -5) / 16)));
            CurrentTemperatureRange = TemperatureRange.VeryCold;
            TemperaturePopulationIncreaseMultiplier = 0.75f - 0.30f * (1 - ((CurrentTemperature - -5) / 16));
            TemperatureFoodConsumptionMultiplier = 1.3f + 0.4f * (1 - ((CurrentTemperature - -5) / 16));
            TemperaturePopulationDecreaseMultiplier = 1.25f + 0.35f * (1 - ((CurrentTemperature - -5) / 16));
        }
        else if (CurrentTemperature <= 15) 
        {
            //TODO: Add rain effect on sustained low temperatures
            TemperatureFilterImage.gameObject.SetActive(true);
            TemperatureFilterImage.color = new Color(0, 0.6f, 1, 0.05f * (1 - ((CurrentTemperature - 6) / 9)));
            CurrentTemperatureRange = TemperatureRange.Cold;
            TemperaturePopulationIncreaseMultiplier = 1f - 0.25f * (1-((CurrentTemperature-6) / 9));
            TemperatureFoodConsumptionMultiplier = 1f + 0.3f * (1 - ((CurrentTemperature - 6) / 9));
            TemperaturePopulationDecreaseMultiplier = 1f + 0.25f * (1 - ((CurrentTemperature - 6) / 9));
        }
        else if (CurrentTemperature <= 24) 
        {
            TemperatureFilterImage.gameObject.SetActive(false);
            CurrentTemperatureRange = TemperatureRange.Normal;
            TemperaturePopulationIncreaseMultiplier = 1f;
            TemperatureFoodConsumptionMultiplier = 1f;
            TemperaturePopulationDecreaseMultiplier = 1f;
        }
        else if (CurrentTemperature <= 32) 
        {
            TemperatureFilterImage.gameObject.SetActive(true);
            TemperatureFilterImage.color = new Color(1, 0.5f, 0, 0.05f * (((CurrentTemperature - 25) / 8)));
            CurrentTemperatureRange = TemperatureRange.Hot;
            TemperaturePopulationIncreaseMultiplier = 1f - 0.25f * (((CurrentTemperature - 25) / 8));
            TemperatureFoodConsumptionMultiplier = 1f - 0.15f * (((CurrentTemperature - 25) / 8));
            TemperaturePopulationDecreaseMultiplier = 1f + 0.15f * (((CurrentTemperature - 25) / 8));
        }
        else { 
            TemperatureFilterImage.color = new Color(1, 0.5f, 0, 0.05f + 0.15f * (((CurrentTemperature - 33) / 8)));
            CurrentTemperatureRange = TemperatureRange.VeryHot;
            TemperaturePopulationIncreaseMultiplier = 0.75f - 0.25f * (((CurrentTemperature - 33) / 8));
            TemperatureFoodConsumptionMultiplier = 0.85f - 0.20f * (((CurrentTemperature - 33) / 8));
            TemperaturePopulationDecreaseMultiplier = 1.15f + 1.5f * (((CurrentTemperature - 33) / 8));
        }
    }

    public void CheckGameEnding() {
        if (Civilization.CurrentStage == CivilizationController.Stage.ModernAge) {
            End(EndTrigger.Win);
        }
        End(EndTrigger.TimeRanOut);
    }

    public void End(EndTrigger trigger)
    {
        IsRunning = false;
        Time.timeScale = 0;
        EndgamePanel.SetActive(true);
        switch (trigger)
        {
            case EndTrigger.Win:
                ResultTitle.text = "You won! How the f?";
                ResultDescription.text = "Congratulations! Your civilization thrived and you were unharm by it. Thank you for playing.";
                break;
            case EndTrigger.Pollution:
                ResultTitle.text = "Civilization thrives but nature dies. You lose!";
                ResultDescription.text = "Fast growing civilizations produce more pollution. Keep the civilization advance and expansion" +
                    " controlled. Forest retain a little of the pollution but are less effective that rocks. Thank you for playing";
                break;
            case EndTrigger.CivilizationDeath:
                ResultTitle.text = "Ooooouch! You lose!";
                ResultDescription.text = "Keep track of your food resources and be careful upon spreading diseases. Use temperature to control" +
                    " food consumption, diseases effectivity and population increase. Resources to far from the civilization give lower quantities but" +
                    " take longer to consume. Thank you for playing";
                break;
            case EndTrigger.TimeRanOut:
                ResultTitle.text = "Slow progress, you lose!";
                ResultDescription.text = "Civilizations require resources and available space to expand. " +
                    "Check that you have enought conditions for you civilization to thrive. Too many resources or resources to far" +
                    " dont always help. Better luck next time. Thank you for playing.";
                break;
            case EndTrigger.Default:
                ResultTitle.text = "You lose!";
                ResultDescription.text = "Equilibrium is the key. Thank you for playing.";
                break;
        }
    }

    public void Display(string msg, float duration = 3f) {
        IsDisplayingText = true;
        RemainingTextDuration = duration;
        DisplayText.gameObject.SetActive(true);
        DisplayText.text = msg;
    }

    public void OnStartGame() {
        InformationPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnEndGame() {
        SceneManager.LoadScene("Menu");
    }
}
