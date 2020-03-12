using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class MenuManager : MonoBehaviour
{

    Tween squishTween;
   
    public void Squish(GameObject button)
    {
        squishTween?.Kill(true);
        squishTween = button.transform.DOPunchScale(Quaternion.AngleAxis(-45, Vector3.up) * new Vector3(-.2f, .2f, 0), .2f, 15, 1f);
    }


    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
