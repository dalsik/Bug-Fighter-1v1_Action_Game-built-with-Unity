// 공격이라면 "어느 자리인지"와 "써 봐"가 있다 — CharacterRoot 는 공격에 대해 이것만 안다
public interface IAttack
{
    AttackSlotType Slot { get; }
    bool TryUse(CharacterRoot owner);
}
