
// #########################################
// 이동 관련 애니메이션 상태 전달에 필요한 메서드만 정의해놓고 인터페이스를 상속받는 클래스에서 구현하도록 해서
// SOLID원칙 중 OCP를 준수하도록 하였으며 인터페이스를 기능별로 나누어서 ISP도 준수하도록 하였습니다.
// PlayerAnimationBridge가 PlayerController와 PlayerAniController 사이에서 상태에 대한 정보를 넘기는 과정에서
// 두 클래스의 전체에 대해 알 필요 없이 이 인터페이스만 알면 되도록 하였습니다.
// #########################################
public interface IMovementAnimatable
{
    void SetMovement(bool isMoving, float speed);
    void SetGrounded(bool isGrounded);
    void SetVerticalVelocity(float vel);
}
