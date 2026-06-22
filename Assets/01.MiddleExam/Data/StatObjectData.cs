using UnityEngine;

//언리얼 엔진에서 사용했던 data asset과 유사한 SO를 통해
//데이터 드리븐 방식을 통해 statoject를 관리하도록 구현했습니다.
[CreateAssetMenu(fileName = "StatObjectData", menuName = "Scriptable Objects/StatObjectData")]
public class StatObjectData : ScriptableObject
{
    [Header("Stat Info")]
    public StatType statType;
    public float amount;

    [Header("UI Info")]
    public Sprite icon;
    public string displayname;
}

