using System.Collections.Generic;
using UniRx;
using System;

namespace InventorySDK.Domain
{
    /// <summary>
    /// 인벤토리가 할 수 있는 단순 기능들만 정의
    /// application은 이 인터페이스에만 의존
    /// </summary>
    public interface IInventoryDomain
    {
        int Rows { get; }
        int Cols { get; }

        /// <summary>현재 배치된 모든 아이템(읽기 전용). UI가 다시 그릴 때 순회용</summary>
        IReadOnlyList<PlacedItem> PlacedItems { get; }

        /// <summary>배치 목록이 바뀔 때마다 최신 목록을 흘려보냄.</summary>
        IObservable<IReadOnlyList<PlacedItem>> OnInventoryChanged { get; }

        /// <summary>(row,col)에 item을 놓을 수 있는지 판별만 한다(상태 변경 x)</summary>
        bool CanPlace(ItemInfo item, int row, int col);

        /// <summary>배치 시.</summary>
        bool TryPlace(ItemInfo item, int row, int col);

        /// <summary>칸에 배치된 아이템을 제거</summary>
        bool RemoveAt(int row, int col);

        /// <summary>
        /// (frow,fcol)의 아이템을 (torow,tocol)으로 이동.
        /// 자기 자신을 먼저 떼고 검사하므로, 이동 전후가 겹쳐도 동작함
        /// 놓을 수 없으면 원위치로 복구
        /// </summary>
        bool TryMove(int frow, int fcol, int torow, int tocol);

        /// <summary>칸을 점유한 아이템을 돌려줌 없으면 null</summary>
        PlacedItem GetItemAt(int row, int col);
    }
}
