using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{

    public GameObject inventoryItemPrefab;
    public Transform itemParent;

    public TextMeshProUGUI Description;
    public Image ItemImage;
    public GameObject InventoryEmpty;

    public Sprite[] sprites;

    public enum ItemIconType
    {
        Coin1, Coin2, Coin3, Coin4, Coin5, Coin6, Coin7, Coin8,
        PadlockKey, ShrineKey, SewerKey, TowerKey, ServantKey,
        Axe,
        StatueHead,
        Heirloom, TrueHeirloom,
        Mallet,
        Unknown
    }

    public class Item
    {
        public ItemIconType Icon;
        public string Name;
        public string Description;
        public Sprite Image;

        public Item(ItemIconType icon, string name, string description)
        {
            Icon = icon;
            Name = name;
            Description = description;
        }
    }

    public static Item[] itemList = {new Item(ItemIconType.PadlockKey, "padlock key", "A key I found by the large gates in the factory district of Kisaragi."),
        new Item(ItemIconType.Axe, "axe", "A brittle looking axe... It looks like it is about to fall apart but probably could chop some wood. Something about carrying it makes me feel uneasy."),
        new Item (ItemIconType.ServantKey, "Servants' Quarters Key", "A key I found in the kitchen of the manor."),
        new Item (ItemIconType.TowerKey, "Meio's Tower Key", "I found this key in the Servants' Quarters being fervently protected by that monster. It must be important."),
        new Item (ItemIconType.StatueHead, "missing statue head", "I can barely move with this. I need to take my time and make absolutely sure I'm safe when I'm ready to move."),
        new Item (ItemIconType.SewerKey, "sewer key", "It appears Noriko left this for me... I must be getting close to her."),
        new Item (ItemIconType.ShrineKey, "shrine key", "I just need to use this key to get through the shrine and get the hell out of here."),
        new Item (ItemIconType.Coin1, "Coin #1", "This coin is strange - there are six symbols that look close to kanji, but they seem off. Maybe it's really old? It feels like part of a set, somehow."),
        new Item (ItemIconType.Coin2, "Coin #2", "This coin looks like it has the symbol for \"well\" on it, but you realize it's not quite right. Maybe it's ancient?"),
        new Item (ItemIconType.Coin3, "Coin #3", "This coin is really, really old and you can't even make out the markings. What is it doing here?"),
        new Item (ItemIconType.Coin4, "Coin #4", "This symbol looks totally weird. Possibly a kamon - one of those old family crests? But you've never seen anything like it."),
        new Item (ItemIconType.Coin5, "Coin #5", "This coin doesn't even seem correctly pressed. It looks like it has some kind of family crest on it but none that you can recall ever seeing."),
        new Item (ItemIconType.Coin6, "Coin #6", "This coin has strange symbols and things that look like writing but all seem slightly off. You can't really make out what's embossed on it."),
        new Item (ItemIconType.Coin7, "Coin #7", "This coin nags at you - parts of it look familiar. It reminds you of the sloppy way Aunt Noriko writes, strangely. Are there more like it?"),
        new Item (ItemIconType.Coin8, "Coin #8", "This coin bothers you because the symbol in the center is not Japanese but you feel like you have definitely seen it somewhere."),
        new Item (ItemIconType.Heirloom, "Family Heirloom", "So, this is what Noriko wanted me to come to this horrible place for? Supposedly, this can end the curse. I had never seen it before, but Noriko often mentioned it."),
        new Item (ItemIconType.TrueHeirloom, "True Family Heirloom", "The other heirloom was a fraud. The real Noriko didn't want me to come here and be in danger, but now that I have it is time for me to finish everything with this heirloom."),
        new Item(ItemIconType.Mallet, "gong mallet", "I can use this to ring the gong.") };


    public void GenerateInventory()
    {
        ItemImage.gameObject.SetActive(false);
        if (GameManager.instance.Player.Inventory.Count == 0)
        {
            InventoryEmpty.SetActive(true);
        }
        else
        {
            foreach (Transform t in itemParent.GetComponentsInChildren<Transform>())
            {
                if (t != itemParent && t.gameObject != InventoryEmpty) Destroy(t.gameObject);
            }

            InventoryEmpty.SetActive(false);
            foreach (string item in GameManager.instance.Player.Inventory)
            {
                Item itemObj = GetItemFromList(item);
                GameObject go = Instantiate(inventoryItemPrefab);
                go.GetComponentInChildren<TextMeshProUGUI>().text = item;
                go.transform.SetParent(itemParent);
                go.GetComponent<InventoryItemSelectable>().SetItem(itemObj);
                go.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        if (GameManager.instance.Player.Inventory.Count == 0) DisplayImageAndDescription("", 18);
        else
        {
            Item itemObj = GetItemFromList(GameManager.instance.Player.Inventory[0]);
            DisplayImageAndDescription(itemObj.Description, (int)itemObj.Icon);
        }
    }

    public Item GetItemFromList(string name)
    {
        foreach (Item i in itemList)
        {
            if (name == i.Name) return i;
        }
        Debug.LogWarning("Inventory contains an item not in item list: " + name);
        return new Item(ItemIconType.Unknown, name, "A strange, mysterious object. I have no idea what it is, and neither does the programmer.");
    }

    public void DisplayImageAndDescription(string text, int imageRef)
    {
        Description.text = text;
        if (imageRef != (int)ItemIconType.Unknown)
        {
            ItemImage.gameObject.SetActive(true);
            ItemImage.sprite = sprites[imageRef];
        }
    }
}
