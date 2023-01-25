using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryUI : InventoryUI
{
    public static PlayerInventoryUI Instance { get; private set; }

    private Item equippedItem;
    public Item EquippedItem => equippedItem;


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

    public override void Show()
    {
        base.Show();
        inventoryTab.tabGroup.OnTabSelected(inventoryTab);
        inventoryDescription.ResetDescription();
    }

    public void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
    {
        inventoryDescription.SetDescription(itemImage, name, description);
        DeselectAllItems();
        ItemUIs[itemIndex].Select();
    }
}
