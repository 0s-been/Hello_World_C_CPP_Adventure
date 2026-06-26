using System;
using System.Collections.Generic;
using UniRx;
using InventorySDK.Domain;

namespace EquipmentSDK.Domain
{

    /// <summary>
    /// IEquipmentDomain 구현체
    /// 인벤토리 로직을 재사용
    /// </summary>
    public class EquipmentDomain : IEquipmentDomain
    {
        // 부위별 그리드 크기 (행, 열) - 한 곳에 모아 하드코딩
        // 자주 바뀌지 않는 값이므로 데이터 파일 대신 여기서 관리
        private static readonly Dictionary<EquipPart, (int rows, int cols)> SlotSizes
            = new Dictionary<EquipPart, (int, int)>
        {
            { EquipPart.Hat,    (2, 2) },
            { EquipPart.Weapon, (3, 1) },  
            { EquipPart.Chest,  (3, 2) },  
            { EquipPart.Legs,   (3, 2) },
            { EquipPart.Shoes,  (2, 2) },
        };

        // 부위별 작은 인벤토리
        // 여기에 배치/충돌/제거를 위임해서 재사용
        private readonly Dictionary<EquipPart, IInventoryDomain> _slots
            = new Dictionary<EquipPart, IInventoryDomain>();

        private readonly Subject<EquipPart> _onChanged = new Subject<EquipPart>();
        public IObservable<EquipPart> OnEquipmentChanged => _onChanged;

        public EquipmentDomain()
        {
            // 각 부위를 정해진 크기의 InventoryDomain으로 생성
            foreach (var pair in SlotSizes)
            {
                _slots[pair.Key] = new InventoryDomain(pair.Value.rows, pair.Value.cols);
            }
        }

        public (int rows, int cols) GetSlotSize(EquipPart part)
        {
            return SlotSizes.TryGetValue(part, out var size) ? size : (0, 0);
        }

        public IReadOnlyList<PlacedItem> GetPlaced(EquipPart part)
        {
            return _slots.TryGetValue(part, out var grid)? grid.PlacedItems : new List<PlacedItem>();
        }

        public bool TryEquip(ItemInfo item, out ItemInfo displaced)
        {
            displaced = null;

            // 소비 아이템 같은 장착 불가 아이템 거부
            if (item == null || item.EquipPart == EquipPart.None)
                return false;

            // 해당 부위 슬롯 찾기
            if (_slots.TryGetValue(item.EquipPart, out var grid) == false)
                return false;

            // 항상 좌상단(0,0)에 배치 시도
            // 슬롯보다 아이템이 크면 CanPlace가 false -> 장착 실
            if (grid.CanPlace(item, 0, 0) == false)
            {
                // (0,0)에 못 놓는 경우 -> 이미 뭔가 있거나 크기 초과
                // 기존 아이템이 있으면 빼서 교체 시도
                var existing = grid.GetItemAt(0, 0);
                if (existing == null)
                    return false;   // 크기 초과 등 다른 이유 -> 실패

                // 교체 -> 기존 것을 빼고 새 것을 넣는다
                displaced = existing.Item;
                grid.RemoveAt(existing.Row, existing.Col);

                // 기존 것을 뺐으니 이제 들어갈 수 있어야 함
                if (grid.TryPlace(item, 0, 0) == false)
                {
                    // 만약 새 것도 못 들어가면 기존 것 복구
                    grid.TryPlace(displaced, 0, 0);
                    displaced = null;
                    return false;
                }

                _onChanged.OnNext(item.EquipPart);
                return true;
            }

            // 빈 슬롯에 정상 배치
            grid.TryPlace(item, 0, 0);
            _onChanged.OnNext(item.EquipPart);
            return true;
        }

        public ItemInfo Unequip(EquipPart part, int row, int col)
        {
            if (_slots.TryGetValue(part, out var grid) == false)
                return null;

            var target = grid.GetItemAt(row, col);
            if (target == null) return null;

            grid.RemoveAt(target.Row, target.Col);
            _onChanged.OnNext(part);
            return target.Item;
        }

        public PlacedItem GetItemAt(EquipPart part, int row, int col)
        {
            return _slots.TryGetValue(part, out var grid)? grid.GetItemAt(row, col) : null;
        }
    }
}
