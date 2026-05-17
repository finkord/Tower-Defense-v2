using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class TowerButtonData
{
    public GameObject prefab;
    public Image buttonImage;
}

public class TowerSelectionUI : MonoBehaviour
{
    public static TowerSelectionUI Instance;

    [Header("Highlight Settings")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.7f, 1f, 0.7f); // Light green
    public Color unaffordableColor = new Color(1f, 0.5f, 0.5f); // Red
    public List<TowerButtonData> towerButtons = new List<TowerButtonData>();

    [Header("Icon Settings")]
    public float iconScale = 1f;
    public float pixelsPerUnit = 100f; // Standard Unity PPU

    private void Awake()
    {
        Instance = this;
        UpdateHighlights();
    }

    private void Start()
    {
        GenerateTowerIcons();
        UpdateTowerPrices();
    }

    private void UpdateTowerPrices()
    {
        foreach (var btnData in towerButtons)
        {
            if (btnData.prefab != null && btnData.buttonImage != null)
            {
                Tower towerScript = btnData.prefab.GetComponentInChildren<Tower>();
                if (towerScript != null && towerScript.data != null)
                {
                    Transform priceTransform = btnData.buttonImage.transform.Find("Price");
                    if (priceTransform != null)
                    {
                        var textComponent = priceTransform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                        if (textComponent != null)
                        {
                            textComponent.text = towerScript.data.towerPrice.ToString();
                        }
                    }
                }
            }
        }
    }

    private void GenerateTowerIcons()
    {
        foreach (var btnData in towerButtons)
        {
            if (btnData.prefab != null && btnData.buttonImage != null)
            {
                Transform iconTransform = btnData.buttonImage.transform.Find("Icon");
                if (iconTransform != null)
                {
                    // Disable original single-image icon
                    Image defaultImg = iconTransform.GetComponent<Image>();
                    if (defaultImg != null) defaultImg.enabled = false;

                    // Generate full prefab hierarchy using UI Images
                    GameObject root = new GameObject("PrefabVisuals");
                    RectTransform rootRT = root.AddComponent<RectTransform>();
                    rootRT.SetParent(iconTransform, false);
                    rootRT.localPosition = Vector3.zero;
                    rootRT.localScale = Vector3.one * iconScale;

                    CloneSpriteToUI(btnData.prefab, rootRT);
                }
            }
        }
    }

    private void CloneSpriteToUI(GameObject prefabNode, Transform parentUI)
    {
        GameObject uiNode = new GameObject(prefabNode.name);
        RectTransform rt = uiNode.AddComponent<RectTransform>();
        rt.SetParent(parentUI, false);

        // Convert world units to UI pixels using PPU
        rt.localPosition = prefabNode.transform.localPosition * pixelsPerUnit;
        rt.localRotation = prefabNode.transform.localRotation;
        rt.localScale = prefabNode.transform.localScale;

        SpriteRenderer sr = prefabNode.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            Image img = uiNode.AddComponent<Image>();
            img.sprite = sr.sprite;
            img.color = sr.color;
            img.raycastTarget = false;
            img.SetNativeSize(); 
        }

        foreach (Transform child in prefabNode.transform)
        {
            CloneSpriteToUI(child.gameObject, uiNode.transform);
        }
    }

    private static GameObject _selectedTowerPrefab;
    public static GameObject SelectedTowerPrefab
    {
        get { return _selectedTowerPrefab; }
        set
        {
            _selectedTowerPrefab = value;
            if (Instance != null) Instance.UpdateHighlights();
        }
    }

    public void SelectedTower(GameObject towerPrefab)
    {
        if (towerPrefab == SelectedTowerPrefab)
        {
            SelectedTowerPrefab = null;
            return;
        }

        // Get Tower component from prefab
        Tower towerScript = towerPrefab.GetComponentInChildren<Tower>();

        // Check if component and its ScriptableObject data exist
        if (towerScript != null && towerScript.data != null)
        {
            // Allow selection regardless of price; highlighting handles feedback
            SelectedTowerPrefab = towerPrefab;
        }
    }

    public void UpdateHighlights()
    {
        foreach (var btnData in towerButtons)
        {
            if (btnData.buttonImage != null)
            {
                if (btnData.prefab == _selectedTowerPrefab)
                {
                    bool canAfford = true;
                    Tower towerScript = btnData.prefab.GetComponentInChildren<Tower>();
                    if (towerScript != null && towerScript.data != null)
                    {
                        canAfford = CoinManager.instance.coins >= towerScript.data.towerPrice;
                    }
                    
                    btnData.buttonImage.color = canAfford ? selectedColor : unaffordableColor;
                }
                else
                {
                    btnData.buttonImage.color = normalColor;
                }
            }
        }
    }
}