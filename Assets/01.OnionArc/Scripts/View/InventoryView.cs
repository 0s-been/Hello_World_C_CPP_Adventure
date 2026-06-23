using System.Collections.Generic;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InventorySDK.Domain;

namespace OnionArc.View
{

    /// <summary>
    /// uGUI 기반 그리드 view 구현체. 화면 표시 + 입력의 IObservable 변환을 담당
    /// 셀이 클릭되면 이벤트를 받아 (row,col) 스트림으로 바꿔 보냄
    /// presenter는 unity 입력 API를 전혀 모르고 "셀이 클릭됨"이라는 의미만 받으면 됨
    /// </summary>
    public class InventoryView : MonoBehaviour, IInventoryView
    {
        /// <summary> 셀들이 붙을 부모 </summary>
        [SerializeField] private RectTransform _gridRoot;   // 셀들이 붙을 부모
        /// <summary>  격자 정렬 </summary>
        [SerializeField] private GridLayoutGroup _layout;  
        /// <summary> 빈 셀 배경 </summary>
        [SerializeField] private Sprite _cellSprite;

        private int _rows, _cols;
        /// <summary> 각 셀의 색을 바꿔 아이템 점유를 표현 </summary>
        private Image[,] _cells;  

        private readonly Subject<(int, int)> _leftClick  = new Subject<(int, int)>();
        private readonly Subject<(int, int)> _rightClick = new Subject<(int, int)>();

        public IObservable<(int row, int col)> OnCellLeftClicked  => _leftClick;
        public IObservable<(int row, int col)> OnCellRightClicked => _rightClick;

        public void BuildGrid(int rows, int cols)
        {
            _rows = rows;
            _cols = cols;
            _cells = new Image[rows, cols];

            if (_layout != null)
                _layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            if (_layout != null)
                _layout.constraintCount = cols;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var go = new GameObject($"Cell_{r}_{c}", typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(_gridRoot, false);

                    var img = go.GetComponent<Image>();
                    img.sprite = _cellSprite;
                    img.color = EmptyColor;
                    _cells[r, c] = img;

                    // 클릭을 (row,col) 스트림으로 변환 
                    int rr = r, cc = c;
                    var trigger = go.AddComponent<EventTrigger>();
                    var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                    entry.callback.AddListener(data =>
                    {
                        var ped = (PointerEventData)data;
                        if (ped.button == PointerEventData.InputButton.Left)
                            _leftClick.OnNext((rr, cc));
                        else if (ped.button == PointerEventData.InputButton.Right)
                            _rightClick.OnNext((rr, cc));
                    });
                    trigger.triggers.Add(entry);
                }
            }
        }

        public void Render(IReadOnlyList<PlacedItem> placedItems)
        {
            // 전부 빈 색으로 초기화
            for (int r = 0; r < _rows; r++)
                for (int c = 0; c < _cols; c++)
                    _cells[r, c].color = EmptyColor;

            // 배치된 아이템이 점유한 칸을 색칠
            for (int i = 0; i < placedItems.Count; i++)
            {
                PlacedItem placed = placedItems[i];
                Color color = ColorFor(placed.Item.Id);
                foreach (var (r, c) in placed.GetOccupiedCells())
                    if (r >= 0 && r < _rows && c >= 0 && c < _cols)
                        _cells[r, c].color = color;
            }
        }

        private static readonly Color EmptyColor = new Color(0.18f, 0.18f, 0.2f, 1f);

        private static Color ColorFor(string id)
        {
            switch (id)
            {
                case "sword":  return new Color(0.30f, 0.55f, 0.85f);
                case "shield": return new Color(0.20f, 0.65f, 0.55f);
                case "potion": return new Color(0.85f, 0.45f, 0.35f);
                default:       return Color.gray;
            }
        }
    }
}
