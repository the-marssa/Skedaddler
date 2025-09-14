using UnityEngine;

[CreateAssetMenu(fileName = "SkinData", menuName = "Shop/Skin Data", order = 0)]
public sealed class SkinData : ScriptableObject
{
    [Header("Identity")]
    public string skinId;           
    public string displayName;

    [Header("Visuals")]
    public Sprite preview;          
    public bool preserveAspect = true;

    [Header("Price")]
    public int price = 100;
    public Sprite currencyIcon;     
}
