using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    public int coins;
    public TextMeshProUGUI coinTxt;

    private void Awake()
    {
        instance = this;
        UpdateCoins(0);
    }

    public void UpdateCoins(int changeAmount)
    {
        coins += changeAmount;
        
        if (coinTxt != null) coinTxt.text = coins.ToString();

        // Dynamically update tower selection UI colors when coins change
        if (TowerSelectionUI.Instance != null)
        {
            TowerSelectionUI.Instance.UpdateHighlights();
        }
    }
}