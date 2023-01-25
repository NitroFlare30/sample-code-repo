using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketInventoryController : InventoryController
{

    private bool buyMode = true;
    private PlayerMoneyManager playerMoneyManager;

    [SerializeField]
    private Inventory playerInventoryData;
    
    private MarketInventoryUI MarketInventoryUI => InventoryUI as MarketInventoryUI;

    protected override void PrepareData()
    {
        playerInventoryData.OnInventoryUpdated += UpdateInventoryUI;
        base.PrepareData();
    }

    protected override void PrepareUI()
    {
        MarketInventoryUI.InitInventoryUI(inventoryData.Size);
        MarketInventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
        MarketInventoryUI.OnItemAction += HandleItemActionRequest;
        playerMoneyManager = PlayerMoneyManager.instance;
    }

    private void HandleDescriptionRequest(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;
        Item item = inventoryItem.item;
        MarketInventoryUI.UpdateDescription(itemIndex, item.GameSprite, item.ItemName, item.Description);
    }

    protected override void HandleItemActionRequest(int itemIndex)
    {
        if (buyMode)
        {
            Item itemToBuy = inventoryData.GetItemAt(itemIndex).item;
            if (itemToBuy != null)
            {
                int moneyRequired = itemToBuy.BuyingValue;
                if (playerMoneyManager.playerMoney >= moneyRequired)
                {
                    inventoryData.RemoveItem(itemIndex);
                    playerInventoryData.AddItem(itemToBuy, 1);
                    playerMoneyManager.playerMoney -= moneyRequired;
                }
            }
                
        }
        else
        {
            Item itemToSell = playerInventoryData.GetItemAt(itemIndex).item;
            if (itemToSell != null)
            {
                playerInventoryData.RemoveItem(itemIndex, 1);
                playerMoneyManager.playerMoney += itemToSell.SellingValue;
            }
        }
    }

    protected override void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        MarketInventoryUI.ResetAllItems();
        foreach (var item in inventoryData.GetCurrentInventoryState())
        {
            InventoryUI.UpdateData(item.Key, item.Value.item.GameSprite, item.Value.quantity);
        }
        foreach (var item in playerInventoryData.GetCurrentInventoryState())
        {
            MarketInventoryUI.UpdatePlayerInventoryData(item.Key, item.Value.item.GameSprite, item.Value.quantity);
        }
    }


}
