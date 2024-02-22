using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryUI : InventoryUI
{
    public static PlayerInventoryUI Instance { get; private set; }

    private Item equippedItem;
    public Item EquippedItem => equippedItem;

    public InventoryHotbarUI inventoryHotbarUI;
    [SerializeField]
    private TabElement inventoryTab;
    [SerializeField]
    private InventoryDescriptionUI inventoryDescription;

    private void Awake()
    {
        Instance = this;
    }

    protected override void Start()
    {
        base.Start();
        inventoryDescription.ResetDescription();

        
    }

    public override void InitInventoryUI(int inventorySize)
    {
        inventoryHotbarUI.InitHotbarUI();
        base.InitInventoryUI(inventorySize);
    }

    public override void Show()
    {
        base.Show();
        inventoryTab.tabGroup.OnTabSelected(inventoryTab);
        inventoryDescription.ResetDescription();
    }

    public void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
    {
        inventoryDescription.SetItemDescription(itemImage, name, description);
        DeselectAllItems();
        ItemUIs[itemIndex].Select();
    }

    public override void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
    {
        base.UpdateData(itemIndex, itemImage, itemQuantity);
        if (itemIndex < 8)
        {
            if (itemImage != null)
                inventoryHotbarUI.UpdateData(itemIndex, itemImage, itemQuantity);
            else
                inventoryHotbarUI.UpdateData(itemIndex, emptySlotSprite, itemQuantity);
        }
    }
}
