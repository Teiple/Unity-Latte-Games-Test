using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelRequirementUI : MonoBehaviour
{
    [SerializeField] private Image blockImage;
    [SerializeField] private Text chunkCountText;
    [SerializeField] private Image completeCheckImage;

    private void Start()
    {
        completeCheckImage.enabled = false;
    }

    public void SetNumber(Color blockColor, int chunkCount)
    {
        blockImage.color = blockColor;
        chunkCountText.text = chunkCount.ToString();
    }

    public void MarkCompleted()
    {
        blockImage.color = Color.gray;
        chunkCountText.text = "";
        completeCheckImage.enabled = true;
    }
}
