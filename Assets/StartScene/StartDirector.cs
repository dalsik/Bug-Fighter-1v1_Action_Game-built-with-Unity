using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartDirector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int randomValue = Random.Range(0, 2); // 0 �Ǵ� 1�� ���� ����
            if (randomValue == 0)
            {
                SceneManager.LoadScene("Map1"); // week5 �� �ε�
            }
            else if (randomValue == 1)
            {
                SceneManager.LoadScene("Map2"); // Map2 �� �ε�
            }
        }
    }
}