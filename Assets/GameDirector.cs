using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 관리를 위한 네임스페이스 추가

public class GameDirector : MonoBehaviour
{
    GameObject HP_BarPlayer1;
    GameObject HP_BarPlayer2;
    public bool isPlayer1ShieldActive = false; // 플레이어1 쉴드 상태
    public bool isPlayer2ShieldActive = false; // 플레이어2 쉴드 상태

    private bool isGamePaused = false;         // 게임 정지 상태 확인용 플래그

    void Start()
    {
        this.HP_BarPlayer1 = GameObject.Find("HP_BarPlayer1"); // 플레이어1 체력바
        this.HP_BarPlayer2 = GameObject.Find("HP_BarPlayer2"); // 플레이어2 체력바
    }

    public void DecreaseHP1(float damage)
    {
        if (isPlayer1ShieldActive || isGamePaused) return; // 실드 활성화 상태 또는 게임 정지 시 무효화

        // 체력 감소
        this.HP_BarPlayer1.GetComponent<Image>().fillAmount -= damage;

        // 체력 0 체크
        if (this.HP_BarPlayer1.GetComponent<Image>().fillAmount <= 0)
        {
            PauseGame(2); // 2P 승리
        }
    }

    public void DecreaseHP2(float damage)
    {
        if (isPlayer2ShieldActive || isGamePaused) return; // 실드 활성화 상태 또는 게임 정지 시 무효화

        // 체력 감소
        this.HP_BarPlayer2.GetComponent<Image>().fillAmount -= damage;

        // 체력 0 체크
        if (this.HP_BarPlayer2.GetComponent<Image>().fillAmount <= 0)
        {
            PauseGame(1); // 1P 승리
        }
    }

    public void IncreaseHP1(float damage)
    {
        this.HP_BarPlayer1.GetComponent<Image>().fillAmount += damage;
    }

    public void IncreaseHP2(float damage)
    {
        this.HP_BarPlayer2.GetComponent<Image>().fillAmount += damage;
    }

    private void PauseGame(int winner)
    {
        isGamePaused = true;
        Time.timeScale = 0;

        // 승리 조건에 따라 씬 전환
        if (winner == 1)
        {
            SceneManager.LoadScene("1PWinScene"); // 1P 승리 씬으로 전환
        }
        else if (winner == 2)
        {
            SceneManager.LoadScene("2PWinScene"); // 2P 승리 씬으로 전환
        }
    }
}

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    GameObject HP_BarPlayer1;
    GameObject HP_BarPlayer2;
    public bool isPlayer1ShieldActive = false; // 플레이어1 쉴드 상태
    public bool isPlayer2ShieldActive = false; // 플레이어2 쉴드 상태

    public GameObject GameOverText;
    private bool isGamePaused = false;         // 게임 정지 상태 확인용 플래그

    void Start()
    {
        this.HP_BarPlayer1 = GameObject.Find("HP_BarPlayer1"); // 플레이어1 체력바
        this.HP_BarPlayer2 = GameObject.Find("HP_BarPlayer2"); // 플레이어2 체력바
    }

    public void DecreaseHP1(float damage)
    {
        if (isPlayer1ShieldActive || isGamePaused) return; // 실드 활성화 상태 또는 게임 정지 시 무효화

        // 체력 감소
        this.HP_BarPlayer1.GetComponent<Image>().fillAmount -= damage;

        // 체력 0 체크
        if (this.HP_BarPlayer1.GetComponent<Image>().fillAmount <= 0)
        {
            PauseGame(); // 게임 정지
        }
    }

    public void DecreaseHP2(float damage)
    {
        if (isPlayer2ShieldActive || isGamePaused) return; // 실드 활성화 상태 또는 게임 정지 시 무효화

        // 체력 감소
        this.HP_BarPlayer2.GetComponent<Image>().fillAmount -= damage;

        // 체력 0 체크
        if (this.HP_BarPlayer2.GetComponent<Image>().fillAmount <= 0)
        {
            PauseGame(); // 게임 정지
        }
    }

    public void IncreaseHP1(float damage)
    {
        this.HP_BarPlayer1.GetComponent<Image>().fillAmount += damage;
    }

    public void IncreaseHP2(float damage)
    {
        this.HP_BarPlayer2.GetComponent<Image>().fillAmount += damage;
    }

    private void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0;

        if (GameOverText != null)
        {
            GameOverText.SetActive(true); // GameOverText 활성화
        }
    }
}*/

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{

    GameObject HP_BarPlayer1;
    GameObject HP_BarPlayer2;
    public bool isPlayer1ShieldActive = false; // 플레이어1 쉴드 상태
    public bool isPlayer2ShieldActive = false; // 플레이어2 쉴드 상태

    // Start is called before the first frame update
    void Start()
    {
        this.HP_BarPlayer1 = GameObject.Find("HP_BarPlayer1");      // 플레이어1 체력바 객체 설정
        this.HP_BarPlayer2 = GameObject.Find("HP_BarPlayer2");      // 플레이어2 체력바 객체 설정
    }


    public void DecreaseHP1(float damage)
    {
        
        if (isPlayer1ShieldActive) return; // 실드가 활성화 된 경우 데미지 무효화
        this.HP_BarPlayer1.GetComponent<Image>().fillAmount -= damage;
    }

    public void DecreaseHP2(float damage)
    {
        if (isPlayer2ShieldActive) return;  // 실드가 활성화 된 경우 데미지 무효화
        this.HP_BarPlayer2.GetComponent<Image>().fillAmount -= damage;
    }

    public void IncreaseHP1(float damage)
    {
        this.HP_BarPlayer1.GetComponent<Image>().fillAmount += damage;
    }

    public void IncreaseHP2(float damage)
    {
        this.HP_BarPlayer2.GetComponent<Image>().fillAmount += damage;
    }

}*/
