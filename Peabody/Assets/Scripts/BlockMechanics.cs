using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/* BlockMechanics.cs
 * This script sets out mechanics of each game block
 * check if all diamonds were collected and display appropriate menu
*/


public class BlockMechanics : MonoBehaviour
{
    public static bool Swiped = false;

    public static bool TaskFinished = false;
    public Vector3 newpos;


    private static int counter = 0;


    static GameObject PopUpPanel = null;
    static GameObject HeadText = null;
    static GameObject LowerText = null;

    public static GameObject[] Empties = null;
    public static GameObject[] Cracked = null;
    public static GameObject[] Diamonds = null;
    public static GameObject[] DiamondPoints = null;
    public static GameObject[] LifePoints = null;
    public static GameObject[] Peabody = null;
    public static GameObject[] RampsL = null;
    public static GameObject[] RampsR = null;
    public static GameObject[] Spikes = null;
    public static GameObject[] Spawn = null;
    public static GameObject[] Teleports = null;
    public static int PeabodyLocationCol;
    public static int PeabodyLocationRow;
    private static GameObject UnderPeabody;
    public static GameObject explosion;
    public static GameObject explosion2;



    // Start is called before the first frame update
    void Start()
    {

        PopUpPanel = GameObject.Find("/Canvas/Panel/PopUp");
        HeadText = GameObject.Find("/Canvas/Panel/LevelPanel/HeadText");
        LowerText = GameObject.Find("/Canvas/Panel/LevelPanel/Info");

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        //run check ony after each swipe to lower computing demands
        if (Swiped == true && (Input.touchCount == 0 || Swipingmechanism.row != PeabodyLocationRow || Swipingmechanism.Peabodyfall))
        { 
            

            //Check if there are still diamonds to collect
            CheckIfTaskFinished();

            //If value that usually contains new position for Peabody isnt same as current peabody location or isnt equal zero, proceed
            if ((newpos != Peabody[0].transform.position) && ((newpos.y != 0) || (newpos.x != 0)))
            {
                //If new position is lower than position of Peabody
                if (newpos.y != Peabody[0].transform.position.y)
                {
                    Peabody[0].transform.position = new Vector2(Peabody[0].transform.position.x, Peabody[0].transform.position.y - 1500 * Time.deltaTime);
                    if (newpos.y > Peabody[0].transform.position.y)
                    {
                        Peabody[0].transform.position = new Vector2(Peabody[0].transform.position.x, newpos.y);
                    }

                }
                //If new position is on the right from Peabody current location
                if (newpos.x > Peabody[0].transform.position.x)
                {
                    Peabody[0].transform.position = new Vector2(Peabody[0].transform.position.x + 1500 * Time.deltaTime, Peabody[0].transform.position.y);
                    if (newpos.x < Peabody[0].transform.position.x)
                    {
                        Peabody[0].transform.position = new Vector2(newpos.x, Peabody[0].transform.position.y);
                    }
                }
                //If new position is on the left from Peabody current location
                else if (newpos.x < Peabody[0].transform.position.x)
                {
                    Peabody[0].transform.position = new Vector2(Peabody[0].transform.position.x - 1500 * Time.deltaTime, Peabody[0].transform.position.y);
                    if (newpos.x > Peabody[0].transform.position.x)
                    {
                        Peabody[0].transform.position = new Vector2(newpos.x, Peabody[0].transform.position.y);
                    }
                }
            }
            //Else if position was changed during current swipe, finish this method and wait for new swipe
            else if (counter > 2)
            {

                counter = 0;
                Swiped = false;


            }
            //Else check if there isnt some interactive block under Peabody
            else
            {
                newpos = CheckIfPeabodyFly();

            }
        }
        //If all diamonds were collected, show end game panel
        if (TaskFinished == true)
        {
            GameObject.Find("/SceneManager/Win").GetComponent<AudioSource>().Play(0);
            GameObject.Find("/Canvas/Panel/").GetComponent<Image>().color = new Color32(0, 150, 0, 255);
            TaskFinished = false;
            GameObject.Find("/Canvas/Panel/LevelPanel").SetActive(true);
            GameObject.Find("/Canvas/Panel/UserReportingPrefab/UserReportButton").SetActive(true);
            Destroy(GameObject.Find("/Canvas/Panel/GameObject"));
            Swipingmechanism.offset = 0;
            enabled = false;

        }
    }

    //if there are no diamonds to collect, finish the level
    public static void CheckIfTaskFinished ()
    {
        
        Diamonds = GameObject.FindGameObjectsWithTag("Diamond");

        //If there are no diamonds left, declare that level is finished
        if (Diamonds.Length == 0)
        {
            TaskFinished = true;
        }
        //Else keep declaring level as not finished
        else
        {
            TaskFinished = false;

        }


    }



   // Check if there is an empty block under peabody - this method works only when player isnt swiping
    public Vector3 CheckIfPeabodyFly()
    {
        LoadData();
        //If peabody is on the last row, load block in the row 0
        if (PeabodyLocationRow == 9)
        {
            PeabodyLocationRow = -1;
            UnderPeabody = GameObject.Find("/Canvas/Panel/GameObject/" + 0 + "&" + PeabodyLocationCol);
        }
        //else load row that is right under peabody
        else
        {
            UnderPeabody = GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow + 1) + "&" + PeabodyLocationCol);

        }
        //If there are teleports in the level
        if (Teleports.Length > 1)
        {//if second teleport is at the same location as Peabody, teleport Peabody
            if (GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow) + "&" + (PeabodyLocationCol)) == Teleports[1])
            {
                teleportPeabody();
                PeabodyLocationRow = Int32.Parse(Teleports[0].name.Split('&')[0]);
                PeabodyLocationCol = Int32.Parse(Teleports[0].name.Split('&')[1]);
                return new Vector3(0, 0, 0);
            }
        }
        //Switch choosing action based on what interactive block is under Peabody
            switch (UnderPeabody.tag)
        {
            //If there is an empty block, move Peabody on its position
            case "EmptyTile":
                GameObject.Find("/SceneManager/Fall").GetComponent<AudioSource>().Play(0);
                PeabodyLocationRow = PeabodyLocationRow + 1;
                return UnderPeabody.transform.position;

                //If there is an Cracked block, destroy the block and move peabody on its position
            case "Cracked":
                //Find cracked block and transfer it to Empty block
                GameObject.Find("/SceneManager/Crack").GetComponent<AudioSource>().Play(0);
                UnderPeabody.GetComponent<Image>().color = new Color32(0, 0, 0, 0);
                UnderPeabody.tag = "EmptyTile";
                //Destroy collider that belonged to the block - empty spaces have no colliders
                Destroy(UnderPeabody.GetComponent<BoxCollider2D>());
                //Let peabody fall into new created empty space
                PeabodyLocationRow = PeabodyLocationRow + 1;
                return UnderPeabody.transform.position;

                //If there is an ramp with slide to the left side, check if there is an empty space on the left from the ramp, if yes, let peabody slide
            case "RampL":
                
                //Standart case with block space on the left side of the ramp
                if (PeabodyLocationCol>0)
                {
                    //If there is an empty space, let peabody slide
                    if (GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow + 1) + "&" + (PeabodyLocationCol - 1)).tag == "EmptyTile")
                    {
                        GameObject.Find("/SceneManager/Slide").GetComponent<AudioSource>().Play(0);
                        PeabodyLocationCol = PeabodyLocationCol - 1;
                        PeabodyLocationRow = PeabodyLocationRow + 1;
                        return GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow) + "&" + (PeabodyLocationCol)).transform.position;
                    }
                    //If there isnt, do nothing
                    else
                    {
                        return new Vector3(0, 0, 0);
                    }

                }
                //Case when ramp is on the left side of the screen and peabody will slide through the side of the screen to the right side
                else
                {
                    //If there is an empty space, let peabody slide
                    if (GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow + 1) + "&" + (4)).tag == "EmptyTile")
                    {
                        GameObject.Find("/SceneManager/Slide").GetComponent<AudioSource>().Play(0);
                        
                        PeabodyLocationCol = 4;
                        PeabodyLocationRow = PeabodyLocationRow + 1;
                        Peabody[0].transform.position = GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow) + "&" + (PeabodyLocationCol)).transform.position;
                        return new Vector3(0, 0, 0);
                    }
                    //If there isnt, do nothing
                    else
                    {
                        return new Vector3(0, 0, 0);
                    }
                }



            //If there is an ramp with slide to the right side, check if there is an empty space on the right from the ramp, if yes, let peabody slide
            case "RampR":
                //Standart case with block space on the right side of the slide
                if (PeabodyLocationCol + 1 < 5)
                {
                    //If there is an empty space, let peabody slide
                    if (GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow + 1) + "&" + (PeabodyLocationCol + 1)).tag == "EmptyTile")
                    {
                        GameObject.Find("/SceneManager/Slide").GetComponent<AudioSource>().Play(0);
                        PeabodyLocationRow = PeabodyLocationRow + 1;
                        PeabodyLocationCol = PeabodyLocationCol + 1;
                        return GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow) + "&" + (PeabodyLocationCol)).transform.position;
                    }
                    //If there isnt, do nothing
                    else
                    {
                        return new Vector3(0, 0, 0);
                    }
                }
                //Case when ramp is on the right side of the screen and peabody will slide through the side of the screen to the left side
                else
                {
                    //If there is an empty space, let peabody slide
                    if (GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow + 1) + "&" + (0)).tag == "EmptyTile")
                    {
                        GameObject.Find("/SceneManager/Slide").GetComponent<AudioSource>().Play(0);
                        PeabodyLocationRow = PeabodyLocationRow + 1;
                        PeabodyLocationCol = 0;
                        Peabody[0].transform.position = GameObject.Find("/Canvas/Panel/GameObject/" + (PeabodyLocationRow) + "&" + (PeabodyLocationCol)).transform.position;
                        return new Vector3(0, 0, 0);
                    }
                    //If there isnt, do nothing
                    else
                    {
                        return new Vector3(0, 0, 0);
                    }
                }

                //If there is an spike block under peabody, take life from peabody
            case "Spike":

                TakeLife();
                return new Vector3(0, 0, 0);

                //If there is an teleport under peabody, let peabody fall - teleportation is handled when Peabody is on the same position as teleport
            case "Teleport":

                GameObject.Find("/SceneManager/Fall").GetComponent<AudioSource>().Play(0);
                PeabodyLocationRow = PeabodyLocationRow + 1;
                return UnderPeabody.transform.position;

                //if there isnt interactive block under peabody, do nothing
            default:
               
                    counter++;
                    return new Vector3(0, 0, 0);


                
                


        }

    }

    //Teleport method
    public static void teleportPeabody()
    {
        //Make sound of teleportation
        GameObject.Find("/SceneManager/Teleport").GetComponent<AudioSource>().Play(0);
        //Teleport peabody to new location
        Peabody[0].transform.position = Teleports[0].transform.position;

        //create animation on the location of the first teleport
        explosion = (GameObject)Instantiate(Resources.Load("Teleport"));
        explosion.transform.localPosition = new Vector2(Teleports[0].transform.position.x, Teleports[0].transform.position.y+50);
        explosion.transform.SetParent(GameObject.Find("/Canvas/Panel/GameObject/Peabody").transform);

        //create animation on the location of the second teleport
        explosion2 = (GameObject)Instantiate(Resources.Load("Teleport"));
        explosion2.transform.localPosition = new Vector2(Teleports[1].transform.position.x, Teleports[1].transform.position.y + 50);
        explosion2.transform.SetParent(GameObject.Find("/Canvas/Panel/GameObject/Peabody").transform);

    }

    //Show in GUI how many diamonds were collected
    public static void CollectDiamonds()
    {
        // proceed with method only if level wasnt finished
        if (Diamonds.Length < DiamondPoints.Length)
        {
            GameObject.Find("/SceneManager/Diamond").GetComponent<AudioSource>().Play(0);
            DiamondPoints[Diamonds.Length].GetComponent<Image>().color = new Color32(255, 255, 255, 255);

        }

    }

    //When peabody touch the spike, 1 life is lost
    public static void TakeLife()
    {
            GameObject.Find("/SceneManager/Loss").GetComponent<AudioSource>().Play(0);
            HeadText.GetComponent<Text>().text = "Game Over!";
            LowerText.GetComponent<Text>().text = "Sadly peabody died, try again!";
            GameObject.Find("/Canvas/Panel/LevelPanel").SetActive(true);
            GameObject.Find("/Canvas/Panel/").GetComponent<Image>().color = new Color32(255, 0, 0, 255);
            GameObject.Find("/Canvas/Panel/LevelPanel/Button (1)").SetActive(false);
            GameObject.Find("/Canvas/Panel/GameObject").SetActive(false);

    }

    // Pause game
    public static void PauseGame()
    {
        Time.timeScale = 0;
    }



    // Resume game
    public static void ResumeGame()
    {
        PopUpPanel.SetActive(false);
        Time.timeScale = 1;
    }


    //find objects with game tags for further functions
    public static void LoadData()
    {
        Empties = GameObject.FindGameObjectsWithTag("EmptyTile");
        Cracked = GameObject.FindGameObjectsWithTag("Cracked");
        DiamondPoints = GameObject.FindGameObjectsWithTag("DiamondPoint");
        RampsL = GameObject.FindGameObjectsWithTag("RampL");
        RampsR = GameObject.FindGameObjectsWithTag("RampR");
        Spikes = GameObject.FindGameObjectsWithTag("Spike");
        Spawn = GameObject.FindGameObjectsWithTag("Spawn");
        Teleports = GameObject.FindGameObjectsWithTag("Teleport");
        Diamonds = GameObject.FindGameObjectsWithTag("Diamond");
       
    }

}
