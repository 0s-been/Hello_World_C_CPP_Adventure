using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InventorySDK.Domain;

namespace OnionArc.View
{

    /// <summary>
    /// 장착 View 구현체 (그리드 방식). 셀은 두 겹 구조(배경 + 아이템 이미지).
    /// 아이템 배경이 투명해도 뒤의 배경 셀이 비쳐서 안 뚫린다.
    /// </summary>
    public class EquipmentView : MonoBehaviour, IEquipmentView
    {
        [SerializeField] private RectTransform _hatRoot;
        [SerializeField] private RectTransform _weaponRoot;
        [SerializeField] private RectTransform _chestRoot;
        [SerializeField] private RectTransform _legsRoot;
        [SerializeField] private RectTransform _shoesRoot;

        [SerializeField] private Sprite _cellSprite;
        [SerializeField] private float _cellSize = 40f;
        [SerializeField] private TooltipView _tooltip;

        private readonly Subject<(EquipPart, int, int)> _onSlotClicked
            = new Subject<(EquipPart, int, int)>();
        public IObservable<(EquipPart part, int row, int col)> OnSlotClicked => _onSlotClicked;

        private Dictionary<EquipPart, Image[,]> _cells;       // 배경
        private Dictionary<EquipPart, Image[,]> _itemImages;  // 아이템 조각
        private Dictionary<EquipPart, ItemInfo[,]> _cellItems;
        private Dictionary<EquipPart, RectTransform> _roots;

        private void Awake()
        {
            EnsureRoots();
        }

        private void EnsureRoots()
        {
            if (_roots != null) return;
            _roots = new Dictionary<EquipPart, RectTransform>
            {
                { EquipPart.Hat,    _hatRoot },
                { EquipPart.Weapon, _weaponRoot },
                { EquipPart.Chest,  _chestRoot },
                { EquipPart.Legs,   _legsRoot },
                { EquipPart.Shoes,  _shoesRoot },
            };
        }

        public void BuildGrids(IReadOnlyDictionary<EquipPart, (int rows, int cols)> sizes)
        {
            if (_roots == null) EnsureRoots();

            _cells = new Dictionary<EquipPart, Image[,]>();
            _itemImages = new Dictionary<EquipPart, Image[,]>();
            _cellItems = new Dictionary<EquipPart, ItemInfo[,]>();

            foreach (var pair in sizes)
            {
                EquipPart part = pair.Key;
                int rows = pair.Value.rows;
                int cols = pair.Value.cols;

                if (!_roots.TryGetValue(part, out var root) || root == null)
                    continue;

                var grid = new Image[rows, cols];
                var itemGrid = new Image[rows, cols];
                var items = new ItemInfo[rows, cols];
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        // 배경 셀
                        var go = new GameObject($"Cell_{part}_{r}_{c}",
                            typeof(RectTransform), typeof(Image));
                        var rt = go.GetComponent<RectTransform>();
                        rt.SetParent(root, false);
                        rt.sizeDelta = new Vector2(_cellSize, _cellSize);
                        rt.anchoredPosition = new Vector2(c * _cellSize, -r * _cellSize);

                        var img = go.GetComponent<Image>();
                        img.sprite = _cellSprite;
                        img.color = EmptyColor;
                        grid[r, c] = img;

                        // 자식 아이템 이미지
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
                        itemGrid[r, c] = itemImg;

                        int rr = r, cc = c;
                        EquipPart pp = part;
                        var trigger = go.AddComponent<EventTrigger>();

                        var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                        clickEntry.callback.AddListener(_ => _onSlotClicked.OnNext((pp, rr, cc)));
                        trigger.triggers.Add(clickEntry);

                        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                        enterEntry.callback.AddListener(data =>
                        {
                            var ped = (PointerEventData)data;
                            ItemInfo item = _cellItems[pp][rr, cc];
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
                _cells[part] = grid;
                _itemImages[part] = itemGrid;
                _cellItems[part] = items;
            }
        }

        public void RenderPart(EquipPart part, IReadOnlyList<PlacedItem> placed)
        {
            if (_cells == null || !_cells.TryGetValue(part, out var grid)) return;
            var itemGrid = _itemImages[part];
            var items = _cellItems[part];

            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // 초기화
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c].color = EmptyColor;
                    itemGrid[r, c].enabled = false;
                    itemGrid[r, c].sprite = null;
                    items[r, c] = null;
                }

            // 배치된 아이템
            for (int i = 0; i < placed.Count; i++)
            {
                PlacedItem p = placed[i];
                ItemInfo item = p.Item;

                foreach (var (r, c) in p.GetOccupiedCells())
                {
                    if (r < 0 || r >= rows || c < 0 || c >= cols) continue;

                    int localRow = r - p.Row;
                    int localCol = c - p.Col;

                    Sprite piece = ItemSpriteLoader.Get(item, localRow, localCol);
                    Image itemImg = itemGrid[r, c];

                    if (piece != null)
                    {
                        itemImg.sprite = piece;
                        itemImg.color = Color.white;
                        itemImg.enabled = true;
                    }
                    else
                    {
                        grid[r, c].color = ColorFor(item.Id);
                    }

                    items[r, c] = item;
                }
            }
        }

        private static readonly Color EmptyColor = new Color(0.18f, 0.18f, 0.2f, 1f);

        private static Color ColorFor(string id)
        {
            switch (id)
            {
                case "sword":   return new Color(0.30f, 0.55f, 0.85f);
                case "shield":  return new Color(0.20f, 0.65f, 0.55f);
                case "helmet":  return new Color(0.75f, 0.65f, 0.30f);
                case "boots":   return new Color(0.60f, 0.45f, 0.70f);
                case "bow":     return new Color(0.85f, 0.55f, 0.30f);
                case "plate":   return new Color(0.50f, 0.55f, 0.60f);
                case "greaves": return new Color(0.45f, 0.50f, 0.40f);
                case "cap":     return new Color(0.80f, 0.70f, 0.40f);
                case "sandals": return new Color(0.55f, 0.50f, 0.65f);
                default:        return Color.gray;
            }
        }
    }
}
