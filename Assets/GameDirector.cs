using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // �� ������ ���� ���ӽ����̽� �߰�

public class GameDirector : MonoBehaviour
{
    GameObject HP_BarPlayer1;
    GameObject HP_BarPlayer2;
    public bool isPlayer1ShieldActive = false; // �÷��̾�1 ���� ����
    public bool isPlayer2ShieldActive = false; // �÷��̾�2 ���� ����

    private bool isGamePaused = false;         // ���� ���� ���� Ȯ�ο� �÷���

    void Start()
    {
        this.HP_BarPlayer1 = GameObject.Find("HP_BarPlayer1"); // �÷��̾�1 ü�¹�
        this.HP_BarPlayer2 = GameObject.Find("HP_BarPlayer2"); // �÷��̾�2 ü�¹�
    }

    public void DecreaseHP1(float damage)
    {
        if (isPlayer1ShieldActive || isGamePaused) return; // �ǵ� Ȱ��ȭ ���� �Ǵ� ���� ���� �� ��ȿȭ

        // ü�� ����
        this.HP_BarPlayer1.GetComponent<Image>().fillAmount -= damage;

        // ü�� 0 üũ
        if (this.HP_BarPlayer1.GetComponent<Image>().fillAmount <= 0)
        {
            PauseGame(2); // 2P �¸�
        }
    }

    public void DecreaseHP2(float damage)
    {
        if (isPlayer2ShieldActive || isGamePaused) return; // �ǵ� Ȱ��ȭ ���� �Ǵ� ���� ���� �� ��ȿȭ

        // ü�� ����
        this.HP_BarPlayer2.GetComponent<Image>().fillAmount -= damage;

        // ü�� 0 üũ
        if (this.HP_BarPlayer2.GetComponent<Image>().fillAmount <= 0)
        {
            PauseGame(1); // 1P �¸�
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

        // �¸� ���ǿ� ���� �� ��ȯ
        if (winner == 1)
        {
            SceneManager.LoadScene("1PWinScene"); // 1P �¸� ������ ��ȯ
        }
        else if (winner == 2)
        {
            SceneManager.LoadScene("2PWinScene"); // 2P �¸� ������ ��ȯ
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
    public bool isPlayer1ShieldActive = false; // �÷��̾�1 ���� ����
    public bool isPlayer2ShieldActive = false; // �÷��̾�2 ���� ����

    public GameObject GameOverText;
    private bool isGamePaused = false;         // ���� ���� ���� Ȯ�ο� �÷���

    void Start()
    {
        this.HP_BarPlayer1 = GameObject.Find("HP_BarPlayer1"); // �÷��̾�1 ü�¹�
        this.HP_BarPlayer2 = GameObject.Find("HP_BarPlayer2"); // �÷��̾�2 ü�¹�
    }

    public void DecreaseHP1(float damage)
    {
        if (isPlayer1ShieldActive || isGamePaused) return; // �ǵ� Ȱ��ȭ ���� �Ǵ� ���� ���� �� ��ȿȭ

        // ü�� ����
        this.HP_BarPlayer1.GetComponent<Image>().fillAmount -= damage;

        // ü�� 0 üũ
        if (this.HP_BarPlayer1.GetComponent<Image>().fillAmount <= 0)
        {
            PauseGame(); // ���� ����
        }
    }

    public void DecreaseHP2(float damage)
    {
        if (isPlayer2ShieldActive || isGamePaused) return; // �ǵ� Ȱ��ȭ ���� �Ǵ� ���� ���� �� ��ȿȭ

        // ü�� ����
        this.HP_BarPlayer2.GetComponent<Image>().fillAmount -= damage;

        // ü�� 0 üũ
        if (this.HP_BarPlayer2.GetComponent<Image>().fillAmount <= 0)
        {
            PauseGame(); // ���� ����
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
            GameOverText.SetActive(true); // GameOverText Ȱ��ȭ
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
    public bool isPlayer1ShieldActive = false; // �÷��̾�1 ���� ����
    public bool isPlayer2ShieldActive = false; // �÷��̾�2 ���� ����

    // Start is called before the first frame update
    void Start()
    {
        this.HP_BarPlayer1 = GameObject.Find("HP_BarPlayer1");      // �÷��̾�1 ü�¹� ��ü ����
        this.HP_BarPlayer2 = GameObject.Find("HP_BarPlayer2");      // �÷��̾�2 ü�¹� ��ü ����
    }


    public void DecreaseHP1(float damage)
    {
        
        if (isPlayer1ShieldActive) return; // �ǵ尡 Ȱ��ȭ �� ��� ������ ��ȿȭ
        this.HP_BarPlayer1.GetComponent<Image>().fillAmount -= damage;
    }

    public void DecreaseHP2(float damage)
    {
        if (isPlayer2ShieldActive) return;  // �ǵ尡 Ȱ��ȭ �� ��� ������ ��ȿȭ
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
