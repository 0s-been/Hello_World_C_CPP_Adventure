using System.Collections.Generic;
using UniRx;
using System;

namespace InventorySDK.Domain
{
    /// <summary>
    /// IInventoryDomain의 구현체.
    ///
    /// 상태를 2곳에 든다:
    ///   _grid        : PlacedItem[,]  → "어느 칸이 찼나"를 판별하기 위함
    ///   _placedItems : List          → "배치된 것들"을 순회/표시하기 위함
    /// 이 둘의 동기화는 반드시 이 클래스 안에서만 일어난다(외부는 배열을 못 만짐).
    /// → 상태가 두 곳에 흩어져 생기는 버그를 한 곳에 가둔다 (캡슐화).
    /// </summary>
    public class InventoryDomain : IInventoryDomain
    {
        private readonly PlacedItem[,] _grid;
        private readonly List<PlacedItem> _placedItems = new List<PlacedItem>();

        // ReactiveProperty 대신 Subject를 쓴 이유: "값"이 아니라 "변경 이벤트"를
        // 흘려보내는 용도라서. 구독자에게 최신 목록을 통지만 하면 된다.
        private readonly Subject<IReadOnlyList<PlacedItem>> _onChanged
            = new Subject<IReadOnlyList<PlacedItem>>();

        public int Rows { get; }
        public int Cols { get; }
        public IReadOnlyList<PlacedItem> PlacedItems => _placedItems;
        public IObservable<IReadOnlyList<PlacedItem>> OnInventoryChanged => _onChanged;

        public InventoryDomain(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            _grid = new PlacedItem[rows, cols];
        }

        public bool CanPlace(ItemInfo item, int row, int col)
        {
            if (item == null) return false;

            // 임시 PlacedItem을 만들어 점유 칸을 물어본다 (계산 책임은 PlacedItem에 있음)
            var candidate = new PlacedItem(item, row, col);
            foreach (var (r, c) in candidate.GetOccupiedCells())
            {
                // 그리드 경계를 벗어나면 불가
                if (r < 0 || r >= Rows || c < 0 || c >= Cols) return false;
                // 이미 다른 아이템이 차지한 칸이면 불가
                if (_grid[r, c] != null) return false;
            }
            return true;
        }

        public bool TryPlace(ItemInfo item, int row, int col)
        {
            if (!CanPlace(item, row, col)) return false;

            var placed = new PlacedItem(item, row, col);
            foreach (var (r, c) in placed.GetOccupiedCells())
                _grid[r, c] = placed;        // 배열 갱신

            _placedItems.Add(placed);        // 목록 갱신 
            _onChanged.OnNext(_placedItems); // 변경 통지
            return true;
        }

        public bool RemoveAt(int row, int col)
        {
            var target = GetItemAt(row, col);
            if (target == null) return false;

            foreach (var (r, c) in target.GetOccupiedCells())
                _grid[r, c] = null;          // 배열에서 비움

            _placedItems.Remove(target);     // 목록에서 제거
            _onChanged.OnNext(_placedItems); // 변경 통지
            return true;
        }

        public PlacedItem GetItemAt(int row, int col)
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Cols) return null;
            return _grid[row, col];
        }
    }
}
