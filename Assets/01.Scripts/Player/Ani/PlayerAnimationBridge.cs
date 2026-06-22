using UnityEngine;
// ################################################
//매 프레임마다 플레이어의 상태를 체크하고 그 데이터를 PlayerAniController에 전달하는 역할만 담당합니다.
//플레이어와 애님컨트롤러 사이에서 작동하는 중간다리 or 인터페이스 같은 역할을 통해 player controller와 anicontroller의 의존성을 줄이는 구조입니다.
//player controller가 직접 anicontroller를 호출하던 UpdateAnimation()을 분리하였습니다.
//solid원칙 중에서 srp을 준수하기 위해 playercontroller는 플레이어의 상태를 체크하고
//PlayerAnimationBridge는 그 정보를 animcontroller에게 전달만 하도록 기능을 나눠서 구현했습니다.
//이 과정에서 인터페이스를 추가하고 정보는 인터페이스로 전달하여 ISP와 OCP도 준수하도록 했습니다.
// ################################################
public class PlayerAnimationBridge : MonoBehaviour
{

    private IMovementAnimatable m_AnimController;
    private Rigidbody           m_Rigidbody;
    private PlayerController    m_Controller;

    void Awake()
    {
        m_AnimController = GetComponentInChildren<PlayerAniController>();
        m_Rigidbody      = GetComponent<Rigidbody>();
        m_Controller     = GetComponent<PlayerController>();

    }

    public void Tick()
    {
        // 각 컴포넌트가 제대로 할당되지 않았을 경우 예외처리
        if (m_AnimController == null || m_Controller == null)
        {
            return;
        }

        m_AnimController.SetMovement(m_Controller.IsMoving, m_Controller.MoveDir.magnitude);
        m_AnimController.SetGrounded(m_Controller.IsGrounded);
        m_AnimController.SetVerticalVelocity(m_Rigidbody.linearVelocity.y);
    }
}
