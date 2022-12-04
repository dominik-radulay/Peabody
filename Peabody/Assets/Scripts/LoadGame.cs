using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        // if player played the game before, he can continue the game and continue button is unlocked
        if (PlayerPrefs.GetInt("LastLevel") > 0)
        {
            gameObject.GetComponent<Button>().interactable = true;


        }
        else {
            
        gameObject.GetComponent<Button>().interactable = false; 
        
        }
        
    }

    // Update is called once per frame


    public static void Load()
    {
        SceneManager.LoadScene(PlayerPrefs.GetInt("LastLevel"));
    }
}
