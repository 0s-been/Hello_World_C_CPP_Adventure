using UnityEngine;
using System;

/// <summary>
/// 게임 입력이 막힘을 제어하는 클래스
///
///   기존엔 PlayerController, PlayerCamera, ComboAtk, SkillManager가 각자
///   m_IsMiniGameActive 플래그를 따로 들고 토글했음. UI가 늘어날 때마다
///   네 군데를 똑같이 고쳐야 함. ->ocp 위반
///   이제 차단 상태에 대한 플래그를 여기에 두고, 각 컴포넌트는 IsBlocked만 조회하도록해서
///   차단 여부에 대한 상태만 알도록 수정
///
///   bool이 아닌 정수형 count로 한 이유
///   블록 카운트 패턴이라는 것을 알게 됨.
///   스킬트리와 인벤토리가 동시에 열릴 수 있음. bool 하나면 먼저 닫는 쪽이
///   차단을 통째로 풀어버림. 그래서 "열린 ui혹은 차단을 요구하는 쪽의 수"를 세고,
///   0이 될 때만 입력을 복구
///
///   MonoBehaviour가 아닌 정적 클래스로 둔 이유
///   씬 어디서든 참조해야 하고 인스턴스가 하나뿐이면 충분하기 때문.
///   Zenject로 싱글톤 주입해도 되지만, 기존 컴포넌트들이 Zenject를 안 쓰므로
///   과제 외에 작업량이 늘어나서 추후에 리팩토링해야할듯
/// </summary>
public static class InputBlocker
{
    private static int _blockCount = 0;

    public static bool IsBlocked => _blockCount > 0;
    public static event Action<bool> OnBlockChanged;

    public static void Push()
    {
        _blockCount++;

        if (_blockCount == 1)
        {
            //0->1 입력 블락 시작
            OnBlockChanged?.Invoke(true);
        }
    }

    public static void Pop()
    {
        if (_blockCount == 0) return;

        _blockCount--;

        if (_blockCount == 0)
        {
            //1 -> 0 입력 블락 해제
            OnBlockChanged?.Invoke(false);
        }
    }

    public static void Reset()
    {
        bool temp = IsBlocked;
        _blockCount = 0;

        if (temp)
        {
            OnBlockChanged?.Invoke(false);
        }
    }
}
