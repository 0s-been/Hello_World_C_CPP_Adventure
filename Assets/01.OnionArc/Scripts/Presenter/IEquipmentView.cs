using System;
using System.Collections.Generic;
using UniRx;
using InventorySDK.Domain;

namespace OnionArc.View
{

    /// <summary>
    /// 장착 view interface (그리드 방식) 
    /// 부위별 그리드를 그리고
    /// 슬롯 셀 클릭(해제 요청)을 (부위, 행, 열)로 흘려보낸다.
    /// </summary>
    public interface IEquipmentView
    {
        /// <summary>슬롯 셀이 클릭됨(해제 시도용). 어느 부위의 (row,col)인지 전달.</summary>
        IObservable<(EquipPart part, int row, int col)> OnSlotClicked { get; }

        /// <summary>각 부위 그리드의 빈 셀들을 생성한다. size는 (부위→(rows,cols)).</summary>
        void BuildGrids(IReadOnlyDictionary<EquipPart, (int rows, int cols)> sizes);

        /// <summary>특정 부위의 배치 상태를 받아 그 부위 그리드만 다시 그린다.</summary>
        void RenderPart(EquipPart part, IReadOnlyList<PlacedItem> placed);
    }
}
