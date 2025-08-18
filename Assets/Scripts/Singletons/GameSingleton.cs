using Jelly;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
        public Color color;
        public Material materialForChunk;
    }

    [System.Serializable]
    public struct LevelRequirement
    {
        public Jelly.ColorCode colorCode;
        public int minNumber;
    }


    public static GameSingleton instance;


    [SerializeField] private JellyColor[] jellyColorMaterials;
    [SerializeField] private LevelBlockVariantPrefab[] blockVariantPrefabs;
    [SerializeField] private Material defaultChunkMaterial;
    [SerializeField] private LevelRequirement[] levelRequirements;
    [SerializeField] private Hud currentHud;
    
    private LevelGrid currentLevelGrid;
    private Dictionary<Jelly.ColorCode, int> currentProgress;

    public LevelGrid CurrentLevelGrid { get { return currentLevelGrid; } }
    public Dictionary<Jelly.ColorCode, int> CurrentProgress { get { return currentProgress; } }
    public LevelRequirement[] LevelRequirements { get { return levelRequirements; } }
    

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
        currentHud = FindAnyObjectByType<Hud>();
        currentProgress = new();
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

    public Color GetColorForColorCode(Jelly.ColorCode colorCode)
    {
        foreach (var jellyColor in jellyColorMaterials)
        {
            if (jellyColor.colorCode == colorCode)
            {
                return jellyColor.color;
            }
        }
        return Color.white;
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

    public void AddProgress(Chunk[] removedChunks)
    {
        for (int i = 0; i < removedChunks.Length; i++)
        {
            Jelly.ColorCode colorCode = removedChunks[i].ColorCode;
            if (!currentProgress.ContainsKey(colorCode))
            {
                currentProgress[colorCode] = 1;
            } else
            {
                currentProgress[colorCode]++;
            }
        }
        EvaluateProgress();
    }

    
    private bool EvaluateProgress()
    {
        bool goalReached = true;
        for (int i = 0; i < levelRequirements.Length; i++)
        {
            Jelly.ColorCode colorCode = levelRequirements[i].colorCode;
            int requiredNumber = levelRequirements[i].minNumber;
            if (currentProgress.TryGetValue(colorCode, out int currentNumber))
            {
                if (currentNumber < requiredNumber)
                {
                    goalReached = false;
                } else
                {
                    currentHud.MarkRequirementComplete(i);
                }
            } else
            {
                goalReached = false;
            }
        }
        return goalReached;
    }
}