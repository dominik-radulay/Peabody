using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveData : MonoBehaviour
{


    public void SaveExperience(int NextLevel)
    {

        PlayerPrefs.SetInt("LastLevel", NextLevel);

        SceneManager.LoadScene(NextLevel);
    }
}
