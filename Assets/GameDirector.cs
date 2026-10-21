using UnityEngine;
using UnityEngine.SceneManagement;

// 판 진행 — 두 캐릭터의 Died 를 구독해서 승패 씬으로 넘긴다.
// 체력 계산 · 쉴드 · HP바 그리기는 모른다 (Health · Hurtbox · HealthBarView 몫).
public class GameDirector : MonoBehaviour
{
    [SerializeField] Health player1;
    [SerializeField] Health player2;
    [SerializeField] string player1WinScene = "1PWinScene";
    [SerializeField] string player2WinScene = "2PWinScene";

    bool finished;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void OnEnable()
    {
        if (player1 != null) player1.Died += OnDied;
        if (player2 != null) player2.Died += OnDied;
    }

    void OnDisable()
    {
        if (player1 != null) player1.Died -= OnDied;
        if (player2 != null) player2.Died -= OnDied;
    }

    void OnDied(Health dead)
    {
        if (finished) return;

        finished = true;
        SceneManager.LoadScene(dead == player1 ? player2WinScene : player1WinScene);
    }
}
