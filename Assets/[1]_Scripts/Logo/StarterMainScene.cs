using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrakeBricks
{
    public class StarterMainScene : MonoBehaviour
    {
        void Start()
        {
            //переходим к следующей сцене через 2 сек.
            StartCoroutine( StartMainScene(2f) );
        }
        

        //загружает следующую сцену
        IEnumerator StartMainScene(float time)
        {
            yield return new WaitForSeconds(time);

            var nextScene = SceneManager.GetActiveScene().buildIndex +1;
            SceneManager.LoadScene(nextScene);
        }

    }
}