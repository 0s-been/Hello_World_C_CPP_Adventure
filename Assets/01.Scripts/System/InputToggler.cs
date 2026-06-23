using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 패널의 열림/닫힘만 담당하는 전용 컴포넌트
///
///   이 컴포넌트는 "켜고 끌 패널" 자신이 아니라 그 바깥에 둬야함
///   패널을 SetActive(false)로 끄면 그 안의 스크립트도 멈추기 때문.
///   _panel 슬롯에 켜고 끌 패널을 연결한다.
/// </summary>

public class InputToggler : MonoBehaviour
{
    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private bool _openOnStart = false;

    private InputReader _inputReader;
    private bool _IsOpen;

    //아직 입력 담당 관련하는 부분은 zenject를 미사용하던 시절이라
    //이 입력 체계는 나중에 싹 다 갈아 엎을수도
    private void Awake()
    {
        _inputReader = FindAnyObjectByType<InputReader>();

        if (_inputReader != null)
        {
            _inputReader.OnInventoryInput += Toggle;
        }
        else
        {
            Debug.LogError("[InventoryToggle] InputReader를 찾지 못함");
        }
    }

    private void Start()
    {
        SetOpen(_openOnStart);
    }

    private void OnDestroy()
    {
        if (_inputReader != null)
        {
            _inputReader.OnInventoryInput -= Toggle;
        }

        if (_IsOpen)
        {
            InputBlocker.Pop();
        }
    }

    private void Toggle()
    {
        SetOpen(!_IsOpen);
    }

    private void SetOpen(bool open)
    {
        if (open == _IsOpen) return;

        _IsOpen = open;

        if (_panel != null)
        {
            _panel.SetActive(open);
        }

        if(open) InputBlocker.Push();
        else     InputBlocker.Pop();
    }
}
