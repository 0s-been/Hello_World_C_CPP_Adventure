using System;
using System.Collections.Generic;
using InventorySDK.Domain;

namespace InventorySDK.Application
{

    /// <summary>
    /// applicaion 계층 구현
    /// 지금은 단일 domain이라 대부분 위임인 상태
    /// </summary>
    // 그럼에도 applicaion을 둔 이유 -> 나중에 여러 domain을 조합하거나 
    // 추가 규칙을 넣을 때 확정성을 위해서
    public class InventoryApplication : IInventoryApplication
    {
        private readonly IInventoryDomain _domain;

        // zenject가 채워줌
        public InventoryApplication(IInventoryDomain domain)
        {
            _domain = domain;
        }

        public int Rows => _domain.Rows;
        public int Cols => _domain.Cols;
        public IReadOnlyList<PlacedItem> PlacedItems => _domain.PlacedItems;
        public IObservable<IReadOnlyList<PlacedItem>> OnInventoryChanged => _domain.OnInventoryChanged;

        public bool CanPlace(ItemInfo item, int row, int col) => _domain.CanPlace(item, row, col);
        public bool TryPlace(ItemInfo item, int row, int col) => _domain.TryPlace(item, row, col);
        public bool RemoveAt(int row, int col) => _domain.RemoveAt(row, col);
        public PlacedItem GetItemAt(int row, int col) => _domain.GetItemAt(row, col);
        public bool TryMove(int frow, int fcol, int trow, int tcol)
            => _domain.TryMove(frow, fcol, trow, tcol);
    }
}
