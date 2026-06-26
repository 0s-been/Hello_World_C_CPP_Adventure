using System;
using System.Collections.Generic;
using InventorySDK.Domain;
using System.Data;
using Unity.VisualScripting;

namespace InventorySDK.Application
{

    /// <summary>
    /// 인벤토리 유스케이스에 대한 interface
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
        bool TryMove(int frow, int fcol, int trow, int tcol);
            
    }
}
