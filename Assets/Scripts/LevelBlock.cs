using Jelly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Jelly.Block;

public class LevelBlock : MonoBehaviour
{
    [SerializeField] private bool isInitialized = false;
    private Jelly.Block block;
    
    public Jelly.Block Block { get { return block; } }

    public void Initialize(Jelly.Block block)
    {
        if (isInitialized || block == null)
        {
            return;
        }

        this.block = block;
        
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

        // Register the block removal event
        block.PreparedRemoval += OnBlockPreparedRemoval;
        // Register the block variant changed event
        block.VariantChanged += OnBlockVariantChanged;

        isInitialized = true;
    }


    private void OnBlockPreparedRemoval(Block blockCalled)
    {
        if (block != blockCalled)
        {
            return;
        }

        // Unregister the events
        block.PreparedRemoval -= OnBlockPreparedRemoval;
        block.VariantChanged -= OnBlockVariantChanged;
        
        Destroy(gameObject);
    }

    private void OnBlockVariantChanged(Block blockCalled, Jelly.BlockVariant newVariant)
    {
        if (block != blockCalled)
        {
            return;
        }
        if (newVariant == Jelly.BlockVariant.None)
        {
            OnBlockPreparedRemoval(blockCalled);
            return;
        }

        // Temp: Remove this and create a new block
        block.PreparedRemoval -= OnBlockPreparedRemoval;
        block.VariantChanged -= OnBlockVariantChanged;

        // Create a new block with the new variant
        GameObject newBlockObj = Instantiate(GameSingleton.instance.GetBlockVariantPrefab(newVariant), transform.parent);
        LevelBlock newLevelBlock = newBlockObj.GetComponent<LevelBlock>();
        newLevelBlock.Initialize(blockCalled);

        // Destroy the this block
        Destroy(gameObject);
    }
}
