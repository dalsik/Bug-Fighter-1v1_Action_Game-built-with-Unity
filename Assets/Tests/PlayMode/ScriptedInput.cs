// 테스트가 "이번 프레임에 이걸 하라"고 직접 정하는 입력.
// MonoBehaviour 도 아니다 — CharacterRoot 는 ICharacterInput 약속만 보기 때문에 그대로 꽂힌다.
public class ScriptedInput : ICharacterInput
{
    public float Horizontal { get; set; }
    public int DashDirection { get; set; }
    public bool JumpPressed { get; set; }
    public bool ShieldPressed { get; set; }
    public AttackSlotType? Attack { get; set; }

    public bool AttackPressed(AttackSlotType slot) => Attack == slot;

    public void Clear()
    {
        Horizontal = 0f;
        DashDirection = 0;
        JumpPressed = false;
        ShieldPressed = false;
        Attack = null;
    }
}
