using UnityEngine;
using UnityEngine.UI;

namespace OnionArc.View
{
    /// <summary>
    /// 드래그 중 마우스를 따라다니는 고스트 이미지
    /// 인벤토리/장착이 공유함
    /// "스프라이트를 마우스 따라 그린다"는 단일 책임을 짐
    /// 데이터를 안 바꾸는 순수 표시라 Presenter를 거치지 않음
    /// 따라서 tooltipview와 같은 이유로 interface 정의 안 함
    /// </summary>
    public class DragGhostView : MonoBehaviour
    {
        /// <summary> 따라다닐 이미지의 RectTransform </summary>
        [SerializeField] private RectTransform _ghostRect;
        /// <summary> 고스트 이미지 </summary>
        [SerializeField] private Image _ghostImage;
        /// <summary> 마우스 커서로부터 간격 </summary>
        [SerializeField] private Vector2 _offset = Vector2.zero;
        /// <summary> 고스트 이미지의 알파값 <summary>
        [SerializeField] private float _alpha = 0.7f;

        private void Awake()
        {
            // 시작 시 숨김
            if (_ghostImage != null) _ghostImage.enabled = false;
        }

        /// <summary>드래그 시작 -> 잡은 아이템 스프라이트로 고스트를 킴</summary>
        public void Begin(Sprite sprite)
        {
            if (_ghostImage == null) return;

            _ghostImage.sprite = sprite;
            var col = _ghostImage.color;
            col.a = _alpha;
            _ghostImage.color = col;
            _ghostImage.enabled = true;
            // 고스트 이미지가 클릭을 가로채지 않도록 false
            _ghostImage.raycastTarget = false;
        }

        /// <summary>드래그 중 -> 마우스 위치로 고스트를 이동</summary>
        public void Move(Vector2 screenPos)
        {
            if (_ghostRect == null) return;
            _ghostRect.position = screenPos + _offset;
        }

        /// <summary>드래그 끝 -> 고스트 이미지 off </summary>
        public void End()
        {
            if (_ghostImage != null) _ghostImage.enabled = false;
        }
    }
}
