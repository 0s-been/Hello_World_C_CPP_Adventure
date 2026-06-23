using System;
using System.Collections.Generic;

namespace InventorySDK.Application
{
    using InventorySDK.Domain;

    /// <summary>
    /// 인벤토리 유스케이스의 계약.
    /// </summary>
    public interface IInventoryApplication
    {
        int Rows { get; }
        int Cols { get; }
        IReadOnlyList<PlacedItem> PlacedItems { get; }
        IObservable<IReadOnlyList<PlacedItem>> OnInventoryChanged { get; }

        bool CanPlace(ItemInfo item, int row, int col);
        bool TryPlace(ItemInfo item, int row, int col);
        bool RemoveAt(int row, int col);
        PlacedItem GetItemAt(int row, int col);
    }
}
