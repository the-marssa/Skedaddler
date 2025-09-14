using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BalanceDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceLabel;
    [SerializeField] private Button addTestButton; 

    private void OnEnable()
    {
        ShopWallet.OnBalanceChanged += Refresh;
        Refresh(ShopWallet.Balance);
    }
    private void OnDisable()
    {
        ShopWallet.OnBalanceChanged -= Refresh;
    }
    private void Start()
    {
        if (addTestButton)
            addTestButton.onClick.AddListener(() => ShopWallet.AddBalance(100));
    }

    private void Refresh(int bal)
    {
        if (balanceLabel) balanceLabel.text = $"{bal}";
    }
}
