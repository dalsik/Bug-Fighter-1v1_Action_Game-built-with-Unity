// 효과 하나 = 맞은 캐릭터에게 일어나는 일 하나 (피해 · 넉백 · 경직 …)
// 새 효과(독 · 둔화 · 흡혈)는 이 약속을 지키는 클래스를 하나 더 만들어 공격 오브젝트에 붙이면 된다.
public interface IEffect
{
    void Apply(HitInfo hit, CharacterRoot target);
}
