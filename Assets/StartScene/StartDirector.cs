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
            int randomValue = Random.Range(0, 2); // 0 또는 1의 난수 생성
            if (randomValue == 0)
            {
                SceneManager.LoadScene("Map1"); // week5 씬 로드
            }
            else if (randomValue == 1)
            {
                SceneManager.LoadScene("Map2"); // Map2 씬 로드
            }
        }
    }
}