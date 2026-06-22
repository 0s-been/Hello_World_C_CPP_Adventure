// #########################################
// 점프, 대쉬 등 특정 액션에 대한 애니메이션을 담당하는 인터페이스입니다.
// 전투에 필요한 특정 액션들은 앞으로 이곳에 추가하면 되기 때문에 OCP를 준수할 수 있습니다.
// 인터페이스를 기능별로 나누어서 ISP도 준수하도록 하였습니다.
// PlayerJump와 PlayerDash가 PlayerAniController 전체 대신 이 인터페이스만 알면 되도록 하였습니다.
// #########################################
public interface ICombatAnimatable
{
    void TriggerJump();
    void TriggerDash();
    void SetDashing(bool isDashing);
}
