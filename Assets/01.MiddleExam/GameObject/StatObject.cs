using UnityEngine;
using UnityEngine.UI;
using System;

public class StatObject : MonoBehaviour
{
    public int Row { get; private set; }
    public int Col { get; private set; }

    //statobj -> View
    //v가 이 이벤트들을 구독해서 p에게 중계해줌
    //그럼 p는 statobj를 직접 참조하지 않아도 됨
    public event Action<HitResult> OnHit;

    private StatType m_type;
    private float m_amount;
    private Image m_image;

    public RectTransform m_rect { get; private set; }
    

    private void Awake()
    {
        m_image = GetComponent<Image>();
        m_rect = GetComponent<RectTransform>();

    }

    public void Init(int row, int col, StatObjectViewModel vm)
    {
        Row                     = row;
        Col                     = col;
        m_type                  = vm.type;
        m_amount                = vm.amount;
        m_image.sprite          = vm.icon;
        m_image.color           = Color.white;

    }

    // UIManager가 충돌 감지 후 직접 호출
    public void OnArrowHit()
    {
        var result = new HitResult
        {
            type = m_type,
            amount = m_amount,
            row = Row,
            col = Col
        };

        OnHit?.Invoke(result);
        gameObject.SetActive(false);
    }

    //근데 충돌 되면 setactive가 false로 되서 이펙트가 안 되잖아
    //그럼 이펙트 플레이 후 비활성화?
    //그럼 중복 충돌은?
    //파괴 이펙트는 v가 하는 걸로?
    public void PlayDestroyEffect()
    {

    }
 
}
