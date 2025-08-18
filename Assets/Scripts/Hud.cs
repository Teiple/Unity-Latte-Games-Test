using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [SerializeField] private LevelRequirementUI levelRequirementUIPrefab;
    [SerializeField] private GridLayoutGroup levelRequirementGrid;

    private void Start()
    {
        foreach (var requirement in GameSingleton.instance.LevelRequirements)
        {
            LevelRequirementUI requirmentUIGameObj = Instantiate<LevelRequirementUI>(levelRequirementUIPrefab, levelRequirementGrid.transform);
            Color color = GameSingleton.instance.GetColorForColorCode(requirement.colorCode);
            requirmentUIGameObj.SetNumber(color, requirement.minNumber);
        }
    }


    public void MarkRequirementComplete(int requirmentIndex)
    {
        LevelRequirementUI requirementUI = levelRequirementGrid.transform.GetChild(requirmentIndex).GetComponent<LevelRequirementUI>();
        requirementUI.MarkCompleted();
    } 
}
