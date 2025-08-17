using Jelly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LevelBlock : MonoBehaviour
{
    private Jelly.Block block;
    [SerializeField] private bool isInitialized = false;
    // Example color codes, can be set in the inspector
    [SerializeField] private string colorCharCodes = "rrggbbyy";

    public void Initialize(string colorCharCodes)
    {
        if (isInitialized)
        {
            return;
        }

        this.colorCharCodes = colorCharCodes;
        block = new Jelly.Block(colorCharCodes);

        int chunkCount = block.GetChunkCount();
        MeshRenderer[] chunkMeshes = GetComponentsInChildren<MeshRenderer>();
        if (chunkMeshes.Length != chunkCount)
        {
            return;
        }

        ColorCode[] colorCodes = block.GetChunkColorCodes();
        for (int i = 0; i < chunkCount; i++)
        {
            chunkMeshes[i].material = GameSingleton.instance.GetMaterialForColorCode(colorCodes[i]);
        }

        isInitialized = true;
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Initialize(colorCharCodes);
            isInitialized = true;
        }
    }
}
