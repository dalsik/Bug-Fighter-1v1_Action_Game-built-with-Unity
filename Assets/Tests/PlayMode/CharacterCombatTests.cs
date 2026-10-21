using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

// 실제 Map1 씬(옮기기 도구로 바꾼 상태)에서 벌(1P) · 개미(2P)가 옛 수치대로 싸우는지 확인한다.
// 키보드 대신 ScriptedInput 을 SetInput 으로 꽂아 조종한다.
public class CharacterCombatTests
{
    const float Tolerance = 0.0001f;

    CharacterRoot bee;
    CharacterRoot ant;
    ScriptedInput beeInput;
    ScriptedInput antInput;

    [UnitySetUp]
    public IEnumerator LoadMap()
    {
        SceneManager.LoadScene("Map1");
        yield return null;
        yield return null;

        // 젤리가 떨어져 체력을 채우면 숫자 확인이 흔들리므로 끈다
        GameObject generator = GameObject.Find("jellyGenerator");
        if (generator != null) generator.SetActive(false);
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (go.name.StartsWith("jelly") && go.name.EndsWith("(Clone)")) Object.Destroy(go);

        bee = GameObject.Find("Bee").GetComponent<CharacterRoot>();
        ant = GameObject.Find("Ant").GetComponent<CharacterRoot>();
        beeInput = new ScriptedInput();
        antInput = new ScriptedInput();
        bee.SetInput(beeInput);
        ant.SetInput(antInput);

        yield return new WaitForSeconds(0.5f);     // 바닥에 내려앉을 때까지
    }

    [Test]
    public void Migration_GivesBothCharactersAllFiveAttackSlots()
    {
        foreach (AttackSlotType slot in Enum.GetValues(typeof(AttackSlotType)))
        {
            Assert.IsTrue(bee.HasAttack(slot), $"벌에 {slot} 이 없음");
            Assert.IsTrue(ant.HasAttack(slot), $"개미에 {slot} 이 없음");
        }
        Assert.AreEqual(Team.Player1, bee.Team);
        Assert.AreEqual(Team.Player2, ant.Team);
        Assert.AreEqual(1f, bee.Facing, "벌은 오른쪽을 보고 시작");
        Assert.AreEqual(-1f, ant.Facing, "개미는 왼쪽을 보고 시작");
    }

    [UnityTest]
    public IEnumerator BeeMelee_DamagesKnocksBackAndStunsAnt()
    {
        yield return PlaceInFront(bee, ant, 0.2f);
        float before = ant.Health.Current;

        yield return Press(beeInput, i => i.Attack = AttackSlotType.NormalMelee);

        Assert.AreEqual(before - 0.1f, ant.Health.Current, Tolerance, "벌 근접 피해 0.1");
        Assert.IsTrue(ant.Status.IsStunned, "맞은 개미는 경직");
        yield return new WaitForFixedUpdate();
        Assert.Greater(ant.Body.velocity.x, 0f, "오른쪽(벌이 보는 쪽)으로 밀림");

        yield return new WaitForSeconds(0.3f);
        Assert.IsTrue(ant.Status.CanAct, "경직 0.2초 뒤에는 풀린다 — 발사체가 사라져도 맞은 쪽이 시간을 잰다");
    }

    [UnityTest]
    public IEnumerator Shield_BlocksDamageKnockbackAndStun()
    {
        yield return PlaceInFront(bee, ant, 0.2f);
        float before = ant.Health.Current;

        yield return Press(antInput, i => i.ShieldPressed = true);
        Assert.IsTrue(ant.GetComponent<Shield>().IsActive);

        yield return Press(beeInput, i => i.Attack = AttackSlotType.NormalMelee);

        Assert.AreEqual(before, ant.Health.Current, Tolerance);
        Assert.IsFalse(ant.Status.IsStunned);
    }

    [UnityTest]
    public IEnumerator AntSpit_FliesAndHitsBee_ThenDisappears()
    {
        yield return PlaceInFront(ant, bee, 2f);
        float before = bee.Health.Current;

        yield return Press(antInput, i => i.Attack = AttackSlotType.NormalRanged);
        yield return new WaitForSeconds(1f);

        Assert.AreEqual(before - 0.025f, bee.Health.Current, Tolerance, "개미 침 피해 0.025");
        Assert.AreEqual(0, Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length, "맞힌 발사체는 사라진다");
    }

    [UnityTest]
    public IEnumerator BeeStrongRanged_FiresThreeStingers_WithCooldown()
    {
        yield return Press(beeInput, i => i.Attack = AttackSlotType.StrongRanged);
        Assert.AreEqual(3, Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length);

        yield return new WaitForSeconds(2.2f);     // 사거리 7.5 를 다 날아가 사라진다
        Assert.AreEqual(0, Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length);
    }

    [UnityTest]
    public IEnumerator BeeJumps_ThreeTimes_AndGetsThemBackAfterLanding()
    {
        Jumper jumper = bee.GetComponent<Jumper>();
        Assert.AreEqual(3, jumper.JumpsLeft);

        // 한 번만 뛰고 내려와도 다시 3번 — 옛 코드는 3번 다 써야만 되돌아왔다
        yield return Press(beeInput, i => i.JumpPressed = true);
        Assert.AreEqual(2, jumper.JumpsLeft);
        yield return WaitUntilLanded(jumper);
        Assert.AreEqual(3, jumper.JumpsLeft);

        for (int n = 0; n < 3; n++)
        {
            yield return Press(beeInput, i => i.JumpPressed = true);
            yield return new WaitForSeconds(0.15f);
        }
        Assert.AreEqual(0, jumper.JumpsLeft);

        yield return Press(beeInput, i => i.JumpPressed = true);
        Assert.AreEqual(0, jumper.JumpsLeft, "네 번째는 안 뛴다");

        yield return WaitUntilLanded(jumper);
        Assert.AreEqual(3, jumper.JumpsLeft);
    }

    [UnityTest]
    public IEnumerator AntJumps_OnlyOnce()
    {
        Jumper jumper = ant.GetComponent<Jumper>();
        Assert.AreEqual(1, jumper.JumpsLeft);

        yield return Press(antInput, i => i.JumpPressed = true);
        yield return new WaitForSeconds(0.15f);
        Assert.AreEqual(0, jumper.JumpsLeft);
        Assert.IsFalse(jumper.TryJump(), "공중에서는 못 뛴다");

        yield return WaitUntilLanded(jumper);
        Assert.AreEqual(1, jumper.JumpsLeft);
    }

    [UnityTest]
    public IEnumerator AntUltimate_TeleportsThroughBee_AndWorksOncePerMatch()
    {
        yield return PlaceInFront(ant, bee, 2f);
        float before = bee.Health.Current;
        float startX = ant.Position.x;

        yield return Press(antInput, i => i.Attack = AttackSlotType.Ultimate);
        yield return new WaitForSeconds(0.7f);     // 준비 0.5초 뒤 순간이동

        Assert.AreEqual(startX - 5f, ant.transform.position.x, 0.3f, "왼쪽으로 5만큼 순간이동");
        Assert.AreEqual(before - 0.3f, bee.Health.Current, Tolerance, "필살기 피해 0.3");

        IAttack ultimate = FindAttack(ant, AttackSlotType.Ultimate);
        Assert.IsFalse(ultimate.TryUse(ant), "판당 한 번");
    }

    [UnityTest]
    public IEnumerator BeeLaser_PinsBee_AndOnlyHitsAfterCharging()
    {
        yield return PlaceInFront(bee, ant, 3f);
        float before = ant.Health.Current;
        Vector2 pinned = bee.Position;

        yield return Press(beeInput, i => i.Attack = AttackSlotType.Ultimate);
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(before, ant.Health.Current, Tolerance, "충전 중(1초)에는 안 맞는다");
        Assert.IsFalse(bee.Status.CanAct, "쏘는 동안 벌은 조작 불가");
        Assert.AreEqual(pinned.x, bee.Position.x, 0.01f, "쏘는 동안 제자리 고정");

        yield return new WaitForSeconds(1.0f);
        Assert.AreEqual(before - 0.2f, ant.Health.Current, Tolerance, "충전 뒤 레이저 피해 0.2");

        yield return new WaitForSeconds(0.8f);
        Assert.AreEqual(before - 0.2f, ant.Health.Current, Tolerance, "한 번만 맞는다");
        Assert.IsTrue(bee.Status.CanAct, "2초 뒤 다시 움직인다");
    }

    [Test]
    public void Health_DiesOnce_AndIgnoresDamageAfterwards()
    {
        Health health = new GameObject("health-test").AddComponent<Health>();
        int died = 0;
        health.Died += _ => died++;

        health.TakeDamage(0.6f);
        health.Heal(0.2f);
        Assert.AreEqual(0.6f, health.Current, Tolerance);

        health.TakeDamage(1f);
        health.TakeDamage(1f);
        health.Heal(0.5f);
        Assert.AreEqual(0f, health.Current);
        Assert.AreEqual(1, died);

        Object.Destroy(health.gameObject);
    }

    // ── 도우미 ────────────────────────────────────────────────────────────

    // 정확히 한 프레임만 누른다 (키보드의 GetKeyDown 과 같게).
    // 먼저 Update 직후 시점에 맞춘다 — WaitForFixedUpdate 바로 뒤에 누르면 Update 를 두 번 지나서야 떼어진다.
    static IEnumerator Press(ScriptedInput input, Action<ScriptedInput> press)
    {
        yield return null;
        press(input);
        yield return null;      // 다음 프레임 Update 에서 CharacterRoot 가 한 번 읽는다
        input.Clear();
    }

    // target 몸의 가까운 쪽 끝이 attacker 앞 gap 만큼에 오도록 옮긴다
    static IEnumerator PlaceInFront(CharacterRoot attacker, CharacterRoot target, float gap)
    {
        Bounds bounds = target.GetComponent<Collider2D>().bounds;
        float nearEdge = attacker.Facing > 0f ? bounds.min.x : bounds.max.x;
        float shift = attacker.Position.x + attacker.Facing * gap - nearEdge;

        target.transform.position += new Vector3(shift, 0f, 0f);
        target.Body.velocity = Vector2.zero;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
    }

    static IEnumerator WaitUntilLanded(Jumper jumper)
    {
        yield return new WaitForSeconds(0.2f);
        float giveUp = Time.time + 5f;
        while (!jumper.IsGrounded && Time.time < giveUp) yield return null;
        yield return new WaitForSeconds(0.15f);
    }

    static IAttack FindAttack(CharacterRoot root, AttackSlotType slot)
    {
        foreach (IAttack attack in root.GetComponentsInChildren<IAttack>())
            if (attack.Slot == slot) return attack;
        return null;
    }
}
