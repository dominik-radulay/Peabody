using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ColliderInfoCollection.cs
 * This file contain pre defined methods "OnTriggerEnter2D", "OnTriggerStay2D" and "OnTriggerExit2D" triggered by collision boxes on Peabody and Diamonds
 * Data about collisions are saved depending on specified criterias and then used by other classes
 * 
 * 
 * 
*/


public class ColliderInfoCollection : MonoBehaviour
{
    public static string MyNameL = null;
    public static string HisNameL = null;
    public static string MyNameR = null;
    public static string HisNameR = null;
    public static float offset = 0f;
    public static bool blocker = false;
    public static float location = 0f;
    public static string TouchedDiamond = null;
    public static float DiamondLocation = 0f;




    public void Update()
    {
        List<Collider2D> results = new List<Collider2D>();
        if (Physics2D.OverlapCollider(gameObject.GetComponent<Collider2D>(), new ContactFilter2D().NoFilter(), results) > 0)
        {
            PeabodyColision(results[0], gameObject);
        }
        else if (Physics2D.OverlapCollider(gameObject.GetComponent<Collider2D>(), new ContactFilter2D().NoFilter(), results) == 0)
        {
            if (Input.touchCount > 0)
            {
                // if swipe to the left continues and Peabody appear on right on the screen, he will keep the information about block that was pushing him, if not, erase the information
                if (gameObject.name == "Right" && Swipingmechanism.offset < 0)
                {
                    PeabodyColisionDelete(gameObject);
                }
                // if swipe to the right continues and Peabody appear on left on the screen, he will keep the information about block that was pushing him, if not, erase the information
                else if (gameObject.name == "Left" && Swipingmechanism.offset > 0)
                {
                    PeabodyColisionDelete(gameObject);
                }
            }
            // erase informations about touching objects if finger doesnt touch the screen
            else
            {
            PeabodyColisionDelete(gameObject);

            }

            

        }
     }

    private void OnTriggerStay2D(Collider2D other)
    {
        PeabodyColision( other, gameObject);

        // If Diamond was touched by peabody, diamond is collected
        if ((gameObject.name == "Collider" && offset != 0) && ((other.gameObject.name == "Right") || (other.gameObject.name == "Left")))
        {
            DiamondCollection();

        }


    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        PeabodyColision(other, gameObject);


        //Situations for Diamond Colliders
        if (gameObject.name == "Collider")
        {
            // if the other object is Peabody, diamond is collected
            if ((other.gameObject.name == "Right") || (other.gameObject.name == "Left"))
            {
                    DiamondCollection();
            }
            // Otherwise, block swiping
            else
            {
                GameObject.Find("/SceneManager/Block").GetComponent<AudioSource>().Play(0);
                offset = Swipingmechanism.offset;
                blocker = true;
                location = Input.GetTouch(0).position.x;
                TouchedDiamond = other.gameObject.name;
                DiamondLocation = gameObject.transform.position.x;



            }
            
        }




    }

    void OnTriggerExit2D(Collider2D other)
    {

        //When there is no object touching the Diamond anymore, swiping is allowed again
        if (gameObject.name == "Collider")
        {
            blocker = false;
        }

    }   

    void DiamondCollection()
    {

        //Collect diamond
        transform.parent.gameObject.SetActive(false);
        //Check how many diamonds are still in the game via LoadData method
        BlockMechanics.LoadData();
        //Show new informations in the GUI
        BlockMechanics.CollectDiamonds();
        // If this was a last diamond, finish the game
        BlockMechanics.CheckIfTaskFinished();

    }

    void PeabodyColisionDelete( GameObject gameObject)
    {
        //Erase informations about block that touched Peabody from the right side but now left
        if ((gameObject.name == "Right"))
        {
            MyNameR = null;
            HisNameR = null;

        }
        //Erase informations about block that touched Peabody from the left side but now left
        if ((gameObject.name == "Left"))
        {
            MyNameL = null;
            HisNameL = null;

        }

    }


    void PeabodyColision(Collider2D other, GameObject gameObject)
    {

        if (gameObject.name == "Right" && other.gameObject.name != "Collider" && other.gameObject.tag != "Teleport")
        {
            //if Peabody touch spike because of player swipe, he loose a life
            if (other.gameObject.tag == "Spike")
            {
                BlockMechanics.TakeLife();
            }
            // otherwise, save informations about boxes that touched peabody
            else
            {
                MyNameR = gameObject.name;
                HisNameR = other.gameObject.name;

            }
        }
        if (gameObject.name == "Left" && other.gameObject.name != "Collider" && other.gameObject.tag != "Teleport")
        {
            //if Peabody touch spike because of player swipe, he loose a life
            if (other.gameObject.tag == "Spike")
            {
                BlockMechanics.TakeLife();
            }
            // otherwise, save informations about boxes that touched peabody
            else
            {
                MyNameL = gameObject.name;
                HisNameL = other.gameObject.name;
            }
        }
    }
}