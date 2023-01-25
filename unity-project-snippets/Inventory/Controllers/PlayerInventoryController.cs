using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : InventoryController
{
    protected const int PLAYER_INVENTORY_SIZE = 32;

    public static PlayerInventoryController Instance { get; private set; }

    public InventoryItem EquippedItemData { get; set; }
    public Item EquippedItem => EquippedItemData.item;

    private PlayerInventoryUI PlayerInventoryUI => InventoryUI as PlayerInventoryUI;

    private void Awake()
    {
        Instance = this;
    }

    protected override void PrepareUI()
    {
        base.PrepareUI();
        EquippedItemData = InventoryItem.GetEmptyItem();
        PlayerInventoryUI.OnDescriptionRequested += HandleEquipItem;
    }

    private void HandleDescriptionRequest(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;
        Item item = inventoryItem.item;
        PlayerInventoryUI.UpdateDescription(itemIndex, item.GameSprite, item.ItemName, item.Description);
    }

    private void HandleEquipItem(int index)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(index);
        if (inventoryItem.IsEmpty)
            EquippedItemData = InventoryItem.GetEmptyItem();
        else
            EquippedItemData = inventoryItem;
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
    }

    public void RemoveItemFromInventory(QuantifiedItem itemToRemove)
    {
        inventoryData.RemoveItem(itemToRemove.item, itemToRemove.quantity);
    }

    public void AddItemsToInventory(List<QuantifiedItem> itemsToAdd)
    {
        foreach (QuantifiedItem quantifiedItem in itemsToAdd)
        {
            AddItemToInventory(quantifiedItem);
        }
    }

    public void RemoveItemsFromInventory(List<QuantifiedItem> itemsToRemove)
    {
        foreach (QuantifiedItem quantifiedItem in itemsToRemove)
        {
            RemoveItemFromInventory(quantifiedItem);
        }
    }
}

[Serializable]
public struct QuantifiedItem
{
    public Item item;
    public int quantity;
}
