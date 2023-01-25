using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageInventoryUI : InventoryUI
{
    public RectTransform playerAreaPanel;
    protected List<InventoryItemUI> PlayerItemUIs = new List<InventoryItemUI>();

    public Action<RectTransform, int> OnStorageClickAction;

    protected override void Start()
    {
        base.Start();
    }

    public override void InitInventoryUI(int inventorySize)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            InventoryItemUI item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            item.transform.SetParent(areaPanel);
            ItemUIs.Add(item);
            item.OnItemClicked += HandleStorageItemSelection;
            item.OnItemBeginDrag += HandleBeginDrag;
            item.OnItemDroppedOn += HandleSwap;
            item.OnItemEndDrag += HandleEndDrag;
        }
        for (int i = 0; i < inventorySize; i++)
        {
            InventoryItemUI item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            item.transform.SetParent(playerAreaPanel);
            PlayerItemUIs.Add(item);
            item.OnItemClicked += HandleStorageItemSelection;
            //item.OnItemBeginDrag += HandleBeginDrag;
            //item.OnItemDroppedOn += HandleSwap;
            //item.OnItemEndDrag += HandleEndDrag;

        }
    }

    public void UpdatePlayerInventoryData(int itemIndex, Sprite itemImage, int itemQuantity)
    {
        if (PlayerItemUIs.Count > itemIndex)
            PlayerItemUIs[itemIndex].SetData(itemImage, itemQuantity);
    }

    public override void Show()
    {
        base.Show();
        playerAreaPanel.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        playerAreaPanel.gameObject.SetActive(false);
        base.Hide();
    }

    private void HandleStorageItemSelection(InventoryItemUI itemUI)
    {
        // Player Inventory -> Storage Inventory
        if (PlayerItemUIs.Contains(itemUI))
            OnStorageClickAction?.Invoke(playerAreaPanel, PlayerItemUIs.IndexOf(itemUI));
        // Storage Inventory -> Player Inventory
        else
            OnStorageClickAction?.Invoke(areaPanel, ItemUIs.IndexOf(itemUI));
    }

    public override void ResetAllItems()
    {
        base.ResetAllItems();
        foreach (var item in PlayerItemUIs)
        {
            item.ResetData();
            item.Deselect();
        }
    }
}
