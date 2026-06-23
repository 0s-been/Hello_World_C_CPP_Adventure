using System.Collections.Generic;
using UniRx;
using System;

namespace InventorySDK.Domain
{
    /// <summary>
    /// 인벤토리가 "무엇을 할 수 있는가"만 정의
    /// application은 이 인터페이스에만 의존
    /// </summary>
    public interface IInventoryDomain
    {
        int Rows { get; }
        int Cols { get; }

        /// <summary>현재 배치된 모든 아이템(읽기 전용). UI가 다시 그릴 때 순회용.</summary>
        IReadOnlyList<PlacedItem> PlacedItems { get; }

        /// <summary>배치 목록이 바뀔 때마다 최신 목록을 흘려보낸다.</summary>
        IObservable<IReadOnlyList<PlacedItem>> OnInventoryChanged { get; }

        /// <summary>(row,col)에 item을 놓을 수 있는지 판별만 한다(상태 변경 X).</summary>
        bool CanPlace(ItemInfo item, int row, int col);

        /// <summary>배치 시도. 성공하면 true + 변경 통지, 실패하면 false.</summary>
        bool TryPlace(ItemInfo item, int row, int col);

        /// <summary>(row,col) 칸에 걸친 아이템을 제거. 성공 시 변경 통지.</summary>
        bool RemoveAt(int row, int col);

        /// <summary>(row,col) 칸을 점유한 아이템을 돌려준다. 없으면 null.</summary>
        PlacedItem GetItemAt(int row, int col);
    }
}
