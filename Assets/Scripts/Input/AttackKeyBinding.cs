using System;
using UnityEngine;

// 키 하나 → 공격 자리 하나
[Serializable]
public struct AttackKeyBinding
{
    public AttackSlotType slot;
    public KeyCode key;

    public AttackKeyBinding(AttackSlotType slot, KeyCode key)
    {
        this.slot = slot;
        this.key = key;
    }
}
