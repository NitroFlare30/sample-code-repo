using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageInventoryController : InventoryController, IWorldObject
{
    protected StorageInventoryUI StorageInventoryUI => InventoryUI as StorageInventoryUI;

    [SerializeField]
    protected Inventory playerInventoryData;

    protected override void PrepareData()
    {
        playerInventoryData.OnInventoryUpdated += UpdateInventoryUI;
        base.PrepareData();
    }

    protected override void PrepareUI()
    {
        InventoryUI.InitInventoryUI(inventoryData.Size);

        StorageInventoryUI.OnStorageClickAction += HandleStorageClickAction;
    }

    protected override void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        StorageInventoryUI.ResetAllItems();
        foreach (var item in inventoryData.GetCurrentInventoryState())
        {
            InventoryUI.UpdateData(item.Key, item.Value.item != null ? item.Value.item.GameSprite : null, item.Value.quantity);
        }
        foreach (var item in playerInventoryData.GetCurrentInventoryState())
        {
            StorageInventoryUI.UpdatePlayerInventoryData(item.Key, item.Value.item != null ? item.Value.item.GameSprite : null, item.Value.quantity);
        }
    }

    public override void ForceUpdateInventoryUI()
    {
        base.ForceUpdateInventoryUI();
        foreach (var item in playerInventoryData.GetCurrentInventoryState())
        {
            StorageInventoryUI.UpdatePlayerInventoryData(item.Key, item.Value.item != null ? item.Value.item.GameSprite : null, item.Value.quantity);
        }
    }

    protected void HandleStorageClickAction(RectTransform rectTransform, int itemIndex)
    {
        InventoryItem inventoryItem;
        
        // Player Inventory -> Storage Inventory
        if (rectTransform == StorageInventoryUI.playerAreaPanel)
        {
            inventoryItem = playerInventoryData.GetItemAt(itemIndex);
            Debug.Log("PlayerInventory (" + itemIndex + "): " + inventoryItem.item.ItemName + " x1 -> StorageInventory");

            inventoryData.AddItem(inventoryItem, 1);
            playerInventoryData.RemoveItem(itemIndex, 1);
        }
        // Storage Inventory -> Player Inventory
        else
        {
            inventoryItem = inventoryData.GetItemAt(itemIndex);
            Debug.Log("StorageInventory (" + itemIndex + "): " + inventoryItem.item.ItemName + " x1 -> PlayerInventory");

            playerInventoryData.AddItem(inventoryItem, 1);
            inventoryData.RemoveItem(itemIndex, 1);
        }
    }

    public void Interact()
    {
        ForceUpdateInventoryUI();
        StorageInventoryUI.Show();
    }
}
