using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* SceneControll.cs
 * This file contain basic methods for buttons to change scenes
*/


public class SceneControll: MonoBehaviour
{
 public void OpenForm()
    {
        SceneManager.LoadScene(13);

        
    }
 public void OpenGridgenerator()
    {
        SceneManager.LoadScene(1);

        
    }

    public void openEmailSendScreen()
    {
        SceneManager.LoadScene(8);


    }



    private void Start()
    {
        Application.targetFrameRate = 300;
    }


}
