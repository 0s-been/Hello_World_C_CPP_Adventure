using System;
using System.Collections.Generic;
using UniRx;
using InventorySDK.Domain;

namespace EquipmentSDK.Application
{

    /// <summary>장착 application의 interface </summary>
    public interface IEquipmentApplication
    {
        (int rows, int cols) GetSlotSize(EquipPart part);
        IReadOnlyList<PlacedItem> GetPlaced(EquipPart part);
        IObservable<EquipPart> OnEquipmentChanged { get; }

        bool TryEquip(ItemInfo item, out ItemInfo displaced);
        ItemInfo Unequip(EquipPart part, int row, int col);
        PlacedItem GetItemAt(EquipPart part, int row, int col);
    }
}
