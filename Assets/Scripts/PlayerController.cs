using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerAction
    {
        None,
        GenerateTrees,
        GenerateRocks,
        GenerateAnimals,
        IncreaseTemperature,
        DecreaseTemperature,
        ReleaseBacteria,
        ReleaseVirus,
        CreateEarthquake,
        CreateWildfire,
        CreateStorm
    }

    public float MaximumWill = 100f;
    public float CurrentWill = 0f;
    public float WillRecoveryRate = 10f;
    public float CurrentPollution = 0f;

    public GameController GameController;
    public CivilizationController Civilization;

    public TextMeshProUGUI WillText;
    public TextMeshProUGUI PollutionText;

    public PlayerAction CurrentIntent;
    private ActionManager SourceAction;
    

    void Start()
    {
        WillText.text = ((int)CurrentWill).ToString();
        PollutionText.text = ((int)(CurrentPollution * 100))+"%";
    }
    
    void Update()
    {
        CurrentWill += (WillRecoveryRate) * Time.deltaTime;
        if (CurrentWill > MaximumWill) {
            CurrentWill = MaximumWill;
        }
        WillText.text = ((int)CurrentWill).ToString();
        PollutionText.text = ((int)(CurrentPollution * 100)) + "%";
    }

    public void IncreasePollution(float n) {
        CurrentPollution += n;
    }

    public bool SetIntent(PlayerAction action, ActionManager source) {
        //TODO: Add earthquakes, wildfires and storms
        if (CurrentWill >= source.ActionCost)
        {
            switch (action)
            {
                case PlayerAction.IncreaseTemperature:
                    IncreaseTemperature(source);
                    break;
                case PlayerAction.DecreaseTemperature:
                    DecreaseTemperature(source);
                    break;
                case PlayerAction.ReleaseBacteria:
                    ReleaseBacteria(source);
                    break;
                case PlayerAction.ReleaseVirus:
                    ReleaseVirus(source);
                    break;
                default:
                    CurrentIntent = action;
                    SourceAction = source;
                    break;
            }
            return true;
        }
        else {
            GameController.Display("Not enought will to use " + source.ActionName);
        }
        return false;
    }

    public void ConsumeIntent() {
        CurrentWill -= SourceAction.ActionCost;
        CurrentIntent = PlayerAction.None;
        SourceAction = null;
    }

    public void IncreaseTemperature(ActionManager source) {
        if (GameController.CurrentTemperature < 40)
        {
            CurrentWill -= source.ActionCost;
            GameController.SetTemperature(+1);
        }
        else
        {
            GameController.Display("Temperature is at maximum");
        }
    }

    public void DecreaseTemperature(ActionManager source)
    {
        if (GameController.CurrentTemperature > -10)
        {
            CurrentWill -= source.ActionCost;
            GameController.SetTemperature(-1);
        }
        else
        {
            GameController.Display("Temperature is at minimum");
        }
    }

    private void ReleaseBacteria(ActionManager source)
    {
        CurrentWill -= source.ActionCost;
        Civilization.ActivateBacteria();
    }
    private void ReleaseVirus(ActionManager source)
    {
        CurrentWill -= source.ActionCost;
        Civilization.ActivateVirus();
    }
}
