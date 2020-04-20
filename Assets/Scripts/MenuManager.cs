using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject HiddenPanel;
    public bool IsLoadingScene = false;
    public GameObject StartingOptions;
    public GameObject LoadingText;
    public AudioOnClick Audio;

    private void Start()
    {
        
    }

    void Update()
    {
        
    }

    IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void OnClickOnSheep() {
        Audio.PlayClick();
        HiddenPanel.gameObject.SetActive(!HiddenPanel.activeInHierarchy);
    }

    public void OnEasyModeSelection() {
        Audio.PlayClick();
        if (!IsLoadingScene)
        {
            LoadScene(0);
        }
    }

    public void OnNormalModeSelection() {
        Audio.PlayClick();
        if (!IsLoadingScene) {
            LoadScene(1);
        }
    }

    private void LoadScene(int dificulty) {
        IsLoadingScene = true;
        PlayerPrefs.SetInt("Difficulty", dificulty);
        StartingOptions.SetActive(false);
        LoadingText.SetActive(true);
        StartCoroutine(LoadYourAsyncScene());
    }
}
