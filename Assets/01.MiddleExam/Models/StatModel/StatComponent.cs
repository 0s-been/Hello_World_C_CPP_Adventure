using System;
using System.Collections.Generic;
using UnityEngine;

public class StatComponent : IStatSystem
{
    public event Action<StatType, float> OnStatChanged;
    public event Action<int> OnSkillPointChanged;

    //dict로 스탯 관리하는 이유
    //string등이나 다른 타입으로 관리하면 새로운 스탯 추가 시 
    //계속 그 case문을 추가해야 하는데 enum + dict로 관리해서 
    //추가해도 기존 코드 수정하지 않고 그냥 유효한 키->그에 대한 값만 수행하도록 함
    //OCP원칙 준수
    private Dictionary<StatType, float> _stats;
    private int _skillPoint;

    public StatComponent(Dictionary<StatType, float> initialstats, int initialSP)
    {
        //원본을 복사해서 사용하여 원본 지킴
        _stats = new Dictionary<StatType, float>(initialstats);
        _skillPoint = initialSP;
    }

    //람다식 사용 
    //조건 => 로직 or 반환값
    //TryGetValue -> key가 존재하지 않을 때 예외 방지
    //out float val -> out 키워드로 val이 TryGetValue의 결과로 할당됨
    public float GetStat(StatType type)
        => _stats.TryGetValue(type, out float val) ? val : 0f;

    public int GetSkillPoint() => _skillPoint;

    public void ApplyStat(StatType type, float amount)
    {
        //키가 없으면 0으로 초기화 후 더하기
        if (_stats.ContainsKey(type) == false) _stats[type] = 0f;

        _stats[type] += amount;

        //변경 이벤트 알림 -> presenter에게
        OnStatChanged?.Invoke(type, _stats[type]);
    }

    public void AddSkillPoint(int amount)
    {
        _skillPoint += amount;
        OnSkillPointChanged?.Invoke(_skillPoint);
    }

    public void SpendSkillPoint(int amount)
    {
        //음수 방지
        _skillPoint = Mathf.Max(0, _skillPoint - amount);
        OnSkillPointChanged?.Invoke(_skillPoint);
    }
}
