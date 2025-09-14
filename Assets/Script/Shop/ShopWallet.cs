using UnityEngine;
using System;

public static class ShopWallet
{
    private const string BalanceKey = "WALLET_BALANCE";
    private const string OwnedKey = "OWNED_"; 
    private const int DefaultBalance = 500;

    public static event Action<int> OnBalanceChanged;
    public static event Action<string> OnPurchased; 

    public static int Balance
    {
        get
        {
            if (!PlayerPrefs.HasKey(BalanceKey))
                PlayerPrefs.SetInt(BalanceKey, DefaultBalance);
            return PlayerPrefs.GetInt(BalanceKey, DefaultBalance);
        }
    }

    public static bool IsOwned(string skinId)
        => PlayerPrefs.GetInt(OwnedKey + skinId, 0) == 1;

    public static bool TryBuy(string skinId, int price)
    {
        if (IsOwned(skinId)) return true;
        int bal = Balance;
        if (bal < price) return false;

        bal -= price;
        PlayerPrefs.SetInt(BalanceKey, bal);
        PlayerPrefs.SetInt(OwnedKey + skinId, 1);
        PlayerPrefs.Save();

        OnBalanceChanged?.Invoke(bal);
        OnPurchased?.Invoke(skinId);
        return true;
    }

    
    public static void AddBalance(int amount)
    {
        int bal = Balance + Mathf.Max(0, amount);
        PlayerPrefs.SetInt(BalanceKey, bal);
        PlayerPrefs.Save();
        OnBalanceChanged?.Invoke(bal);
    }
}
