using System;
using System.Collections.Generic;
using InventorySDK.Domain;

namespace OnionArc.Model
{
    /// <summary>
    /// 인벤토리 시스템 관련 비즈니스 로직을 수행할 model의 interface
    /// </summary>
    public interface IInventoryModel
    {
        int Rows { get; }
        int Cols { get; }
        /// <summary> 인벤토리에 배치된 아이템들의 정보</summary>
        IReadOnlyList<PlacedItem> PlacedItems { get; }
        /// <summary> 배치 정보가 변경될 시 PlacedItems를 보냄</summary>
        IObservable<IReadOnlyList<PlacedItem>> OnInventoryChanged { get; }

        /// <summary> 아이템 정보, 배치할 위치를 통해 배치 가능여부 판별</summary>
        bool TryPlace(ItemInfo item, int row, int col);

        /// <summary> 해당 위치의 아이템을 인벤 내에서 제거</summary>
        bool RemoveAt(int row, int col);
        /// <summary> 해당 위치의 아이템을 인벤 내에서 get</summary>
        PlacedItem GetItemAt(int row, int col);
    }
}
