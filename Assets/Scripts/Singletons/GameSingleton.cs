using Jelly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameSingleton : MonoBehaviour
{
    const int MAX_LEVELS = 3;

    public enum GameOverReason
    {
        OutOfSpace = 0,
        LevelComplete = 1,
    }


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
    [SerializeField] private LevelBlock levelBlockBasePrefab;
    [SerializeField] private Material defaultChunkMaterial;
    [SerializeField] private LevelRequirement[] levelRequirements;
    
    private LevelGrid currentLevelGrid;
    private Hud currentHud;
    private Dictionary<Jelly.ColorCode, int> currentProgress;
    private string currentLevel;
    private bool gameOver;
    private int totalCoins;


    public LevelGrid CurrentLevelGrid { get { return currentLevelGrid; } }
    public Dictionary<Jelly.ColorCode, int> CurrentProgress { get { return currentProgress; } }
    public LevelRequirement[] LevelRequirements { get { return levelRequirements; } }
    public bool GameOver {  get { return gameOver; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Copy the level requirements for GameSingleton instance
            instance.UpdateProperties(this);
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        totalCoins = 0;
        Initialize();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
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

    public LevelBlock GetLevelBlockBasePrefab(Jelly.BlockVariant variant)
    {
        //foreach (var blockVariantPrefab in blockVariantPrefabs)
        //{
        //    if (blockVariantPrefab.variant == variant)
        //    {
        //        return blockVariantPrefab.prefab;
        //    }
        //}
        return levelBlockBasePrefab;
    }

    public void AddProgress(Chunk[] removedChunks)
    {
        if (gameOver)
        {
            return;
        }

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

        if (EvaluateProgress())
        {
            SetGameOver(GameOverReason.LevelComplete);
        } else if (currentLevelGrid.IsGridFull)
        {
            SetGameOver(GameOverReason.OutOfSpace);
        }
    }
    
    public void LoadNextLevel()
    {
        LoadLevel(1);
    }

    public void LoadPreviousLevel()
    {
        LoadLevel(-1);
    }


    public void ReloadLevel()
    {
        LoadScene(currentLevel);
    }

    public int GetCurrentLevelNumber()
    {
        if (Int32.TryParse(currentLevel.Substring(currentLevel.Length - 2, 2), out int levelNumber))
        {
            return levelNumber;
        }
        return -1;
    }

    public void NotifyCombo(int combo)
    {
        if (combo >= 3)
        {
            int rewards = combo * 20;
            currentHud.NotifyCombo(combo, rewards);
            totalCoins += rewards;
            currentHud.UpdateTotalCoins(totalCoins);
        }
    }


    private void LoadLevel(int increment)
    {
        int levelNumber = GetCurrentLevelNumber() + increment;
        if (levelNumber > 0 && levelNumber <= MAX_LEVELS)
        {
            string nextLevel = $"Level{(levelNumber).ToString("D2")}";
            if (SceneExists(nextLevel))
            {
                LoadScene(nextLevel);
            }
        }
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
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
                    if (currentNumber > 0)
                    {
                        currentHud.SetRequirementNumber(i, requiredNumber - currentNumber);
                    }
                    goalReached = false;
                }
                else
                {
                    currentHud.MarkRequirementComplete(i);
                }
            }
            else
            {
                goalReached = false;
            }
        }
        return goalReached;
    }

    private bool SceneExists(string sceneName)
    {
        int index = SceneUtility.GetBuildIndexByScenePath(sceneName);
        return index != -1;
    }

    private void Initialize()
    {
        currentLevelGrid = FindObjectOfType<LevelGrid>();
        currentLevel = GetCurrentSceneName();
        currentHud = FindObjectOfType<Hud>();
        currentProgress = new();
        gameOver = false;

        currentHud.UpdateTotalCoins(totalCoins);
    }

    private void UpdateProperties(GameSingleton other)
    {
        levelRequirements = other.levelRequirements;
    }

    private void SetGameOver(GameOverReason reason)
    {
        gameOver = true;
        if (reason == GameOverReason.LevelComplete)
        {
            totalCoins += 50;
        }
        currentHud.UpdateTotalCoins(totalCoins);
        currentHud.ShowGameOverPanel(reason);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Initialize();
    }
}