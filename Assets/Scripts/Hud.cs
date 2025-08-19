using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DigitalRuby.Tween;

public class Hud : MonoBehaviour
{
    [SerializeField] private LevelRequirementUI levelRequirementUIPrefab;
    [SerializeField] private GridLayoutGroup levelRequirementGrid;
    [SerializeField] private Text levelIndicator;
    [SerializeField] private CanvasGroup gameOverPanel;
    [SerializeField] private LayoutElement outOfSpaceGameOverLayout;
    [SerializeField] private LayoutElement levelCompleteGameOverLayout;
    [SerializeField] private ComboNotifUI comboNotifUI;
    [SerializeField] private Text totalCoinsText;


    private void Start()
    {
        foreach (var requirement in GameSingleton.instance.LevelRequirements)
        {
            LevelRequirementUI requirmentUIGameObj = Instantiate<LevelRequirementUI>(levelRequirementUIPrefab, levelRequirementGrid.transform);
            Color color = GameSingleton.instance.GetColorForColorCode(requirement.colorCode);
            requirmentUIGameObj.SetNumber(requirement.minNumber);
            requirmentUIGameObj.SetColor(color);
        }
        levelIndicator.text = $"Level {GameSingleton.instance.GetCurrentLevelNumber().ToString()}";
        
        gameOverPanel.gameObject.SetActive(false);
        
        outOfSpaceGameOverLayout.ignoreLayout = true;
        outOfSpaceGameOverLayout.gameObject.SetActive(false);
        
        levelCompleteGameOverLayout.ignoreLayout = true;
        levelCompleteGameOverLayout.gameObject.SetActive(false);
    }


    public void SetRequirementNumber(int requirementIndex, int requirementNumber)
    {
        LevelRequirementUI requirementUI = levelRequirementGrid.transform.GetChild(requirementIndex).GetComponent<LevelRequirementUI>();
        requirementUI.SetNumber(requirementNumber);
    }

    public void MarkRequirementComplete(int requirementIndex)
    {
        LevelRequirementUI requirementUI = levelRequirementGrid.transform.GetChild(requirementIndex).GetComponent<LevelRequirementUI>();
        requirementUI.MarkCompleted();
    }

    public void OnNextLevelPressed()
    {
        GameSingleton.instance.LoadNextLevel();
    }

    public void OnPreviousLevelPressed()
    {
        GameSingleton.instance.LoadPreviousLevel();
    }

    public void OnRestartPressed()
    {
        GameSingleton.instance.ReloadLevel();
    }

    public void ShowGameOverPanel(GameSingleton.GameOverReason reason)
    {
        if (gameOverPanel.gameObject.activeInHierarchy)
        {
            return;
        }

        gameOverPanel.alpha = 0;
        gameOverPanel.gameObject.SetActive(true);

        if (reason == GameSingleton.GameOverReason.OutOfSpace)
        {
            outOfSpaceGameOverLayout.ignoreLayout = false;
            outOfSpaceGameOverLayout.gameObject.SetActive(true);
        }
        else if (reason == GameSingleton.GameOverReason.LevelComplete)
        {
            levelCompleteGameOverLayout.ignoreLayout = false;
            levelCompleteGameOverLayout.gameObject.SetActive(true);
        }

        gameOverPanel.gameObject.Tween(
            $"Fade{gameOverPanel.GetInstanceID()}",
            gameOverPanel.alpha,
            1.0f,
            0.5f, // duration
            TweenScaleFunctions.CubicEaseInOut,
            (t) =>
            {
                gameOverPanel.alpha = t.CurrentValue;
            });
    }

    public void NotifyCombo(int combo, int rewards)
    {
        comboNotifUI.SetCombo(combo, rewards);
    }

    public void UpdateTotalCoins(int number)
    {
        totalCoinsText.text = number.ToString();
    }
}
