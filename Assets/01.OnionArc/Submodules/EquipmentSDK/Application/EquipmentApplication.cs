using System;
using System.Collections.Generic;
using UniRx;
using InventorySDK.Domain;
using EquipmentSDK.Domain;

namespace EquipmentSDK.Application
{

    /// <summary>장착 유스케이스 구현</summary>
    public class EquipmentApplication : IEquipmentApplication
    {
        private readonly IEquipmentDomain _domain;

        public EquipmentApplication(IEquipmentDomain domain)
        {
            _domain = domain;
        }

        public IObservable<EquipPart> OnEquipmentChanged => _domain.OnEquipmentChanged;

        public (int rows, int cols) GetSlotSize(EquipPart part) => _domain.GetSlotSize(part);
        public IReadOnlyList<PlacedItem> GetPlaced(EquipPart part) => _domain.GetPlaced(part);
        public bool TryEquip(ItemInfo item, out ItemInfo displaced) => _domain.TryEquip(item, out displaced);
        public ItemInfo Unequip(EquipPart part, int row, int col) => _domain.Unequip(part, row, col);
        public PlacedItem GetItemAt(EquipPart part, int row, int col) => _domain.GetItemAt(part, row, col);
    }
}
