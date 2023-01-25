using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    protected InventoryItemUI itemPrefab;

    public RectTransform areaPanel;

    [SerializeField]
    protected ItemDragHandler inventoryDragHandler;

    protected List<InventoryItemUI> ItemUIs = new List<InventoryItemUI>();

    protected int draggedItemIndex = -1;

    public event Action<int> OnDescriptionRequested, OnItemAction, OnStartDragging;
    public event Action<int, int> OnSwapItems;

    protected virtual void Start()
    {
        Hide();
        // TODO: Subscribe self to UI Dictionary
    }

    public virtual void InitInventoryUI(int inventorySize)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            InventoryItemUI item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            item.transform.SetParent(areaPanel);
            ItemUIs.Add(item);
            item.OnItemClicked += HandleItemSelection;
            item.OnItemBeginDrag += HandleBeginDrag;
            item.OnItemDroppedOn += HandleSwap;
            item.OnItemEndDrag += HandleEndDrag;
        }
    }

    public virtual void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
    {
        if (ItemUIs.Count > itemIndex)
        {
            ItemUIs[itemIndex].SetData(itemImage, itemQuantity);
        }
    }

    protected virtual void HandleItemSelection(InventoryItemUI obj)
    {
        int index = ItemUIs.IndexOf(obj);
        if (index == -1)
            return;
        OnDescriptionRequested?.Invoke(index);
    }
    protected virtual void HandleBeginDrag(InventoryItemUI obj)
    {
        int index = ItemUIs.IndexOf(obj);
        if (index == -1)
            return;
        draggedItemIndex = index;
        OnStartDragging?.Invoke(index);
    }
    protected virtual void HandleSwap(InventoryItemUI obj)
    {
        int index = ItemUIs.IndexOf(obj);
        if (index == -1)
            return;
        OnSwapItems?.Invoke(draggedItemIndex, index);
    }

    public virtual void ResetAllItems()
    {
        foreach (var item in ItemUIs)
        {
            item.ResetData();
            item.Deselect();
        }
    }
    protected void HandleEndDrag(InventoryItemUI obj)
    {
        ResetDragHandler();
    } 

    public virtual void Show()
    {
        gameObject.SetActive(true);
        ResetSelection();
    }
    public virtual void Hide()
    {
        gameObject.SetActive(false);
        ResetDragHandler();
    }

    public void SetDragHandler(Sprite sprite)
    {
        inventoryDragHandler.Toggle(true);
        inventoryDragHandler.SetDragImage(sprite);
    }
    protected virtual void ResetDragHandler()
    {
        inventoryDragHandler.Toggle(false);
        draggedItemIndex = -1;
    }

    protected void ResetSelection()
    {
        DeselectAllItems();
    }

    protected void DeselectAllItems()
    {
        foreach (InventoryItemUI item in ItemUIs)
        {
            item.Deselect();
        }
    }

    public void RemoveItem(Item item, int quantity = -1)
    {

    }

    public void AddItem(Item item, int quantity = -1)
    {

    }

    public bool CheckForItem(Item item, int numReq = -1)
    {
        return true;
    }
}
