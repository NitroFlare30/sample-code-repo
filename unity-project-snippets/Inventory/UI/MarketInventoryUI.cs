using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketInventoryUI : InventoryUI
{

    public RectTransform playerAreaPanel;
    protected List<InventoryItemUI> PlayerItemUIs = new List<InventoryItemUI>();

    [SerializeField]
    private InventoryDescriptionUI inventoryDescription;

    protected override void HandleItemSelection(InventoryItemUI obj)
    {
        int index = ItemUIs.IndexOf(obj);
        if (index == -1)
            return;
        //OnItemAction?.Invoke(index);
    }

    public void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
    {
        inventoryDescription.SetDescription(itemImage, name, description);
        DeselectAllItems();
        ItemUIs[itemIndex].Select();
    }

    public void UpdatePlayerInventoryData(int itemIndex, Sprite itemImage, int itemQuantity)
    {
        if (PlayerItemUIs.Count > itemIndex)
            PlayerItemUIs[itemIndex].SetData(itemImage, itemQuantity);
    }
}
