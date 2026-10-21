// 캐릭터가 이번 프레임에 "무엇을 하라는지"만 묻는 창구 — 어떤 키인지는 모른다.
// 키보드(KeyboardInput) · AI · 테스트용 입력이 이 약속만 지키면 갈아 끼울 수 있다.
public interface ICharacterInput
{
    float Horizontal { get; }       // -1(왼쪽) ~ +1(오른쪽)
    int DashDirection { get; }      // 이번 프레임 대시 명령: -1 / 0 / +1
    bool JumpPressed { get; }
    bool ShieldPressed { get; }
    bool AttackPressed(AttackSlotType slot);
}
