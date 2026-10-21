using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 한 번만 돌리는 옮기기 도구 — 옛 PlayerController(개미) · BeeController(벌)로 꾸며진 씬과 프리팹을
// 새 구조(CharacterRoot + 부품 + 공격 자식 + 효과 컴포넌트)로 바꾼다. 숫자는 옛 코드 값 그대로다.
// 첫 씬(Map1)의 개미 · 벌을 옮겨 Ant.prefab · Bee.prefab 으로 저장하고, 나머지 씬은 그 프리팹으로 바꿔 끼운다.
// 그래서 밸런스 숫자는 프리팹 하나에만 있다 (씬마다 따로 고칠 필요 없음).
// 메뉴: Bug Fighter > Migrate Scenes and Prefabs  (이미 옮긴 것은 건너뛴다)
public static class BugFighterSetup
{
    static readonly string[] ScenePaths =
    {
        "Assets/Map1.unity", "Assets/Map2.unity",
        "Assets/GameScene.unity", "Assets/week2.unity", "Assets/week3.unity",
    };

    const string CharacterFolder = "Assets/Prefab/Characters";
    const string BeePrefabPath = CharacterFolder + "/Bee.prefab";
    const string AntPrefabPath = CharacterFolder + "/Ant.prefab";
    const string InputFolder = "Assets/Settings/Input";
    const string SpitPath = "Assets/Prefab/Projectile.prefab";
    const string SwordPath = "Assets/Prefab/Sword.prefab";
    const string StingerPath = "Assets/Prefab/StingerPrefab.prefab";
    const string LaserPath = "Assets/Lazer.prefab";
    const string SlashPath = "Assets/Prefab/Ant_Ultimate_effect.prefab";
    const string ShieldPath = "Assets/Prefab/Shield.prefab";

    [MenuItem("Bug Fighter/Migrate Scenes and Prefabs")]
    public static void MigrateAll()
    {
        MigratePrefabs();

        InputProfile p1Keys = Profile($"{InputFolder}/P1_Keys.asset",
            KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S,
            new AttackKeyBinding(AttackSlotType.NormalMelee, KeyCode.J),
            new AttackKeyBinding(AttackSlotType.StrongMelee, KeyCode.U),
            new AttackKeyBinding(AttackSlotType.NormalRanged, KeyCode.K),
            new AttackKeyBinding(AttackSlotType.StrongRanged, KeyCode.I),
            new AttackKeyBinding(AttackSlotType.Ultimate, KeyCode.L));

        InputProfile p2Keys = Profile($"{InputFolder}/P2_Keys.asset",
            KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow,
            new AttackKeyBinding(AttackSlotType.NormalMelee, KeyCode.Keypad1),
            new AttackKeyBinding(AttackSlotType.StrongMelee, KeyCode.Keypad4),
            new AttackKeyBinding(AttackSlotType.NormalRanged, KeyCode.Keypad2),
            new AttackKeyBinding(AttackSlotType.StrongRanged, KeyCode.Keypad5),
            new AttackKeyBinding(AttackSlotType.Ultimate, KeyCode.Keypad3));

        foreach (string path in ScenePaths)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            MigrateScene(p1Keys, p2Keys);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[BugFighterSetup] {path} 옮김");
        }

        AssetDatabase.SaveAssets();
    }

    // ── 씬 ────────────────────────────────────────────────────────────────

    static void MigrateScene(InputProfile p1Keys, InputProfile p2Keys)
    {
        // 지금 맵에서 벌은 1P(WASD), 개미는 2P(방향키 · 키패드)다
        GameObject bee = PlaceCharacter("Bee", BeePrefabPath, go => BuildBee(go, p1Keys));
        GameObject ant = PlaceCharacter("Ant", AntPrefabPath, go => BuildAnt(go, p2Keys));

        Health beeHealth = bee != null ? bee.GetComponent<Health>() : null;
        Health antHealth = ant != null ? ant.GetComponent<Health>() : null;

        GameDirector director = Object.FindFirstObjectByType<GameDirector>();
        if (director != null)
            new Fields(director).Obj("player1", beeHealth).Obj("player2", antHealth).Apply();

        BindHealthBar("HP_BarPlayer1", beeHealth);
        BindHealthBar("HP_BarPlayer2", antHealth);
    }

    // 씬의 캐릭터를 프리팹 인스턴스로 만든다.
    //  - 프리팹이 아직 없으면: 이 씬의 캐릭터를 그 자리에서 옮기고 프리팹으로 저장 (이 씬이 원본)
    //  - 프리팹이 있으면: 옛 오브젝트를 지우고 같은 위치 · 회전 · 크기로 프리팹을 놓는다
    static GameObject PlaceCharacter(string objectName, string prefabPath, System.Action<GameObject> build)
    {
        GameObject found = GameObject.Find(objectName);
        if (found == null) return null;

        GameObject prefab = Load(prefabPath);
        if (prefab != null && PrefabUtility.GetCorrespondingObjectFromSource(found) == prefab) return found;

        if (prefab == null)
        {
            build(found);
            if (!AssetDatabase.IsValidFolder(CharacterFolder)) AssetDatabase.CreateFolder("Assets/Prefab", "Characters");
            PrefabUtility.SaveAsPrefabAssetAndConnect(found, prefabPath, InteractionMode.AutomatedAction);
            return found;
        }

        Transform old = found.transform;
        GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(prefab, found.scene);
        placed.name = objectName;
        placed.transform.SetParent(old.parent, false);
        placed.transform.SetPositionAndRotation(old.position, old.rotation);
        placed.transform.localScale = old.localScale;
        placed.transform.SetSiblingIndex(old.GetSiblingIndex());
        Object.DestroyImmediate(found);
        return placed;
    }

    static void BuildBee(GameObject bee, InputProfile keys)
    {
        if (!BuildBody(bee, Team.Player1, keys,
                walkForce: 21f,             // 옛 코드: 30 × 입력값 0.7
                spriteFacingSign: 1f,
                jumpForce: 700f, maxJumps: 3, resetFallSpeed: true)) return;

        Transform attacks = Child(bee.transform, "Attacks");

        GameObject melee = Melee(attacks, AttackSlotType.NormalMelee, "AttackTrigger", 0f,
            new Vector2(0.7f, 1f), new Vector2(0.35f, 0f), Vector2.zero, 0f);
        Damage(melee, 0.1f); Knockback(melee, 10f); Stun(melee, 0.2f);

        GameObject strongMelee = Melee(attacks, AttackSlotType.StrongMelee, "AAttackTrigger", 2f,
            new Vector2(0.7f, 1f), new Vector2(0.35f, 0f), new Vector2(15f, 0f), 0.1f);   // 대시(15) 후 0.1초 뒤 판정
        Damage(strongMelee, 0.2f); Knockback(strongMelee, 10f); Stun(strongMelee, 0.2f);

        GameObject ranged = Shooter(attacks, AttackSlotType.NormalRanged, "SpitTrigger", 0f,
            StingerPath, 10f, new Vector2(0f, -0.6f));
        Damage(ranged, 0.025f); Knockback(ranged, 5f);

        GameObject strongRanged = Shooter(attacks, AttackSlotType.StrongRanged, "SpitTrigger", 2f,
            StingerPath, 7.5f, new Vector2(-0.2f, -0.7f), new Vector2(0f, -0.6f), new Vector2(-0.1f, -0.5f));
        Damage(strongRanged, 0.025f); Knockback(strongRanged, 5f);

        GameObject laser = Attack<LaserAttack>(attacks, AttackSlotType.Ultimate, "UltiTrigger", 0f, 1);
        new Fields(laser.GetComponent<LaserAttack>())
            .Obj("laserPrefab", Load(LaserPath))
            .Vec("spawnOffset", new Vector2(10f, -0.5f))
            .Vec("laserScale", new Vector2(2f, 1f))
            .Num("chargeTime", 1f)
            .Num("fireTime", 1f)
            .Apply();
        Damage(laser, 0.2f); Knockback(laser, 10f); Stun(laser, 0.2f);
    }

    static void BuildAnt(GameObject ant, InputProfile keys)
    {
        if (!BuildBody(ant, Team.Player2, keys,
                walkForce: 27f,             // 옛 코드: 30 × 입력값 0.9
                spriteFacingSign: -1f,      // 개미 오브젝트는 Y축 180° 돌아가 있다
                jumpForce: 1650f, maxJumps: 1, resetFallSpeed: false)) return;

        Transform attacks = Child(ant.transform, "Attacks");

        GameObject melee = Melee(attacks, AttackSlotType.NormalMelee, "AttackTrigger", 0f,
            new Vector2(1f, 2f), new Vector2(0.5f, 0.5f), Vector2.zero, 0f);
        Damage(melee, 0.05f); Knockback(melee, 10f); Stun(melee, 0.2f);

        GameObject strongMelee = Melee(attacks, AttackSlotType.StrongMelee, "AttackTrigger", 2f,
            new Vector2(1f, 2f), new Vector2(0.5f, 0.5f), new Vector2(20f, 8f), 0.1f);     // 앞으로 뛰어 0.1초 뒤 판정
        Damage(strongMelee, 0.15f); Knockback(strongMelee, 10f); Stun(strongMelee, 0.2f);

        GameObject ranged = Shooter(attacks, AttackSlotType.NormalRanged, "SpitTrigger", 0f,
            SpitPath, 5f, new Vector2(0.5f, 0f));
        Damage(ranged, 0.025f); Knockback(ranged, 5f); Stun(ranged, 0.2f);

        GameObject strongRanged = Shooter(attacks, AttackSlotType.StrongRanged, "SSpitTrigger", 2f,
            SwordPath, 10f, new Vector2(0.5f, -0.5f));
        Damage(strongRanged, 0.05f); Knockback(strongRanged, 8f); Stun(strongRanged, 0.3f);

        GameObject slash = Attack<DashSlashAttack>(attacks, AttackSlotType.Ultimate, "UltiTrigger", 0f, 1);
        new Fields(slash.GetComponent<DashSlashAttack>())
            .Obj("slashEffectPrefab", Load(SlashPath))
            .Num("windup", 0.5f)
            .Num("distance", 5f)
            .Num("hitHeight", 1f)
            .Vec("effectScale", new Vector2(2.5f, 2f))
            .Num("effectDuration", 1f)
            .Apply();
        Damage(slash, 0.3f); Knockback(slash, 10f); Stun(slash, 0.2f);
    }

    // 옛 컨트롤러를 떼고 몸 부품을 붙인다. 이미 옮긴 캐릭터면 false
    static bool BuildBody(GameObject go, Team team, InputProfile keys,
        float walkForce, float spriteFacingSign, float jumpForce, int maxJumps, bool resetFallSpeed)
    {
        if (go.GetComponent<CharacterRoot>() != null)
        {
            Debug.Log($"[BugFighterSetup] {go.name} 는 이미 옮겼습니다 — 건너뜀");
            return false;
        }

        RemoveMissingScripts(go);   // 지운 옛 스크립트(컨트롤러 · PlayerHealth) 자리

        CharacterRoot root = go.AddComponent<CharacterRoot>();  // Health · StatusEffects · Mover 가 함께 붙는다
        go.AddComponent<Hurtbox>();
        Jumper jumper = go.AddComponent<Jumper>();
        Shield shield = go.AddComponent<Shield>();
        KeyboardInput input = go.AddComponent<KeyboardInput>();

        new Fields(root).Enum("team", (int)team).Apply();
        new Fields(input).Obj("keys", keys).Apply();
        new Fields(go.GetComponent<Mover>())
            .Num("walkForce", walkForce)
            .Num("maxWalkSpeed", 4f)
            .Num("dashForce", 15f)
            .Num("dashDuration", 0.1f)
            .Num("spriteFacingSign", spriteFacingSign)
            .Apply();
        new Fields(jumper)
            .Num("jumpForce", jumpForce)
            .Int("maxJumps", maxJumps)
            .Bool("resetFallSpeed", resetFallSpeed)
            .Apply();
        new Fields(shield)
            .Obj("effectPrefab", Load(ShieldPath))
            .Num("duration", 1f)
            .Num("cooldown", 5f)
            .Apply();
        return true;
    }

    static GameObject Melee(Transform parent, AttackSlotType slot, string anim, float cooldown,
        Vector2 boxSize, Vector2 boxOffset, Vector2 lunge, float hitDelay)
    {
        GameObject go = Attack<MeleeAttack>(parent, slot, anim, cooldown, 0);
        new Fields(go.GetComponent<MeleeAttack>())
            .Vec("boxSize", boxSize)
            .Vec("boxOffset", boxOffset)
            .Vec("lungeImpulse", lunge)
            .Num("hitDelay", hitDelay)
            .Apply();
        return go;
    }

    static GameObject Shooter(Transform parent, AttackSlotType slot, string anim, float cooldown,
        string prefabPath, float maxDistance, params Vector2[] spawnOffsets)
    {
        GameObject go = Attack<ProjectileAttack>(parent, slot, anim, cooldown, 0);
        new Fields(go.GetComponent<ProjectileAttack>())
            .Obj("prefab", AssetDatabase.LoadAssetAtPath<Projectile>(prefabPath))
            .Num("speed", 5f)
            .Num("maxDistance", maxDistance)
            .Vecs("spawnOffsets", spawnOffsets)
            .Apply();
        return go;
    }

    static GameObject Attack<T>(Transform parent, AttackSlotType slot, string anim, float cooldown, int maxUses)
        where T : AttackBase
    {
        GameObject go = Child(parent, slot.ToString()).gameObject;
        new Fields(go.AddComponent<T>())
            .Enum("slot", (int)slot)
            .Str("animTrigger", anim)
            .Num("cooldown", cooldown)
            .Int("maxUses", maxUses)
            .Apply();
        return go;
    }

    static void Damage(GameObject attack, float amount) =>
        new Fields(attack.AddComponent<DamageEffect>()).Num("amount", amount).Apply();

    static void Knockback(GameObject attack, float force) =>
        new Fields(attack.AddComponent<KnockbackEffect>()).Num("force", force).Num("lift", 0.5f).Apply();

    static void Stun(GameObject attack, float duration) =>
        new Fields(attack.AddComponent<StunEffect>()).Num("duration", duration).Apply();

    static void BindHealthBar(string objectName, Health target)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null || target == null || go.GetComponent<Image>() == null) return;

        HealthBarView view = go.GetComponent<HealthBarView>();
        if (view == null) view = go.AddComponent<HealthBarView>();
        new Fields(view).Obj("target", target).Apply();
    }

    // ── 프리팹 ────────────────────────────────────────────────────────────

    static void MigratePrefabs()
    {
        // 옛 SpitController · SSpitController · StingerController · LaserController 를 떼고,
        // 발사체에는 숫자 없는 Projectile 만 붙인다 (피해 · 넉백은 쏜 공격 쪽 효과가 정한다)
        MigrateProjectile(SpitPath, 0.3f, spriteFacesLeft: true);
        MigrateProjectile(SwordPath, 1f, spriteFacesLeft: false);
        MigrateProjectile(StingerPath, 0.3f, spriteFacesLeft: false);

        GameObject laser = PrefabUtility.LoadPrefabContents(LaserPath);
        RemoveMissingScripts(laser);
        PrefabUtility.SaveAsPrefabAsset(laser, LaserPath);
        PrefabUtility.UnloadPrefabContents(laser);
    }

    static void MigrateProjectile(string path, float visualScale, bool spriteFacesLeft)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        RemoveMissingScripts(root);

        Projectile projectile = root.GetComponent<Projectile>();
        if (projectile == null) projectile = root.AddComponent<Projectile>();
        new Fields(projectile).Num("visualScale", visualScale).Bool("spriteFacesLeft", spriteFacesLeft).Apply();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    // ── 도우미 ────────────────────────────────────────────────────────────

    static InputProfile Profile(string path, KeyCode left, KeyCode right, KeyCode jump, KeyCode shield,
        params AttackKeyBinding[] attacks)
    {
        InputProfile existing = AssetDatabase.LoadAssetAtPath<InputProfile>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
        if (!AssetDatabase.IsValidFolder(InputFolder)) AssetDatabase.CreateFolder("Assets/Settings", "Input");

        InputProfile profile = ScriptableObject.CreateInstance<InputProfile>();
        profile.left = left;
        profile.right = right;
        profile.jump = jump;
        profile.shield = shield;
        profile.doubleTapWindow = 0.3f;
        profile.attacks = attacks;
        AssetDatabase.CreateAsset(profile, path);
        return profile;
    }

    static Transform Child(Transform parent, string name)
    {
        Transform child = new GameObject(name).transform;
        child.SetParent(parent, false);
        return child;
    }

    static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path);

    static void RemoveMissingScripts(GameObject root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
    }

    // private [SerializeField] 를 이름으로 채운다 — 이름이 틀리면 바로 예외 (조용히 기본값으로 남지 않게)
    sealed class Fields
    {
        readonly SerializedObject so;

        public Fields(Object target) => so = new SerializedObject(target);

        public Fields Num(string name, float value) { Prop(name).floatValue = value; return this; }
        public Fields Int(string name, int value) { Prop(name).intValue = value; return this; }
        public Fields Bool(string name, bool value) { Prop(name).boolValue = value; return this; }
        public Fields Str(string name, string value) { Prop(name).stringValue = value; return this; }
        public Fields Enum(string name, int index) { Prop(name).enumValueIndex = index; return this; }
        public Fields Vec(string name, Vector2 value) { Prop(name).vector2Value = value; return this; }
        public Fields Obj(string name, Object value) { Prop(name).objectReferenceValue = value; return this; }

        public Fields Vecs(string name, Vector2[] values)
        {
            SerializedProperty array = Prop(name);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).vector2Value = values[i];
            return this;
        }

        public void Apply() => so.ApplyModifiedPropertiesWithoutUndo();

        SerializedProperty Prop(string name)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property == null)
                throw new System.ArgumentException($"{so.targetObject.GetType().Name} 에 '{name}' 필드가 없습니다");
            return property;
        }
    }
}
