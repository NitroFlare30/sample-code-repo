using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryController : InventoryController
{
    protected const int PLAYER_INVENTORY_SIZE = 32;

    public static PlayerInventoryController Instance { get; private set; }

    public event Action OnPlayerInventoryChange;

    [field: SerializeField]
    public InventoryItem EquippedItemData { get; set; }
    public Item EquippedItem => EquippedItemData.item;

    [SerializeField]
    private Image tempEquippedItemImage;
    [SerializeField]
    private Sprite maskSprite;

    private PlayerInventoryUI PlayerInventoryUI => InventoryUI as PlayerInventoryUI;

    private void Awake()
    {
        Instance = this;
    }

    protected override void PrepareUI()
    {
        base.PrepareUI();
        EquippedItemData = InventoryItem.GetEmptyItem();
    }

    private void HandleDescriptionRequest(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;
        Item item = inventoryItem.item;
        PlayerInventoryUI.UpdateDescription(itemIndex, item.GameSprite, item.ItemName, item.Description);
    }

    public void SetEquippedItem(int index)
    {
        EquippedItemData = inventoryData.GetItemAt(index);
    }

    public bool CheckForItems(List<QuantifiedItem> items)
    {
        foreach (QuantifiedItem item in items)
        {
            if (inventoryData.CheckForItem(item.item, item.quantity) == false)
                return false;
        }
        return true;
    }

    public void AddItemToInventory(QuantifiedItem itemToAdd)
    {
        inventoryData.AddItem(itemToAdd.item, itemToAdd.quantity);
        OnPlayerInventoryChange?.Invoke();
    }

    public void RemoveItemFromInventory(QuantifiedItem itemToRemove)
    {
        inventoryData.RemoveItem(itemToRemove.item, itemToRemove.quantity);
        if (itemToRemove.item.Equals(EquippedItemData.item) && itemToRemove.quantity >= EquippedItemData.quantity)
        {
            Debug.Log("Emptying equipped item");
            EquippedItemData = InventoryItem.GetEmptyItem();
        }
        OnPlayerInventoryChange?.Invoke();
    }

    public void AddItemsToInventory(List<QuantifiedItem> itemsToAdd)
    {
        foreach (QuantifiedItem quantifiedItem in itemsToAdd)
        {
            AddItemToInventory(quantifiedItem);
        }
        OnPlayerInventoryChange?.Invoke();
    }

    public void RemoveItemsFromInventory(List<QuantifiedItem> itemsToRemove)
    {
        foreach (QuantifiedItem quantifiedItem in itemsToRemove)
        {
            RemoveItemFromInventory(quantifiedItem);
        }
        OnPlayerInventoryChange?.Invoke();
    }

    public bool CheckForItem(QuantifiedItem item)
    {
        return inventoryData.CheckForItem(item.item, item.quantity);
    }

    public int GetItemQuantity(Item item)
    {
        return inventoryData.GetItemQuantity(item);
    }

    protected override void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        base.UpdateInventoryUI(inventoryState);
        
    }
}

[Serializable]
public struct QuantifiedItem
{
    public Item item;
    public int quantity;

    public QuantifiedItem(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}
