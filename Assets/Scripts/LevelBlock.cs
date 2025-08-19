using DigitalRuby.Tween;
using Jelly;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        ColorCode[] colorCodes = block.GetChunkColorCodes();
        for (int i = 0; i < chunkCount; i++)
        {
            chunkMeshes[i].material = GameSingleton.instance.GetMaterialForColorCode(colorCodes[i]);
        }
        DoMorph(block.Variant, chunkMeshes);

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
        
        // Destroy(gameObject);
    }

    private void OnBlockVariantChanged(Block blockCalled, List<int> newChunkOrder)
    {
        if (block != blockCalled)
        {
            return;
        }

        Morph(block.Variant, newChunkOrder);
    }

    public void Morph(Jelly.BlockVariant newBlockVariant, List<int> newChunkOrder)
    {
        List<MeshRenderer> chunkMeshes = GetComponentsInChildren<MeshRenderer>().ToList();
        if (newChunkOrder.Count == 0)
        {
            foreach (var chunkMesh in chunkMeshes)
            {
                TweenChunkMeshToZero(chunkMesh);
            }
            return;
        }
        
        List<MeshRenderer> orderedChunkMeshes = new();
        for (int i = 0; i < newChunkOrder.Count; i++)
        {
            chunkMeshes[newChunkOrder[i]].transform.SetSiblingIndex(i);
            orderedChunkMeshes.Add(chunkMeshes[newChunkOrder[i]]);
        }

        foreach (var chunk in chunkMeshes)
        {
            bool ordered = false;
            foreach (var orderedChunk in orderedChunkMeshes)
            {
                if (chunk == orderedChunk)
                {
                    ordered = true;
                    break;
                }
            }
            if (!ordered)
            {
                TweenChunkMeshToZero(chunk);
            }
        }
        
        DoMorph(newBlockVariant, orderedChunkMeshes.ToArray());
    }


    private void DoMorph(Jelly.BlockVariant newBlockVariant, MeshRenderer[] chunkMeshes)
    {
        int endingIndex = 0;
        switch (newBlockVariant)
        {
            case Jelly.BlockVariant.Single:
                {
                    TweenChunkMesh(chunkMeshes[0],
                        new Vector3(0f, 0f, -0.25f),
                        new Vector3(1f, 1f, 0.5f));
                    endingIndex = 1;
                    break;
                }
            case Jelly.BlockVariant.DoubleVertical:
                {
                    TweenChunkMesh(chunkMeshes[0],
                        new Vector3(-0.25f, 0f, -0.25f),
                        new Vector3(0.5f, 1f, 0.5f));
                    TweenChunkMesh(chunkMeshes[1],
                        new Vector3(0.25f, 0f, -0.25f),
                        new Vector3(0.5f, 1f, 0.5f));
                    endingIndex = 2;
                    break;
                }
            case Jelly.BlockVariant.DoubleHorizontal:
                {
                    TweenChunkMesh(chunkMeshes[0],
                        new Vector3(0, 0.25f, -0.25f),
                        new Vector3(1f, 0.5f, 0.5f));
                    TweenChunkMesh(chunkMeshes[1],
                        new Vector3(0, -0.25f, -0.25f),
                        new Vector3(1f, 0.5f, 0.5f));
                    endingIndex = 2;
                    break;
                }
            case Jelly.BlockVariant.TripleLeft:
                {
                    TweenChunkMesh(chunkMeshes[0],
                        new Vector3(-0.25f, 0f, -0.25f),
                        new Vector3(0.5f, 1f, 0.5f));
                    TweenChunkMesh(chunkMeshes[1],
                        new Vector3(0.25f, 0.25f, -0.25f),
                        new Vector3(0.5f, 0.5f, 0.5f));
                    TweenChunkMesh(chunkMeshes[2],
                       new Vector3(0.25f, -0.25f, -0.25f),
                       new Vector3(0.5f, 0.5f, 0.5f));
                    endingIndex = 3;
                    break;
                }
            case Jelly.BlockVariant.TripleTop:
                {
                    TweenChunkMesh(chunkMeshes[0],
                        new Vector3(0f, 0.25f, -0.25f),
                        new Vector3(1f, 0.5f, 0.5f));
                    TweenChunkMesh(chunkMeshes[1],
                        new Vector3(-0.25f, -0.25f, -0.25f),
                        new Vector3(0.5f, 0.5f, 0.5f));
                    TweenChunkMesh(chunkMeshes[2],
                       new Vector3(0.25f, -0.25f, -0.25f),
                       new Vector3(0.5f, 0.5f, 0.5f));
                    endingIndex = 3;
                    break;
                }
            case Jelly.BlockVariant.TripleRight:
                {
                    TweenChunkMesh(chunkMeshes[0],
                        new Vector3(-0.25f, 0.25f, -0.25f),
                        new Vector3(0.5f, 0.5f, 0.5f));
                    TweenChunkMesh(chunkMeshes[1],
                        new Vector3(0.25f, 0f, -0.25f),
                        new Vector3(0.5f, 1f, 0.5f));
                    TweenChunkMesh(chunkMeshes[2],
                       new Vector3(-0.25f, -0.25f, -0.25f),
                       new Vector3(0.5f, 0.5f, 0.5f));
                    endingIndex = 3;
                    break;
                }
            case Jelly.BlockVariant.TripleBottom:
                {
                    TweenChunkMesh(chunkMeshes[0],
                        new Vector3(-0.25f, 0.25f, -0.25f),
                        new Vector3(0.5f, 0.5f, 0.5f));
                    TweenChunkMesh(chunkMeshes[1],
                       new Vector3(0.25f, 0.25f, -0.25f),
                       new Vector3(0.5f, 0.5f, 0.5f));
                    TweenChunkMesh(chunkMeshes[2],
                        new Vector3(0f, -0.25f, -0.25f),
                        new Vector3(1f, 0.5f, 0.5f));
                    endingIndex = 3;
                    break;
                }
        }

        if (chunkMeshes.Length > endingIndex)
            for (int i = endingIndex; i < chunkMeshes.Length; i++)
                TweenChunkMeshToZero(chunkMeshes[i]);
    }


    private void TweenChunkMeshToZero(MeshRenderer chunkMesh)
    {
        chunkMesh.gameObject.Tween(
            $"Scale_{chunkMesh.GetInstanceID()}",
            chunkMesh.transform.localScale,
            Vector3.zero,
            0.5f,
            TweenScaleFunctions.CubicEaseIn,
            (t) =>
            {
                if (chunkMesh != null)
                {
                    chunkMesh.transform.localScale = t.CurrentValue;
                }
            },
            (t) =>
            {
                if (chunkMesh != null)
                {
                    chunkMesh.enabled = false;
                }
            });
    }

    private void TweenChunkMesh(MeshRenderer chunkMesh, Vector3 newLocalPos, Vector3 newLocalScale)
    {
        // Position tween
        chunkMesh.gameObject.Tween(
            $"Move_{chunkMesh.GetInstanceID()}",
            chunkMesh.transform.localPosition,
            newLocalPos,
            0.5f,
            TweenScaleFunctions.CubicEaseInOut,
            (t) =>
            {
                if (chunkMesh != null)
                {
                    chunkMesh.transform.localPosition = t.CurrentValue;
                }
            });

        // Scale tween
        chunkMesh.gameObject.Tween(
            $"Scale_{chunkMesh.GetInstanceID()}",
            chunkMesh.transform.localScale,
            newLocalScale,
            0.5f,
            TweenScaleFunctions.CubicEaseInOut,
            (t) =>
            {
                if (chunkMesh != null)
                {
                    chunkMesh.transform.localScale = t.CurrentValue;
                }
            },
            (t) =>
            {
                if (chunkMesh != null)
                {
                    if (newLocalScale == Vector3.zero)
                    {
                        chunkMesh.enabled = false;
                    }
                    else
                    {
                        chunkMesh.enabled = true;
                    }
                }
            });
    }

}
