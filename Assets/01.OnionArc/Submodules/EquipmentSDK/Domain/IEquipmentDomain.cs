using System;
using System.Collections.Generic;
using UniRx;
using InventorySDK.Domain;

namespace EquipmentSDK.Domain
{

    /// <summary>
    /// 디아블로 방식의 장착 시스템 interface
    /// 충돌 검사/배치/제거 로직을 InventoryDomain에서 그대로 재사용
    /// </summary>
    public interface IEquipmentDomain
    {
        /// <summary>해당 부위 그리드의 (행, 열) 크기 view가 슬롯을 그릴 때 사용</summary>
        (int rows, int cols) GetSlotSize(EquipPart part);

        /// <summary>해당 부위에 현재 배치된 아이템들(읽기 전용)</summary>
        IReadOnlyList<PlacedItem> GetPlaced(EquipPart part);

        /// <summary>장착 상태가 바뀔 때마다 어느 부위가 바뀌었는지에 대해 통지</summary>
        IObservable<EquipPart> OnEquipmentChanged { get; }

        /// <summary>
        /// 아이템을 그 아이템의 EquipPart 슬롯에 장착 시도
        /// 슬롯 크기에 안 맞으면 실패, 성공 반환
        /// 슬롯에 기존 아이템이 있으면 빼서 displaced로 돌려줌
        /// </summary>
        bool TryEquip(ItemInfo item, out ItemInfo displaced);

        /// <summary>해당 부위에 걸친 아이템을 해제하고 반환 없으면 null</summary>
        ItemInfo Unequip(EquipPart part, int row, int col);

        /// <summary>해당 부위를 점유한 아이템 조회 없으면 null</summary>
        PlacedItem GetItemAt(EquipPart part, int row, int col);
    }
}
