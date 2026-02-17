using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#pragma warning disable CS8603

namespace WaterGunLib.Modules
{
    public class ItemManagement
    {
        public static Item GetItemFromName(string itemName)
        {
            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.itemName == itemName)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
