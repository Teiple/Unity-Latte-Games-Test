using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSingleton : MonoBehaviour
{
    [System.Serializable]
    struct LevelBlockVariantPrefab
    {
        public Jelly.BlockVariant variant;
        public GameObject prefab;
    }

    [System.Serializable]
    public struct JellyColor
    {
        public Jelly.ColorCode colorCode;
        public Material materialForChunk;
    }


    public static GameSingleton instance;


    private LevelGrid currentLevelGrid;
    
    [SerializeField] private JellyColor[] jellyColorMaterials;
    [SerializeField] private LevelBlockVariantPrefab[] blockVariantPrefabs;
    [SerializeField] private Material defaultChunkMaterial;

    public LevelGrid CurrentLevelGrid { get { return currentLevelGrid; } }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentLevelGrid = FindObjectOfType<LevelGrid>();
    }

    
    public Material GetMaterialForColorCode(Jelly.ColorCode colorCode)
    {
        foreach (var jellyColor in jellyColorMaterials)
        {
            if (jellyColor.colorCode == colorCode)
            {
                return jellyColor.materialForChunk;
            }
        }
        return defaultChunkMaterial;
    }

    public GameObject GetBlockVariantPrefab(Jelly.BlockVariant variant)
    {
        foreach (var blockVariantPrefab in blockVariantPrefabs)
        {
            if (blockVariantPrefab.variant == variant)
            {
                return blockVariantPrefab.prefab;
            }
        }
        return null;
    }
}