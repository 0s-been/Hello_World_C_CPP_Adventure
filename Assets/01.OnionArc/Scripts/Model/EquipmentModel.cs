using System;
using System.Collections.Generic;
using UniRx;
using InventorySDK.Domain;
using EquipmentSDK.Application;

namespace OnionArc.Model
{

    /// <summary>
    /// application을 주입받아 사용
    /// </summary>
    public class EquipmentModel : IEquipmentModel
    {
        private readonly IEquipmentApplication _application;

        public EquipmentModel(IEquipmentApplication application)
        {
            _application = application;
        }

        public IObservable<EquipPart> OnEquipmentChanged => _application.OnEquipmentChanged;

        public (int rows, int cols) GetSlotSize(EquipPart part) => _application.GetSlotSize(part);
        public IReadOnlyList<PlacedItem> GetPlaced(EquipPart part) => _application.GetPlaced(part);
        public bool TryEquip(ItemInfo item, out ItemInfo displaced) => _application.TryEquip(item, out displaced);
        public ItemInfo Unequip(EquipPart part, int row, int col) => _application.Unequip(part, row, col);
        public PlacedItem GetItemAt(EquipPart part, int row, int col) => _application.GetItemAt(part, row, col);
    }
}
