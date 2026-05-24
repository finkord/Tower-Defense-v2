using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

[System.Serializable]
public class PvPEnemyButtonMapping
{
    public Button button;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI priceText;
    public GameObject enemyPrefab; 
    [HideInInspector] public int currentQueued = 0;
    [HideInInspector] public EnemySpawnConfig config;
}

public class PvPManager : MonoBehaviour
{
    public static PvPManager Instance;

    [Header("References")]
    public WaveManagerV2 waveManager;
    
    [Header("UI Panels")]
    public GameObject attackerPanel; 
    public GameObject[] defenderUIElements; // Drag TowerBTNs here
    
    [Header("Attacker Budget")]
    public int currentBudget;
    public TextMeshProUGUI budgetTxt;
    
    [Header("Enemy Buttons Setup")]
    public PvPEnemyButtonMapping[] enemyMappings;

    [Header("Controls")]
    public Button confirmWaveBTN;
    public Button x10Button; // New UI toggle button

    private bool isX10Active = false;

    [HideInInspector]
    public List<EnemySpawnConfig> pvpWaveQueue = new List<EnemySpawnConfig>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (confirmWaveBTN != null)
        {
            confirmWaveBTN.onClick.RemoveAllListeners();
            confirmWaveBTN.onClick.AddListener(ConfirmWaveAndPassTurn);
        }

        if (x10Button != null)
        {
            x10Button.onClick.AddListener(ToggleX10);
        }

        if (waveManager != null && waveManager.waveMode == WaveMode.PvPHotSeat)
        {
            InitializeAttackerUI();
            StartAttackerTurn();
        }
        else
        {
            if (attackerPanel != null) attackerPanel.SetActive(false);
            SetDefenderUIActive(true);
        }
    }

    private void InitializeAttackerUI()
    {
        foreach (var mapping in enemyMappings)
        {
            if (mapping.enemyPrefab == null || waveManager == null) continue;

            mapping.config = waveManager.availableEnemies.FirstOrDefault(e => e.prefab == mapping.enemyPrefab);
            
            if (mapping.config != null)
            {
                int cost = mapping.config.data.reward; // Cost equals the reward
                if (mapping.priceText != null) mapping.priceText.text = cost.ToString();
                
                if (mapping.button != null)
                {
                    mapping.button.onClick.AddListener(() => AddEnemyToQueue(mapping));
                }
            }
        }
    }

    public void StartAttackerTurn()
    {
        pvpWaveQueue.Clear();
        foreach (var mapping in enemyMappings)
        {
            mapping.currentQueued = 0;
            if (mapping.countText != null) mapping.countText.text = "0";
        }

        currentBudget = waveManager.CalculateBudget(waveManager.currentWaveIndex);
        UpdateBudgetUI();
        
        if (attackerPanel != null) attackerPanel.SetActive(true);
        SetDefenderUIActive(false);
    }

    private void AddEnemyToQueue(PvPEnemyButtonMapping mapping)
    {
        if (mapping.config == null) return;
        
        int cost = mapping.config.data.reward;
        int amountToAdd = 1;

        // If holding shift OR UI button is toggled, try to add 10 at a time
        if (isX10Active || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            amountToAdd = 10;
        }

        // Limit the amount to what the attacker can actually afford
        int maxAffordable = currentBudget / cost;
        amountToAdd = Mathf.Min(amountToAdd, maxAffordable);

        if (amountToAdd > 0)
        {
            currentBudget -= (cost * amountToAdd);
            
            for (int i = 0; i < amountToAdd; i++)
            {
                pvpWaveQueue.Add(mapping.config);
            }
            
            mapping.currentQueued += amountToAdd;
            
            if (mapping.countText != null) mapping.countText.text = mapping.currentQueued.ToString();
            UpdateBudgetUI();
        }
        else
        {
            Debug.Log("Not enough budget!");
        }
    }

    private void UpdateBudgetUI()
    {
        if (budgetTxt != null)
        {
            budgetTxt.text = $"Budget: {currentBudget}";
        }
    }

    public void ConfirmWaveAndPassTurn()
    {
        if (pvpWaveQueue.Count == 0)
        {
            Debug.LogWarning("Cannot send an empty wave! Add enemies first.");
            return;
        }

        if (attackerPanel != null) attackerPanel.SetActive(false);
        SetDefenderUIActive(true);
    }

    public void ToggleX10()
    {
        isX10Active = !isX10Active;
        
        // Optional: Change button color to indicate it's active
        if (x10Button != null)
        {
            var colors = x10Button.colors;
            colors.normalColor = isX10Active ? Color.gray : Color.white;
            colors.selectedColor = isX10Active ? Color.gray : Color.white;
            x10Button.colors = colors;
        }
    }

    private void SetDefenderUIActive(bool active)
    {
        // Toggle specific UI elements (like tower buttons) off during attacker turn
        if (defenderUIElements != null)
        {
            foreach (var ui in defenderUIElements)
            {
                if (ui != null) ui.SetActive(active);
            }
        }
        
        if (waveManager != null && waveManager.startWaveButton != null) 
        {
            waveManager.startWaveButton.gameObject.SetActive(active);
            if (active) waveManager.startWaveButton.interactable = true;
        }

        // Enable or disable TowerPlacer so the attacker cannot place/delete towers
        TowerPlacer tp = FindObjectOfType<TowerPlacer>();
        if (tp != null) tp.enabled = active;
        if (!active) TowerSelectionUI.SelectedTowerPrefab = null;
    }
}
