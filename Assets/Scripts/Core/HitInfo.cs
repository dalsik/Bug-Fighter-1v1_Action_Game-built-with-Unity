using System.Collections.Generic;

// 공격이 상대에게 건네는 꾸러미 — "누가, 어느 쪽으로, 어떤 효과들을"
public readonly struct HitInfo
{
    public readonly CharacterRoot Attacker;
    public readonly float Direction;                // 밀어낼 방향: +1 오른쪽, -1 왼쪽
    public readonly IReadOnlyList<IEffect> Effects;

    public HitInfo(CharacterRoot attacker, float direction, IReadOnlyList<IEffect> effects)
    {
        Attacker = attacker;
        Direction = direction;
        Effects = effects;
    }
}
