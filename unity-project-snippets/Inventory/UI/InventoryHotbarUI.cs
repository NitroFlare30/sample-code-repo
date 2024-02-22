using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryHotbarUI : MonoBehaviour
{
    public static InventoryHotbarUI Instance { get; private set; }

    [SerializeField]
    private List<InventoryItemUI> inventoryItems = new(HOTBAR_SIZE);

    private const int HOTBAR_SIZE = 8;
    private int currentlyEquippedIndex = 0;

    public void InitHotbarUI()
    {
        Instance = this;
        for (int i = 0; i < 8; i++)
        {
            InventoryItemUI item = transform.GetChild(i).GetComponent<InventoryItemUI>();
            inventoryItems.Add(item);
            item.OnItemClicked += HotbarItemClicked;
        }
        DeselectAllItems();
        inventoryItems[0].Select();
    }

    public void UpdateData(int itemIndex, Sprite itemSprite, int itemQuantity)
    {
        if (itemIndex < HOTBAR_SIZE)
        {
            if (itemQuantity < 1)
                inventoryItems[itemIndex].ResetData();
            else
                inventoryItems[itemIndex].SetData(itemSprite, itemQuantity);
        }
        else
        {
            Debug.LogError("Trying to assign hotbar out of bounds: " + itemIndex);
        }
    }

    private void HotbarItemClicked(InventoryItemUI itemUI)
    {
        int index = inventoryItems.IndexOf(itemUI);
        if (index == -1)
            return;
        //Equip item
        PlayerInventoryController.Instance.SetEquippedItem(index);
        currentlyEquippedIndex = index;
        DeselectAllItems();
        itemUI.Select();
    }

    public void SelectHotbarItem(int index)
    {
        index--;
        if (index >= 0 && index < 8)
        {
            PlayerInventoryController.Instance.SetEquippedItem(index);
            currentlyEquippedIndex = index;
            DeselectAllItems();
            inventoryItems[index].Select();
        }
    }

    public void IncrementHotbarSelection()
    {
        currentlyEquippedIndex++;
        if (currentlyEquippedIndex >= 8)
            currentlyEquippedIndex = 0;
        PlayerInventoryController.Instance.SetEquippedItem(currentlyEquippedIndex);
        DeselectAllItems();
        inventoryItems[currentlyEquippedIndex].Select();
    }

    public void DecrementHotbarSelection()
    {
        currentlyEquippedIndex--;
        if (currentlyEquippedIndex < 0)
            currentlyEquippedIndex = 7;
        PlayerInventoryController.Instance.SetEquippedItem(currentlyEquippedIndex);
        DeselectAllItems();
        inventoryItems[currentlyEquippedIndex].Select();
    }

    private void DeselectAllItems()
    {
        foreach (InventoryItemUI itemUI in inventoryItems)
            itemUI.Deselect();
    }

}
