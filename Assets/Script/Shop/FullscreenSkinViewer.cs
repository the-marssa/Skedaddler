using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; 

public sealed class FullscreenSkinViewer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image bigImage;
    [SerializeField] private Image currencyIcon;
    [SerializeField] private TMP_Text priceLabel;
    [SerializeField] private Button buyButton;

    [Header("Optional")]
    [SerializeField] private Button dimmerButton; 

    private SkinData current;

    private void Awake()
    {
        gameObject.SetActive(false);

        if (dimmerButton) dimmerButton.onClick.AddListener(Hide);
        if (buyButton) buyButton.onClick.AddListener(OnBuy);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        
        bool padCancel = Gamepad.current != null && Gamepad.current.bButton.wasPressedThisFrame;

       
        if (esc || padCancel)
            Hide();
    }

    public void Show(SkinData data)
    {
        current = data;
        if (current == null) return;

        if (bigImage)
        {
            bigImage.sprite = current.preview;
            bigImage.preserveAspect = current.preserveAspect;
        }

        if (currencyIcon) currencyIcon.sprite = current.currencyIcon;
        if (priceLabel) priceLabel.text = current.price.ToString();

        
        if (buyButton) buyButton.gameObject.SetActive(!ShopWallet.IsOwned(current.skinId));

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        current = null;
    }

    private void OnBuy()
    {
        if (current == null) return;

        if (ShopWallet.TryBuy(current.skinId, current.price))
        {
            if (buyButton) buyButton.gameObject.SetActive(false);
            Hide(); 
        }
        
    }
}
