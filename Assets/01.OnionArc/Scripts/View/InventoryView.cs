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
    /// uGUI 기반 그리드 view 구현체.
    /// 셀은 두 겹(배경 + 아이템 이미지). 클릭/드래그/호버를 모두 처리.
    ///
    /// 드래그(1단계): 좌클릭으로 아이템을 잡으면 고스트가 마우스를 따라다님.
    /// 드롭 결과는 OnDragDropped로 발행(2단계에서 Presenter가 처리).
    /// </summary>
    public class InventoryView : MonoBehaviour, IInventoryView
    {
        [SerializeField] private RectTransform _gridRoot;
        [SerializeField] private GridLayoutGroup _layout;
        [SerializeField] private Sprite _cellSprite;
        [SerializeField] private TooltipView _tooltip;
        [SerializeField] private DragGhostView _dragGhost;  // 공용 드래그 고스트

        private int _rows, _cols;
        private Image[,] _cells;
        private Image[,] _itemImages;
        private ItemInfo[,] _cellItems;

        private readonly Subject<(int, int)> _leftClick  = new Subject<(int, int)>();
        private readonly Subject<(int, int)> _rightClick = new Subject<(int, int)>();
        private readonly Subject<(int, int)> _dragBegan  = new Subject<(int, int)>();
        private readonly Subject<DragDropInfo> _dragDropped = new Subject<DragDropInfo>();

        public IObservable<(int row, int col)> OnCellLeftClicked  => _leftClick;
        public IObservable<(int row, int col)> OnCellRightClicked => _rightClick;
        public IObservable<(int row, int col)> OnDragBegan => _dragBegan;
        public IObservable<DragDropInfo> OnDragDropped => _dragDropped;

        // 현재 드래그 중인 시작 칸 (-1이면 드래그 안 함)
        private int _dragFromRow = -1, _dragFromCol = -1;

        public void BuildGrid(int rows, int cols)
        {
            _rows = rows;
            _cols = cols;
            _cells = new Image[rows, cols];
            _itemImages = new Image[rows, cols];
            _cellItems = new ItemInfo[rows, cols];

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

                    var itemGo = new GameObject("ItemImage", typeof(RectTransform), typeof(Image));
                    var itemRt = itemGo.GetComponent<RectTransform>();
                    itemRt.SetParent(go.transform, false);
                    itemRt.anchorMin = Vector2.zero;
                    itemRt.anchorMax = Vector2.one;
                    itemRt.offsetMin = Vector2.zero;
                    itemRt.offsetMax = Vector2.zero;

                    var itemImg = itemGo.GetComponent<Image>();
                    itemImg.raycastTarget = false;
                    itemImg.enabled = false;
                    _itemImages[r, c] = itemImg;

                    int rr = r, cc = c;
                    var trigger = go.AddComponent<EventTrigger>();

                    // 클릭 (좌=배치/이동 시작은 드래그가, 좌클릭 단독=배치 / 우=장착)
                    var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                    clickEntry.callback.AddListener(data =>
                    {
                        var ped = (PointerEventData)data;
                        if (ped.button == PointerEventData.InputButton.Left)
                            _leftClick.OnNext((rr, cc));
                        else if (ped.button == PointerEventData.InputButton.Right)
                            _rightClick.OnNext((rr, cc));
                    });
                    trigger.triggers.Add(clickEntry);

                    // 드래그 시작
                    var beginEntry = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
                    beginEntry.callback.AddListener(data =>
                    {
                        var ped = (PointerEventData)data;
                        if (ped.button != PointerEventData.InputButton.Left) return;
                        OnBeginDrag(rr, cc, ped.position);
                    });
                    trigger.triggers.Add(beginEntry);

                    // 드래그 중
                    var dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
                    dragEntry.callback.AddListener(data =>
                    {
                        var ped = (PointerEventData)data;
                        if (_dragGhost != null) _dragGhost.Move(ped.position);
                    });
                    trigger.triggers.Add(dragEntry);

                    // 드래그 끝
                    var endEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
                    endEntry.callback.AddListener(data =>
                    {
                        var ped = (PointerEventData)data;
                        OnEndDrag(ped.position);
                    });
                    trigger.triggers.Add(endEntry);

                    // 호버 툴팁
                    var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                    enterEntry.callback.AddListener(data =>
                    {
                        var ped = (PointerEventData)data;
                        ItemInfo item = _cellItems[rr, cc];
                        if (item != null && _tooltip != null)
                            _tooltip.Show(item, ped.position);
                    });
                    trigger.triggers.Add(enterEntry);

                    var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                    exitEntry.callback.AddListener(_ =>
                    {
                        if (_tooltip != null) _tooltip.Hide();
                    });
                    trigger.triggers.Add(exitEntry);
                }
            }
        }

        private void OnBeginDrag(int row, int col, Vector2 screenPos)
        {
            ItemInfo item = _cellItems[row, col];
            if (item == null) return;   // 빈 칸은 드래그 안 함

            _dragFromRow = row;
            _dragFromCol = col;

            // 잡은 아이템의 대표 스프라이트(0번 조각)로 고스트 시작
            Sprite ghostSprite = ItemSpriteLoader.Get(item, 0, 0);
            if (_dragGhost != null)
            {
                _dragGhost.Begin(ghostSprite);
                _dragGhost.Move(screenPos);
            }

            if (_tooltip != null) _tooltip.Hide();   // 드래그 중엔 툴팁 숨김
            _dragBegan.OnNext((row, col));
        }

        private void OnEndDrag(Vector2 screenPos)
        {
            if (_dragGhost != null) _dragGhost.End();

            if (_dragFromRow < 0) return;   // 드래그 안 했으면 무시

            // 놓은 위치가 어느 인벤토리 칸인지 판정
            bool inside = TryGetCellAt(screenPos, out int toRow, out int toCol);

            var info = new DragDropInfo
            {
                fromRow = _dragFromRow,
                fromCol = _dragFromCol,
                toRow = toRow,
                toCol = toCol,
                insideInventory = inside,
            };
            _dragDropped.OnNext(info);

            _dragFromRow = -1;
            _dragFromCol = -1;
        }

        /// <summary>스크린 좌표가 어느 셀 위인지 판정. 인벤토리 밖이면 false.</summary>
        private bool TryGetCellAt(Vector2 screenPos, out int row, out int col)
        {
            row = -1; col = -1;
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    var rt = _cells[r, c].rectTransform;
                    if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null))
                    {
                        row = r; col = c;
                        return true;
                    }
                }
            }
            return false;
        }

        public void Render(IReadOnlyList<PlacedItem> placedItems)
        {
            for (int r = 0; r < _rows; r++)
                for (int c = 0; c < _cols; c++)
                {
                    _cells[r, c].color = EmptyColor;
                    _itemImages[r, c].enabled = false;
                    _itemImages[r, c].sprite = null;
                    _cellItems[r, c] = null;
                }

            for (int i = 0; i < placedItems.Count; i++)
            {
                PlacedItem placed = placedItems[i];
                ItemInfo item = placed.Item;

                foreach (var (r, c) in placed.GetOccupiedCells())
                {
                    if (r < 0 || r >= _rows || c < 0 || c >= _cols) continue;

                    int localRow = r - placed.Row;
                    int localCol = c - placed.Col;

                    Sprite piece = ItemSpriteLoader.Get(item, localRow, localCol);
                    Image itemImg = _itemImages[r, c];

                    if (piece != null)
                    {
                        itemImg.sprite = piece;
                        itemImg.color = Color.white;
                        itemImg.enabled = true;
                    }
                    else
                    {
                        _cells[r, c].color = ColorFor(item.Id);
                    }

                    _cellItems[r, c] = item;
                }
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
