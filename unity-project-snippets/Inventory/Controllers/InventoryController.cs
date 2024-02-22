using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [field: SerializeField]
    public InventoryUI InventoryUI { get; set; }
    [SerializeField]
    protected Inventory inventoryData;

    public List<InventoryItem> initialItems = new List<InventoryItem>();

    // Start is called before the first frame update
    void Start()
    {
        PrepareUI();
        PrepareData();
    }

    protected virtual void PrepareData()
    {
        inventoryData.Init();
        inventoryData.OnInventoryUpdated += UpdateInventoryUI;
        foreach (InventoryItem item in initialItems)
        {
            if (item.IsEmpty)
                continue;
            inventoryData.AddItem(item);
        }
    }

    protected virtual void PrepareUI()
    {
        InventoryUI.InitInventoryUI(inventoryData.Size);
        
        InventoryUI.OnSwapItems += HandleSwapItems;
        InventoryUI.OnStartDragging += HandleDragging;
        InventoryUI.OnItemAction += HandleItemActionRequest;
    }    

    protected void HandleDragging(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;
        InventoryUI.SetDragHandler(inventoryItem.item.GameSprite);
    }
    protected void HandleSwapItems(int itemIndex1, int itemIndex2)
    {
        inventoryData.SwapItems(itemIndex1, itemIndex2);
    }
    protected virtual void HandleItemActionRequest(int itemIndex)
    {
        
    }

    public virtual void ForceUpdateInventoryUI()
    {
        foreach (var item in inventoryData.GetCurrentInventoryState())
        {
            InventoryUI.UpdateData(item.Key, item.Value.item != null ? item.Value.item.GameSprite : null, item.Value.quantity);
        }
    }

    protected virtual void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        InventoryUI.ResetAllItems();
        foreach (var item in inventoryState)
        {
            InventoryUI.UpdateData(item.Key, item.Value.item != null ? item.Value.item.GameSprite : null, item.Value.quantity);
        }
    }
}
