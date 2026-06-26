using System.Collections.Generic;
using UniRx;
using System;

namespace InventorySDK.Domain
{
    /// <summary>
    /// IInventoryDomain의 구현체
    /// 구현체임에도 infrastructure가 아닌 domain에 둔 이유는 다른 계층과의 상태에 대한 정보를 공유하거나
    /// DI가 일어나지 않기 때문에 domain계층에 뒀음
    ///   _grid        : PlacedItem[,]  -> "어느 칸이 찼나"를 판별하기 위함
    ///   _placedItems : List           -> "배치된 것들"을 순회/표시하기 위함
    /// 이 둘의 동기화는 반드시 이 클래스 안에서만 일어나야 함
    /// 상태가 두 곳에 흩어져 생기는 버그를 한 곳에 가둬서 관리를 용이하게 함
    /// </summary>
    public class InventoryDomain : IInventoryDomain
    {
        /// <summary> 어느 칸이 찼나지를 판별</summary>
        private readonly PlacedItem[,] _grid;
        /// <summary> 배치된 것들을 순회 및 표시하기 위한 용도</summary>
        private readonly List<PlacedItem> _placedItems = new List<PlacedItem>();

        // ReactiveProperty 대신 Subject를 쓴 이유
        // "값"이 아니라 "변경 이벤트"를
        // 흘려보내는 용도이기 때문 구독자에게 최신 목록을 통지만 하면 됨
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

            // 임시 PlacedItem을 만들어 점유 칸을 물어봄 계산 책임은  PlacedItem에
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
            {
                // 배열 갱신
                _grid[r, c] = placed;
            }

            // 목록 갱신 
            _placedItems.Add(placed);
            // 변경 통지
            _onChanged.OnNext(_placedItems);
            return true;
        }

        public bool RemoveAt(int row, int col)
        {
            var target = GetItemAt(row, col);
            if (target == null) return false;

            foreach (var (r, c) in target.GetOccupiedCells())
            {
                // 배열에서 비움
                _grid[r, c] = null;
            }
            // 목록에서 제거
            _placedItems.Remove(target);
            // 변경 통지
            _onChanged.OnNext(_placedItems);
            return true;
        }

        public bool TryMove(int frow, int fcol, int torow, int tocol)
        {
            // 시작 칸의 아이템 찾기
            var target = GetItemAt(frow, fcol);
            if (target == null) return false;

            ItemInfo item = target.Item;

            // 자기 자신의 배치에 대한 그리드 이동할 경우 겹칠 수 있어서 먼저 떼어냄
            // 통지 없이 내부 상태만 비움 중간 상태를 외부에 노출하지 않기 위함
            foreach (var (r, c) in target.GetOccupiedCells())
            {
                _grid[r, c] = null;
            }
            _placedItems.Remove(target);

            // 뗀 상태에서 놓을 수 있는지 검사
            if (CanPlace(item, torow, tocol))
            {
                // 가능 -> 새 위치에 배치
                var moved = new PlacedItem(item, torow, tocol);
                foreach (var (r, c) in moved.GetOccupiedCells())
                {
                    _grid[r, c] = moved;
                }
                _placedItems.Add(moved);
                _onChanged.OnNext(_placedItems); 
                return true;
            }
            else
            {
                // 불가 -> 원위치로 복구 
                foreach (var (r, c) in target.GetOccupiedCells())
                    _grid[r, c] = target;
                _placedItems.Add(target);
                // 복구는 상태 변화가 없는 셈이므로 통지하지 않아도 되지만,
                // 안전하게 현재 목록을 다시 통지 구독하는 측에 동기화 보장
                _onChanged.OnNext(_placedItems);
                return false;
            }
        }

        public PlacedItem GetItemAt(int row, int col)
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Cols) return null;
            return _grid[row, col];
        }
    }
}
