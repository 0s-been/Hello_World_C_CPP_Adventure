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
        /// <summary>좌클릭된 셀 (좌표 배치 시도용)</summary>
        IObservable<(int row, int col)> OnCellLeftClicked { get; }

        /// <summary>우클릭된 셀 좌표 (장착 시도용)</summary>
        IObservable<(int row, int col)> OnCellRightClicked { get; }

        /// <summary>드래그를 시작한 셀 좌표 (그 칸의 아이템을 잡음)</summary>
        IObservable<(int row, int col)> OnDragBegan { get; }

        /// <summary>
        /// 드래그를 놓은 결과. 시작 칸과 놓은 위치를 전달
        /// dropResult -> 어디에 놓았는지 (인벤토리 칸 / 인벤토리 밖)
        /// </summary>
        IObservable<DragDropInfo> OnDragDropped { get; }

        /// <summary>그리드 크기를 받아 빈 셀 UI를 생성</summary>
        void BuildGrid(int rows, int cols);

        /// <summary>배치 목록을 받아 그리드를 다시 그림</summary>
        void Render(IReadOnlyList<PlacedItem> placedItems);
    }

    /// <summary>
    /// 드래그 드롭 한 번의 정보
    /// fromRow, fromCol -> 드래그를 시작한 칸
    /// toRow, toCol-> 놓은 인벤토리 칸 
    /// insideInventory -> 인벤토리 격자 안에 놓았는지 (false면 밖 -> 삭제 후보)
    /// </summary>
    public struct DragDropInfo
    {
        public int fromRow;
        public int fromCol;
        public int toRow;
        public int toCol;
        public bool insideInventory;
    }
}
