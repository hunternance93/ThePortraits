using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemSelectable: MonoBehaviour, ISelectHandler
{
    private InventoryController inventoryController;
    private InventoryController.Item item = null;

    private void Start()
    {
        inventoryController = GetComponentInParent<InventoryController>();
    }

    public void SetItem(InventoryController.Item item)
    {
        this.item = item;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (item == null)
        {
            Debug.Log("Inventory item doesn't contain any item");
            return;
        }
        
        inventoryController.DisplayImageAndDescription(item.Description, (int)item.Icon);
    }
}
