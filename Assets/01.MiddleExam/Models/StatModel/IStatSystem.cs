using System;
using UnityEngine;

public interface IStatSystem
{
    //조회
    float GetStat(StatType statType);
    int GetSkillPoint();

    //변경
    void ApplyStat(StatType type, float amount);
    void AddSkillPoint(int amount);
    void SpendSkillPoint(int amount);

    //이벤트
    event Action<StatType, float> OnStatChanged;
    event Action<int> OnSkillPointChanged;

}
