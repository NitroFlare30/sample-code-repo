using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Inventory : ScriptableObject
{
    [SerializeField]
    private List<InventoryItem> inventoryItems;

    [field: SerializeField]
    public int Size { get; private set; } = 32;

    public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

    public void Init()
    {
        inventoryItems = new List<InventoryItem>();
        for (int i = 0; i < Size; i++)
        {
            inventoryItems.Add(InventoryItem.GetEmptyItem());
        }
    }

    public void AddItem(InventoryItem item) => AddItem(item.item, item.quantity);
    public void AddItem(InventoryItem item, int quantity) => AddItem(item.item, quantity);
    public void AddItem(Item item, int quantity)
    {
        int existingItemIndex = GetItemIndex(item);
        Debug.Log($"Adding {item.ItemName} to inventory add index {existingItemIndex}");
        if (existingItemIndex != -1)
        {
            inventoryItems[existingItemIndex] = inventoryItems[existingItemIndex].AddQuantity(quantity);
            InformControllerAboutChange();
            return;
        }

        // Item does not preexist
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty)
            {
                inventoryItems[i] = new InventoryItem { item = item, quantity = quantity };
                InformControllerAboutChange();
                return;
            }
        }
    }

    public void AddItemAtIndex(InventoryItem item, int itemIndex)
    {
        // WARNING: OVERWRITES CURRENT ITEM AT INDEX
        inventoryItems[itemIndex] = new InventoryItem { item = item.item, quantity = item.quantity };
        InformControllerAboutChange();
    }

    public void RemoveItem(Item item, int quantity = 1)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].item == item && inventoryItems[i].quantity >= quantity)
            {
                RemoveItem(i, quantity);
            }
        }
    }

    public void RemoveItem(int itemIndex, int quantity = 1)
    {
        if (inventoryItems.Count > itemIndex)
        {
            if (inventoryItems[itemIndex].IsEmpty)
                return;
            int remainder = inventoryItems[itemIndex].quantity - quantity;
            if (remainder <= 0)
            {
                inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
                Debug.Log("Empty slot: " + itemIndex);
            }
            else if (remainder > 0)
                inventoryItems[itemIndex] = inventoryItems[itemIndex].SetQuantity(remainder);
            else
                Debug.LogError("Inventory: Trying to remove more item than available");
        }
        InformControllerAboutChange();
    }

    public void RemoveItemStack(int itemIndex)
    {
        inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
        InformControllerAboutChange();
    }

    public InventoryItem GetItemAt(int itemIndex)
    {
        return inventoryItems[itemIndex];
    }

    public Dictionary<int, InventoryItem> GetCurrentInventoryState()
    {
        Dictionary<int, InventoryItem> retVal = new Dictionary<int, InventoryItem>();
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            //if (inventoryItems[i].IsEmpty)
            //    continue;
            retVal[i] = inventoryItems[i];
        }
        return retVal;
    }

    public void SwapItems(int itemIndex1, int itemIndex2)
    {
        if (itemIndex1 < 0 || itemIndex2 < 0)
            return;
        if (inventoryItems[itemIndex1].IsEmpty && inventoryItems[itemIndex2].IsEmpty)
            return;
        (inventoryItems[itemIndex2], inventoryItems[itemIndex1]) = (inventoryItems[itemIndex1], inventoryItems[itemIndex2]);
        InformControllerAboutChange();
    }

    // Breaks if total quantity is split between multiple slots
    public bool CheckForItem(Item item, int quantity = 1)
    {
        int totalCount = 0;
        foreach (InventoryItem invItem in GetCurrentInventoryState().Values)
        {
            if (invItem.item == item)
            {
                totalCount += invItem.quantity;
            }
        }
        return totalCount >= quantity;
    } 

    public int GetItemQuantity(Item item)
    {
        int numItem = 0;
        foreach (InventoryItem invItem in GetCurrentInventoryState().Values)
        {
            if (invItem.item == item)
                numItem += invItem.quantity;
        }
        return numItem;
    }

    /// <returns>Index of item in inventory, -1 if item DNE</returns>
    private int GetItemIndex(Item item)
    {
        var currentInv = GetCurrentInventoryState();
        foreach (KeyValuePair<int, InventoryItem> invItem in currentInv)
        {
            if (invItem.Value.item == item)
            {
                return invItem.Key;
            }
        }
        return -1;
    }

    public void ClearInventoryData() => inventoryItems.Clear();

    private void InformControllerAboutChange()
    {
        OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
    }
}

[Serializable]
public struct InventoryItem
{
    public Item item;
    public int quantity;
    public bool IsEmpty => item == null;

    public InventoryItem SetQuantity(int newQuantity)
    {
        return new InventoryItem
        {
            item = this.item,
            quantity = newQuantity
        };
    }

    public InventoryItem AddQuantity(int quantityToAdd)
    {
        return new InventoryItem
        {
            item = this.item,
            quantity = quantity + quantityToAdd
        };
    }

    public static InventoryItem GetEmptyItem() => new InventoryItem
    {
        item = null, 
        quantity = 0
    };
}
