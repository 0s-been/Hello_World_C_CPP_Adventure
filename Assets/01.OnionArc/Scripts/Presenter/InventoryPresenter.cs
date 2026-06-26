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
    /// MonoBehaviour는 생성자 주입이 불가능하므로 [Inject]어트리뷰트를 통해
    /// zenject로 메서드 주입을 사용
    /// ItemCatalogModel이 csv, json에서 로드한 목록으로 채우도록 함. 하드코딩 x
    /// </summary>
    public class InventoryPresenter : MonoBehaviour
    {
        /// <summary>
        /// 인벤토리와 장착 연동용 InvenModel과 EquipModel을 서로 참조하지 않고
        /// Presenter 단에서 조율하도록 함
        /// </summary>
        [SerializeField] private EquipmentPresenter _equipmentPresenter;

        private IInventoryView _view;
        private IInventoryModel _model;
        private IItemCatalogModel _catalog;
        private IConfirmDialogView _confirmDialog;

        private readonly List<ItemInfo> _palette = new List<ItemInfo>();
        private int _paletteIndex;

        // 삭제 확인 대기 중인 칸 (-1이면 대기 없음)
        private int _pendingDeleteRow = -1, _pendingDeleteCol = -1;

        [Inject]
        public void Construct(IInventoryView view, IInventoryModel model,
                              IItemCatalogModel catalog, IConfirmDialogView confirmDialog)
        {
            _view = view;
            _model = model;
            _catalog = catalog;
            _confirmDialog = confirmDialog;
        }

        private void Start()
        {
            _view.BuildGrid(_model.Rows, _model.Cols);

            _model.OnInventoryChanged
                .Subscribe(items => _view.Render(items))
                .AddTo(this);

            _catalog.OnLoaded
                .Subscribe(items =>
                {
                    _palette.Clear();
                    _palette.AddRange(items);
                    _paletteIndex = 0;
                })
                .AddTo(this);

            _view.OnCellLeftClicked
                .Subscribe(cell => OnLeftClick(cell.row, cell.col))
                .AddTo(this);

            _view.OnCellRightClicked
                .Subscribe(cell => OnRightClick(cell.row, cell.col))
                .AddTo(this);

            _view.OnDragDropped
                .Subscribe(info => OnDragDropped(info))
                .AddTo(this);

            // 확인 다이얼로그 결과 구독   확인->삭제 수행   취소->원위치
            if (_confirmDialog != null)
            {
                _confirmDialog.OnConfirmed
                    .Subscribe(_ => OnDeleteConfirmed())
                    .AddTo(this);

                _confirmDialog.OnCancelled
                    .Subscribe(_ => OnDeleteCancelled())
                    .AddTo(this);
            }

            _view.Render(_model.PlacedItems);
            _catalog.Load();
        }

        private void OnLeftClick(int row, int col)
        {
            PlacedItem placed = _model.GetItemAt(row, col);
            if (placed != null) return;   // 아이템 위 좌클릭은 드래그가 담당

            if (_palette.Count == 0) return;
            ItemInfo item = _palette[_paletteIndex];
            if (_model.TryPlace(item, row, col))
            {
                _paletteIndex = (_paletteIndex + 1) % _palette.Count;
            }
        }

        private void OnRightClick(int row, int col)
        {
            PlacedItem placed = _model.GetItemAt(row, col);
            if (placed == null) return;

            if (_equipmentPresenter != null)
                _equipmentPresenter.EquipFromInventory(placed.Item, placed.Row, placed.Col);
        }

        private void OnDragDropped(DragDropInfo info)
        {
            if (info.insideInventory)
            {
                // 인벤토리 내 이동
                if (info.fromRow == info.toRow && info.fromCol == info.toCol) return;
                _model.TryMove(info.fromRow, info.fromCol, info.toRow, info.toCol);
            }
            else
            {
                // 인벤토리 밖 -> 삭제 확인. 후보 칸을 기억하고 확인창을 띄움
                PlacedItem placed = _model.GetItemAt(info.fromRow, info.fromCol);
                if (placed == null) return;

                _pendingDeleteRow = info.fromRow;
                _pendingDeleteCol = info.fromCol;

                if (_confirmDialog != null)
                    _confirmDialog.Show("아이템을 파괴하면 다시 복구할 수 없습니다.\n정말로 파괴하시겠습니까?");
                else
                    _pendingDeleteRow = -1;   // 다이얼로그 없으면 삭제 안 함
            }
        }

        /// <summary>확인 클릭 -> 기억해둔 칸을 model에 삭제 명령</summary>
        private void OnDeleteConfirmed()
        {
            if (_pendingDeleteRow < 0) return;
            _model.RemoveAt(_pendingDeleteRow, _pendingDeleteCol);
            _pendingDeleteRow = -1;
            _pendingDeleteCol = -1;
        }

        /// <summary>취소 클릭 -> 아무것도 안 함 원위치 유지시키고 대기만 비움</summary>
        private void OnDeleteCancelled()
        {
            _pendingDeleteRow = -1;
            _pendingDeleteCol = -1;
        }
    }
}
