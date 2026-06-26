using System;
using System.Collections.Generic;
using UniRx;
using InventorySDK.Domain;

namespace OnionArc.Model
{
    /// <summary>
    /// 장착 시스템 model의 interface
    /// </summary>
    public interface IEquipmentModel
    {
        (int rows, int cols) GetSlotSize(EquipPart part);
        IReadOnlyList<PlacedItem> GetPlaced(EquipPart part);
        IObservable<EquipPart> OnEquipmentChanged { get; }

        bool TryEquip(ItemInfo item, out ItemInfo displaced);
        ItemInfo Unequip(EquipPart part, int row, int col);
        PlacedItem GetItemAt(EquipPart part, int row, int col);
    }
}
