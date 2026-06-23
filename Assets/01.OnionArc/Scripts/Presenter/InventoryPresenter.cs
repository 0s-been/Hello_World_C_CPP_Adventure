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
    ///
    /// MonoBehaviour는 생성자 주입이 불가능하므로 [Inject]어트리뷰트를 통해
    /// zenject로 메서드 주입을 사용
    /// ItemCatalogModel이 csv, json에서 로드한 목록으로 채우도록 함. 하드코딩 x
    /// </summary>
    public class InventoryPresenter : MonoBehaviour
    {
        private IInventoryView _view;
        private IInventoryModel _model;
        private IItemCatalogModel _catalog;

        /// <summary>
        /// 카탈로그에서 로드된 아이템들이 채워짐
        /// </summary>
        private readonly List<ItemInfo> _palette = new List<ItemInfo>();
        private int _paletteIndex;

        /// <summary>
        /// zenject로 매개변수 받음
        /// </summary>
        [Inject]
        public void Construct(IInventoryView view, IInventoryModel model, IItemCatalogModel catalog)
        {
            _view = view;
            _model = model;
            _catalog = catalog;
        }

        private void Start()
        {
            // 빈 그리드 생성
            _view.BuildGrid(_model.Rows, _model.Cols);

            // Model의 변경 통지 -> View 갱신 
            _model.OnInventoryChanged
                .Subscribe(items => _view.Render(items))
                .AddTo(this);

            // 카탈로그 로드 완료 -> _palette 채움 
            _catalog.OnLoaded
                .Subscribe(items =>
                {
                    _palette.Clear();
                    _palette.AddRange(items);
                    _paletteIndex = 0;
                    //Debug.Log($"[InventoryPresenter] 아이템 {_palette.Count}종 로드됨");
                })
                .AddTo(this);

            // view의 좌클릭 -> 현재 팔레트 위치에 아이템 배치 시도
            _view.OnCellLeftClicked
                .Subscribe(cell =>
                {
                    if (_palette.Count == 0) return; // 아직 로드 전이면 무시
                    var item = _palette[_paletteIndex];
                    if (_model.TryPlace(item, cell.row, cell.col))
                        _paletteIndex = (_paletteIndex + 1) % _palette.Count;
                })
                .AddTo(this);

            // view의 우클릭 -> 제거 시도
            _view.OnCellRightClicked
                .Subscribe(cell => _model.RemoveAt(cell.row, cell.col))
                .AddTo(this);

            // 초기 렌더, 데이터 로드 시작
            _view.Render(_model.PlacedItems);
            _catalog.Load();
        }
    }
}
