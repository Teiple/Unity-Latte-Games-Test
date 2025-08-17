using Jelly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LevelBlock : MonoBehaviour
{
    private Jelly.Block block;
    [SerializeField] private bool isInitialized = false;
    [SerializeField] private string colorCharCodes = "";

    public void Initialize(Jelly.Block block)
    {
        if (isInitialized || block == null)
        {
            return;
        }

        this.block = block;
        colorCharCodes = block.GetColorCharCodes();

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

}
