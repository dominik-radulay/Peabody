using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/* Swipingmechanism.cs
 * This file contain methods for touch and swiping of the rows
 * 
 * 
 * 
*/


public class Swipingmechanism : MonoBehaviour
{
    private Vector2 startSwipePosition;
    private Vector2 currentFingerPosition;

    public static float offset;
    public static GameObject[] Squares;
    public static Vector2[] squaresnatur;
    public static int row;
    public static GameObject[] Peabody;
    private static GameObject NewPositionPeabody;
    private float PeabodyOffset;
    private float PeabodyStartOffset;
    private Vector3 startPeabodyPosition;
    private bool ContainsOneWayL = false;
    private GameObject OneWayL;
    private bool ContainsOneWayR = false;
    private GameObject OneWayR;
    private bool reportedblock = false;
    public static bool Peabodyfall = false;





    // Update is called once per frame
    void Update()
    {
        //If finger doesnt touch a screen method does not run
        if (Input.touchCount > 0 && !Peabodyfall)
        {
            SwipeDetection();
        }
        else if (offset != 0)
        {
            //If ColliderInfoCollection reported that Diamond is touching something and swipe should be blocked
            if (ColliderInfoCollection.blocker)
            {
                char position = ColliderInfoCollection.TouchedDiamond[ColliderInfoCollection.TouchedDiamond.Length - 1];
                //logic: '9' = ASCII 57, therefore 57 - 48 = 9
                int c = position - 48;
                // Diamond was touched during swipe to the left
                if (ColliderInfoCollection.offset > 0)
                {
                    offset = squaresnatur[c].x - ColliderInfoCollection.DiamondLocation-Gridgenerator.cubeSize;
                    MoveBlocks(offset);
                }
                // Diamond was touched during swipe to the right
                else
                {
                    offset = squaresnatur[c].x - ColliderInfoCollection.DiamondLocation + Gridgenerator.cubeSize;
                    MoveBlocks(offset);
                } 
            }
            LockInNewPoistion();
        }
        //If there is no active touch input, reset value of "Peabodyfall"
        else if (Input.touchCount == 0)
        {
            Peabodyfall = false;
        }
    }


    public void SwipeDetection()
    {
        if ((Input.GetTouch(0).position.y > Gridgenerator.rowborders[Gridgenerator.rowborders.Length - 1])&&(Input.GetTouch(0).position.y <(Gridgenerator.rowborders[0]+ Gridgenerator.cubeSize)))
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                startSwipePosition = Input.GetTouch(0).position;
                startPeabodyPosition = Peabody[0].transform.position;
                Peabodyfall = false;
                ContainsOneWayL = false;
                ContainsOneWayR = false;

                // Detect which row player swipe on
                SelectRow(startSwipePosition.y);


                //Save Original position of squares
                squaresnatur = new Vector2[Squares.Length];
                for (int i = 0; i < Squares.Length; i++)
                {
                    squaresnatur[i] = Squares[i].transform.position;
                }
                

            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
            {
                offset = startSwipePosition.x - Input.GetTouch(0).position.x;
                // Blocker is activate for example when row touche the diamond while swipe               
                if (!ColliderInfoCollection.blocker && !blocker())
                {
                    
                    //calculate offset to know distance which finger traveled
                    if (!ContainsOneWayL && !ContainsOneWayR)
                    {
                        MoveBlocks(offset);
                    }
                    //if one way Left cube get out of the screen on the rght side, its blocked
                    else if (ContainsOneWayL && (!(OneWayL.transform.position.x > squaresnatur[Squares.Length - 1].x) || (offset > 0)) && !ContainsOneWayR)
                    {
                        reportedblock = false;
                        MoveBlocks(offset);
                    }
                    //if one way Right cube get out of the screen on the left side, its blocked
                    else if (ContainsOneWayR && (!(OneWayR.transform.position.x < squaresnatur[0].x) || (offset < 0)) && !ContainsOneWayL)
                    {
                        reportedblock = false;
                        MoveBlocks(offset);
                    }
                    //if row contain both left and right one way cubes, the swipe is blocked if one of them get out of the screen in opposite way than their allowed direction
                    else if ((ContainsOneWayL && (OneWayL.transform.position.x <= squaresnatur[Squares.Length - 1].x)) && (ContainsOneWayR && (OneWayR.transform.position.x >= squaresnatur[0].x)))
                    {
                        MoveBlocks(offset);
                    }
                    else
                    {
                        if (!reportedblock)
                        {
                            GameObject.Find("/SceneManager/Block").GetComponent<AudioSource>().Play(0);
                            reportedblock = true;
                        }
                        
                    }
                }
                else if (blocker())
                {
                    
                    GameObject.Find("/SceneManager/Block").GetComponent<AudioSource>().Play(0);
                    LockInNewPoistion();
                    Peabodyfall = true;
                }
                else if (offset > 0 && offset < ColliderInfoCollection.offset)
                {
                    MoveBlocks(offset);
                }
                else if (offset < 0 && offset > ColliderInfoCollection.offset)
                {
                    MoveBlocks(offset);
                }
            }
        }
    }


    

    public void MoveBlocks(float offset)
    {
        // move blocks acordingly to position of the finger
        for (int i = 0; i < Squares.Length; i++)
        {
            //add offset into position
            Squares[i].transform.position = new Vector2(squaresnatur[i].x - offset, squaresnatur[i].y);



            //if cube on the left get out of the screen, it appears on the right
            if (Squares[i].transform.position.x < squaresnatur[0].x - Gridgenerator.cubeSize / 4)
            {
                Squares[i].transform.position += new Vector3(Gridgenerator.cubeSize * Squares.Length, 0);

            }

            //if cube on the right get out of the screen, it appears on the left
            else if (Squares[i].transform.position.x > squaresnatur[Squares.Length - 1].x + Gridgenerator.cubeSize / 4)
            {
                Squares[i].transform.position -= new Vector3(Gridgenerator.cubeSize * Squares.Length, 0);
            }

            //if cube touch peabody, peabody move
            MovePeabody();
        }

    }

    public void MovePeabody()
    {
        
        // When Cube touched peabody from the left  side
        if ( (ColliderInfoCollection.HisNameL != null) && (offset < 0))
        {
            if ((Int32.Parse(ColliderInfoCollection.HisNameL.Split('&')[0]) == row))
            {
                PeabodyOffset = offset - PeabodyStartOffset;
                if (NewPositionPeabody != null)
                {

                    //If player retracted his swipe above level of original position of peabody, peabody wouldnt move anymore
                    if (PeabodyOffset > 1)
                    {

                        Peabody[0].transform.position = startPeabodyPosition;
                        NewPositionPeabody = null;
                        PeabodyStartOffset = 0;
                    }
                    else
                    {
                        Peabody[0].transform.position = new Vector2(NewPositionPeabody.transform.position.x, Peabody[0].transform.position.y);
                    }
                }
                else if (PeabodyOffset < 0)
                {
                        NewPositionPeabody = PeabodyMagnet(1, ColliderInfoCollection.HisNameL);
                }
            }
        }
        // When Cube touched peabody from the right side
        else if ( (ColliderInfoCollection.HisNameR != null) && (offset > 0))
        {
            if (Int32.Parse(ColliderInfoCollection.HisNameR.Split('&')[0]) == row)
            {
                PeabodyOffset = offset - PeabodyStartOffset;

                if (NewPositionPeabody != null)
                {
                    //If player retracted his swipe above level of original position of peabody, peabody wouldnt move anymore
                    if (PeabodyOffset < 0)
                    {
                        Peabody[0].transform.position = startPeabodyPosition;
                        NewPositionPeabody = null;
                        PeabodyStartOffset = 0;

                    }
                    else
                    {
                        Peabody[0].transform.position = new Vector2(NewPositionPeabody.transform.position.x, Peabody[0].transform.position.y);
                    }
                }
                else if (PeabodyOffset > 0)
                {
                    NewPositionPeabody = PeabodyMagnet(-1, ColliderInfoCollection.HisNameR);
                }
            }
        }
    }




    public void LockInNewPoistion()
    {
        //If player moved a row left
        if (offset > 0)
        {
            for (int i = 0; i < Squares.Length; i++)
            {
                //Check what original block position is nearest to the rightest block in swpied row
                if (((Squares[Squares.Length - 1].transform.position.x) > (squaresnatur[i].x - Gridgenerator.cubeSize / 2) && (Squares[Squares.Length - 1].transform.position.x) < (squaresnatur[i].x + Gridgenerator.cubeSize / 2)) || ((Squares[Squares.Length - 1].transform.position.x) < squaresnatur[i].x && (i==0)))
                {
                   //save into variable by how many blocks was the row moved
                    int move = i-(Squares.Length - 1);
                    
                    for (int n = Squares.Length - 1; n > -1; n--)
                    {
                        if (n+move >-1)
                        {
                            //move the first square from absolute left to absolute right
                            Squares[n].transform.position = squaresnatur[n + move];
                            Squares[n].name = (row + "&" + (n + move));

                            //Copy coordinates if block is on same location as peabody
                            if (Squares[n] == NewPositionPeabody)
                            {
                                BlockMechanics.PeabodyLocationCol = n + move;
                                BlockMechanics.PeabodyLocationRow = row;
                            }
                        }
                        else
                        {
                            //Move square "i" to the postion of square one block left
                            Squares[n].transform.position = squaresnatur[Squares.Length + (n+move)];
                            Squares[n].name = (row + "&" + (Squares.Length + (n + move)));

                            //Copy coordinates if block is on same location as peabody
                            if (Squares[n] == NewPositionPeabody)
                            {
                                BlockMechanics.PeabodyLocationCol = Squares.Length + (n + move);
                                BlockMechanics.PeabodyLocationRow = row;

                            }
                        }


                    }
                    offset = 0;
                    if (NewPositionPeabody != null)
                    { 
                    Peabody[0].transform.position = new Vector2(NewPositionPeabody.transform.position.x, Peabody[0].transform.position.y);
                    PeabodyStartOffset = 0;
                    NewPositionPeabody = null;
                    }
                    //send info to SwpiePrototype Class that swipe was finished
                    BlockMechanics.Swiped = true;
                }               
            }
        }
        //If player moved a row right
        else if (offset < 0)
        {
            for (int i = Squares.Length - 1; i > -1; i--)
            {
                //Check what original block position is nearest to the leftest block in swpied row
                if (((Squares[0].transform.position.x) > (squaresnatur[i].x - Gridgenerator.cubeSize / 2) && (Squares[0].transform.position.x) < (squaresnatur[i].x + Gridgenerator.cubeSize / 2))|| (Squares[0].transform.position.x > (squaresnatur[i].x) && i==(Squares.Length - 1)))
                {
                    //save into variable by how many blocks was the row moved
                    int move = i;

                    for (int n = 0; n < Squares.Length; n++)
                    {
                        if (n + move < Squares.Length)
                        {
                            //move the first square from absolute left to absolute right
                            Squares[n].transform.position = squaresnatur[n + move];
                            Squares[n].name = (row + "&" + (n + move));

                            //Copy coordinates if block is on same location as peabody
                            if (Squares[n] == NewPositionPeabody)
                            {
                                BlockMechanics.PeabodyLocationCol = n + move;
                                BlockMechanics.PeabodyLocationRow = row;
                            }
                        }   
                        else
                        {
                            //Move square "i" to the postion of square one block left
                            Squares[n].transform.position = squaresnatur[(n + move)-Squares.Length];
                            Squares[n].name = (row + "&" + ((n + move)- Squares.Length));

                            //Copy coordinates if block is on same location as peabody
                            if (Squares[n] == NewPositionPeabody)
                            {
                                BlockMechanics.PeabodyLocationCol = (n + move) - Squares.Length;
                                BlockMechanics.PeabodyLocationRow = row;

                            }
                        }                        
                    }
                    offset = 0;
                    if (NewPositionPeabody != null)
                    {
                        Peabody[0].transform.position = new Vector2(NewPositionPeabody.transform.position.x, Peabody[0].transform.position.y);
                        NewPositionPeabody = null;
                        PeabodyStartOffset = 0;
                    }
                    //send info to SwpiePrototype Class that swipe was finished
                    BlockMechanics.Swiped = true;
                }
            }
        }
    }


    //Select right row depending on if touch was conducted above bottom corner of block on specific row
    public void SelectRow(float swipelevel)
    {
        int i = (int)Gridgenerator.rows-1;


        while (swipelevel > Gridgenerator.rowborders[i])
        {
            i--;
            if (i == -1)
            {
                
                break;

            }
        }



        i++;
            //Load variable with specific row
            Squares = new GameObject[(int)Gridgenerator.columns];
            row = i;
            bool found = false;
            for (int y = 0; y < Gridgenerator.columns; y++)
            {
                Squares[y] = GameObject.Find("/Canvas/Panel/GameObject/" + i + "&" + y);

                if (!found)
                {
                    if (Squares[y].CompareTag("OneWayR"))
                    {
                        if (ContainsOneWayL == true)
                        {
                            ContainsOneWayL = true;
                            ContainsOneWayR = true;
                            OneWayR = Squares[y];
                            found = true;
                        }
                        else
                        {
                            ContainsOneWayL = false;
                            ContainsOneWayR = true;
                            OneWayR = Squares[y];
                            //found = true;
                        }
                    }
                    else if (Squares[y].CompareTag("OneWayL"))
                    {
                        if (ContainsOneWayR == true)
                        {
                            ContainsOneWayL = true;
                            ContainsOneWayR = true;
                            OneWayL = Squares[y];
                            found = true;
                        }
                        else
                        {
                            ContainsOneWayL = true;
                            ContainsOneWayR = false;
                            OneWayL = Squares[y];
                            //found = true;
                        }
                    }
                }             
            }

    }

    // Find an empty space to which should peabody attach in case when block is already pushing Peabody
    private GameObject PeabodyMagnet (int side, string Hisname)
    {
        PeabodyStartOffset = offset;    
        string MagnetTo;
        int newpos = int.Parse(Hisname.Substring(Hisname.Length - 1)) + side;

        if (newpos > Squares.Length-1)
        {
            newpos = 0;

        }
        else if (newpos < 0)
        {
            newpos = Squares.Length-1;
        }

        MagnetTo = Hisname.Remove(Hisname.Length - 1, 1) + newpos;

        Peabody[0].transform.position = new Vector2(GameObject.Find(MagnetTo).transform.position.x, Peabody[0].transform.position.y);
        return GameObject.Find(MagnetTo);

    }

    // detect if there is an empty space under peabody while swiping
    private bool pitdetector (bool right, int rower)
    {
        
        //if user swiped to the right
        if (right)
        { 
            for (int i = 0; i < 5;i++)
            {
                //find gameobject that is under Peabody
                GameObject gameobject = GameObject.Find("/Canvas/Panel/GameObject/" + (row + rower) + "&" + i);

                //If an empty place or cracked block etc... that was previously on the left from Peabody move right into position that he is now under peabody, make peabody fall
                if (gameobject.CompareTag("EmptyTile") || gameobject.CompareTag("Cracked") || gameobject.CompareTag("RampR") || gameobject.CompareTag("RampL") || gameobject.CompareTag("Teleport"))
                {
                    //In case when peabody is swiped through the right side of the screen and empty space is on the left side of the row
                    if (Peabody[0].transform.position.x > gameobject.transform.position.x &&
                        gameobject.transform.position.x+ Gridgenerator.cubeSize/2 > Peabody[0].transform.position.x
                        //squaresnatur[Squares.Length - 1].x < startPeabodyPosition.x - PeabodyOffset-Gridgenerator.cubeSize/2
                        && rower ==1)
                    {
                        return true;
                    }
                    //In case when row under peabody is swiped to the right
                    if (Peabody[0].transform.position.x > gameobject.transform.position.x && squaresnatur[i].x + Gridgenerator.cubeSize * (5) - offset < Peabody[0].transform.position.x && rower == 0)
                    { 
                        return true;
                    }
                    //In case when peabody is swiped to the left and empty block was already on the right from peabody
                    else if (Peabody[0].transform.position.x > gameobject.transform.position.x && i > BlockMechanics.PeabodyLocationCol)
                    {
                        return true;
                    }



                }

            }
            return false;
        }
        //if user swiped to the left
        else
        {
            for (int i = 4; i > -1; i--)
            {
                //find gameobject that is under Peabody
                GameObject gameobject = GameObject.Find("/Canvas/Panel/GameObject/" + (row + rower) + "&" + i);
                //If an empty place or cracked block etc... that was previously on the right from Peabody move left into position that he is now under peabody, make peabody fall
                if (gameobject.CompareTag("EmptyTile") || gameobject.CompareTag("Cracked") || gameobject.CompareTag("RampR") || gameobject.CompareTag("RampL"))
                {
                    //In case when peabody is swiped through the left side of the screen and empty space is on the right side of the row
                    if (Peabody[0].transform.position.x < gameobject.transform.position.x && 
                        Peabody[0].transform.position.x > gameobject.transform.position.x - Gridgenerator.cubeSize/2
                        //squaresnatur[0].x - Gridgenerator.cubeSize > startPeabodyPosition.x - PeabodyOffset / 2 
                        && rower ==1)
                    {
                        return true;
                    }
                    //In case when row under peabody is swiped to the left
                    if (Peabody[0].transform.position.x < gameobject.transform.position.x && squaresnatur[i].x - Gridgenerator.cubeSize * (5) - offset > Peabody[0].transform.position.x && rower == 0)
                    {
                        
                        return true;
                    }
                    //In case when peabody is swiped to the left and empty block was already on the left from peabody
                    else if (Peabody[0].transform.position.x < gameobject.transform.position.x && i < BlockMechanics.PeabodyLocationCol)
                    {
                        return true;
                    }
                }

            }
            return false;
        }
    }

    private bool blocker ()
    { 
        if (offset< 0)
        {
            if (row == BlockMechanics.PeabodyLocationRow)
            {
                return pitdetector(true, 1);
            }
            else if (row == BlockMechanics.PeabodyLocationRow + 1)
            {
                return pitdetector(false, 0);
            }
            else
            {
                return false;
            }

        }
        else if (offset > 0)
        {

            if (row == BlockMechanics.PeabodyLocationRow)
            {
                return pitdetector(false, 1);
            }
            else if (row == BlockMechanics.PeabodyLocationRow + 1)
            {
                return pitdetector(true, 0);
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}

