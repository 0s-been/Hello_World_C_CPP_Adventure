using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InventorySDK.Domain;

namespace OnionArc.View
{
    /// <summary>
    /// 아이템 설명 툴팁 view
    /// 인벤토리/장착 양쪽이 공유함 
    /// 이 view는 단순 "ItemInfo를 받아 화면에 띄운다"는 단일 책임만 가짐
    /// 데이터를 바꾸지 않는 순수 표시라 Presenter를 거치지 않고 View가 직접 호출한다
    /// 그렇기 때문에 ITooltipView 같은 인터페이스가 없음
    /// 왜냐 mvp패턴에서 interface는 외부와의 어떤 식으로 상호작용하는 지에 대해 정의하는 일종의 계약서인데
    /// 외부와의 상호작용 없이 단순 데이터를 렌더링하는 곳이기 때문
    /// 하지만 같은 view 계층에선 상호작용함
    /// 혹시하도 추후 확장할 경우를 위해 interface를 만들까 고민했지만 yagni원칙에 따라 패스
    /// </summary>
    public class TooltipView : MonoBehaviour
    {
        /// <summary> 툴팁 전체 패널 (켜고 끔)</summary>
        [SerializeField] private GameObject _panel;
        /// <summary> 위치 이동용 </summary>
        [SerializeField] private RectTransform _panelRect;
        /// <summary> 아이템 이름 텍스트 </summary>
        [SerializeField] private TextMeshProUGUI _nameText;
        /// <summary> 차지하는 row, col 크기와 부위에 대한 텍스트 </summary>
        [SerializeField] private TextMeshProUGUI _infoText;
        /// <summary> 아이템 설정 및 효과에 대한 텍스트 </summary>
        [SerializeField] private TextMeshProUGUI _descText;
        /// <summary> 마우스 커서와 툴팁 ui사이의 간격 </summary>
        [SerializeField] private Vector2 _offset = new Vector2(16f, -16f);

        private void Awake()
        {
            // 시작 시 숨김
            if (_panel != null) _panel.SetActive(false);
        }

        /// <summary>아이템 정보를 받아 툴팁을 띄움 </summary>
        public void Show(ItemInfo item, Vector2 screenPos)
        {
            if (item == null || _panel == null) return;

            if (_nameText != null)
                _nameText.text = item.Name;

            if (_infoText != null)
                _infoText.text = $"{item.Width} x {item.Height}" +
                                 (item.EquipPart != EquipPart.None ? $"  ·  {item.EquipPart}" : "");

            if (_descText != null)
            {
                // 설명이 없으면 설명 줄 자체를 숨김
                bool hasDesc = !string.IsNullOrWhiteSpace(item.Description);
                _descText.gameObject.SetActive(hasDesc);
                if (hasDesc) _descText.text = item.Description;
            }

            _panel.SetActive(true);
            Move(screenPos);
        }

        /// <summary> 툴팁 off </summary>
        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        /// <summary >커서 위치를 따라 툴팁 위치를 갱신</summary>
        public void Move(Vector2 screenPos)
        {
            if (_panelRect == null) return;
            _panelRect.position = screenPos + _offset;
        }
    }
}
