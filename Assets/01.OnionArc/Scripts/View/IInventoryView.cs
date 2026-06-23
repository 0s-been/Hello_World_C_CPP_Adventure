using System.Collections.Generic;
using UniRx;
using System;
using InventorySDK.Domain;

namespace OnionArc.View
{

    /// <summary>
    /// 인벤토리 ui의 interface
    /// </summary>
    public interface IInventoryView
    {
        /// <summary>좌클릭된 셀 좌표. 배치 시도용</summary>
        IObservable<(int row, int col)> OnCellLeftClicked { get; }

        /// <summary>우클릭된 셀 좌표. 제거 시도용</summary>
        IObservable<(int row, int col)> OnCellRightClicked { get; }

        /// <summary>그리드 크기를 받아 빈 셀 UI를 생성.</summary>
        void BuildGrid(int rows, int cols);

        /// <summary>배치 목록을 받아 그리드를 다시 그림.</summary>
        void Render(IReadOnlyList<PlacedItem> placedItems);
    }
}
