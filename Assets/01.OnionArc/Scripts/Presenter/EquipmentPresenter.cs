using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
using OnionArc.Model;
using OnionArc.View;
using InventorySDK.Domain;

namespace OnionArc.Presenter
{

    /// <summary>
    /// 장착시스템 presenter 
    ///
    /// 두 model(Inventory, Equipment)을 모두 알고 연결함
    /// 두 model은 서로 모르고, 연동 로직은 이 Presenter에만 모임
    /// view나 특정 결과에 대한 판단, 판단에 빠른 어떤 기능을 수행할 지 정하는 건 presenter
    /// 장착 변경 통지 -> 해당 부위 그리드만 갱신
    /// 슬롯 클릭 → 해제 -> 인벤토리로 되돌림
    /// EquipFromInventory -> 인벤토리 아이템을 장착
    /// </summary>
    public class EquipmentPresenter : MonoBehaviour
    {
        private IEquipmentView _view;
        private IEquipmentModel _equipment;
        private IInventoryModel _inventory;

        // 어느 부위들이 있는지 (그리드 생성/렌더용)
        private static readonly EquipPart[] Parts =
        {
            EquipPart.Hat, EquipPart.Weapon, EquipPart.Chest, EquipPart.Legs, EquipPart.Shoes
        };

        [Inject]
        public void Construct(IEquipmentView view, IEquipmentModel equipment, IInventoryModel inventory)
        {
            _view = view;
            _equipment = equipment;
            _inventory = inventory;
        }

        private void Start()
        {
            // 부위별 그리드 생성 (각 부위 크기를 Model에서 받아 전달)
            var sizes = new Dictionary<EquipPart, (int rows, int cols)>();
            for (int i = 0; i < Parts.Length; i++)
            {
                sizes[Parts[i]] = _equipment.GetSlotSize(Parts[i]);
            }
            _view.BuildGrids(sizes);

            // 장착 변경 통지 -> 해당 부위만 다시 그림
            _equipment.OnEquipmentChanged
                .Subscribe(part => _view.RenderPart(part, _equipment.GetPlaced(part)))
                .AddTo(this);

            // 슬롯 클릭 -> 해제 -> 인벤토리로 되돌림
            _view.OnSlotClicked
                .Subscribe(slot => UnequipToInventory(slot.part, slot.row, slot.col))
                .AddTo(this);

            // 초기 렌더 (전부 빈 상태)
            for (int i = 0; i < Parts.Length; i++)
                _view.RenderPart(Parts[i], _equipment.GetPlaced(Parts[i]));
        }

        /// <summary>
        /// 인벤토리 아이템을 장착 
        /// 교체로 밀려난 기존 아이템은 인벤토리로 되돌림
        /// 인벤토리 Presenter가 장착 가능한 아이템 클릭 시 호출
        /// </summary>
        public bool EquipFromInventory(ItemInfo item, int invRow, int invCol)
        {
            if (item == null || item.EquipPart == EquipPart.None)
                return false;

            // 먼저 장착 시도 (성공해야 인벤토리에서 뺌)
            if (_equipment.TryEquip(item, out ItemInfo displaced)== false)
                return false;

            // 장착 성공 -> 인벤토리에서 제거
            _inventory.RemoveAt(invRow, invCol);

            // 교체로 밀려난 기존 장비는 인벤토리로 되돌림
            if (displaced != null)
                PlaceBackToInventory(displaced);

            return true;
        }

        /// <summary>
        /// 장착 해제 -> 인벤토리로 되돌림.
        /// 인벤토리에 자리가 없으면 해제 자체를 막아서 예외처리
        /// "해제 후 되돌리기"가 아니라 "되돌릴 수 있을 때만 해제" 순서로 해야
        /// 자리가 없을 때 아이템이 증발하지 않음
        /// </summary>
        private void UnequipToInventory(EquipPart part, int row, int col)
        {
            // 인벤토리에 넣어보려면 무엇인지 알아야 하므로
            // 해제 대상 아이템을 먼저 알아냄
            PlacedItem target = _equipment.GetItemAt(part, row, col);
            if (target == null) return;

            ItemInfo item = target.Item;

            // 되돌릴 자리가 있는 지 확인하기 위해 인벤토리에 먼저 넣어봄
            if (!TryPlaceToInventory(item))
                return;   // 자리 없음 -> 해제하지 않음 (아이템 보존, 장착 유지)

            // 인벤토리에 들어갔으니 이제 장착 슬롯에서 제거
            _equipment.Unequip(part, row, col);
        }

        /// <summary>인벤토리 빈 곳을 찾아 배치 시도 </summary>
        private bool TryPlaceToInventory(ItemInfo item)
        {
            for (int r = 0; r < _inventory.Rows; r++)
            {
                for (int c = 0; c < _inventory.Cols; c++)
                {
                    if (_inventory.TryPlace(item, r, c))  return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 교체로 밀려난 아이템을 인벤토리로 되돌림
        /// </summary>
        private void PlaceBackToInventory(ItemInfo item)
        {
            if (TryPlaceToInventory(item)) return;
        }
    }
}
