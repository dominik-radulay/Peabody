using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



/* Gridgenerator.cs
 * This file contain methods for generating of whole gamefield,
 * ensuring that its scaled and centered in middle of the screen according to used device
 * Frame rate is also set out in this script
 * 
*/

public class Gridgenerator : MonoBehaviour
{




    //public float ColumnsEnter;
    //public int LevelNumberEnter;

    // Following variables are for purpose of editing in the Engine GUI
    [Tooltip("Postition of solid blocks in format Row&Column")]
    public List<string> SolidBlocks;
    [Tooltip("Postition of diamonds in format Row&Column")]
    public List<string> Diamonds;
    [Tooltip("Postition of cracked blocks in format Row&Column")]
    public List<string> CrackedBlocks;
    [Tooltip("Postition of ramps with slide to the left side in format Row&Column")]
    public List<string> RampsLeft;
    [Tooltip("Postition of ramps with slide to the right side in format Row&Column")]
    public List<string> RampsRight;
    [Tooltip("Spawn position of Peabody in format Row&Column")]
    public string StartPosition;
    [Tooltip("Postition of spike blocks in format Row&Column")]
    public List<string> Spikes;
    [Tooltip("Postition of teleports in format Row&Column")]
    public List<string> Teleports;
    [Tooltip("Postition of blocks that allows to swipe only to the left in format Row&Column")]
    public List<string> OneWayL;
    [Tooltip("Postition of blocks that allows to swipe only to the right in format Row&Column")]
    public List<string> OneWayR;
    [Tooltip("Show tutorial panel at the start of the level")]
    public bool TutorialIncluded;

    //variables used for storing information about game field  (must be static)
    public static float rows;
    public static float columns;
    public static float cubeSize;
    public static float[] rowborders;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            UserReportingScript other = (UserReportingScript)GameObject.Find("/Canvas/Panel/UserReportingPrefab/UserReporting").GetComponent(typeof(UserReportingScript));
            other.CreateUserReport();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 300;
        Gridgenerate();

    }

    public void Gridgenerate()
    {


        // prevent wrong values
        if (columns < 5)
        {
            columns = 5;
        }


        //calculate number of rows depending on the number of columns
        rows = columns * 1.85f;

        //set cube size so it fits screen either by width or height (smaller number of those two)
        cubeSize = Mathf.Min((Screen.safeArea.height * 0.84f) / rows, (Screen.safeArea.width * 0.84f) / columns);

        //set size of array containing position of bottom line of each row
        rowborders = new float[(int)rows + 1];

        //Load reference block from the prefab and generate it inside of the Level
        GameObject referenceTile = (GameObject)Instantiate(Resources.Load("Square"));

        //set size of reference tile based on CubeSize calculated in previous rows
        referenceTile.transform.localScale = new Vector2(cubeSize, cubeSize);

        // position bug report button
        PositionReportButton();

        // create Diamonds bar
        GenerateDiamondPoints(referenceTile);

        //Nested loop for generating of grid filled with blocks
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                //Instantiate reference block
                GameObject tile = (GameObject)Instantiate(referenceTile, transform);

                //Calculate position for the block so the final grid is exactly in middle of the screen
                float posX = ((Screen.safeArea.width / 2) + Screen.safeArea.position.x - (columns * cubeSize) / 2 + (col * cubeSize) + (cubeSize / 2));
                float posY = ((Screen.safeArea.height / 2) + Screen.safeArea.position.y + (rows * cubeSize) / 2 - (row * cubeSize) - (cubeSize / 2));

                //Set position of the block
                tile.transform.position = new Vector2(posX, posY);

                //Set name of instantiated object to be equal to its location in the grid
                tile.name = (row + "&" + col);
                
                //Generate bock with predefined properties based on Level editor settings
                if (SolidBlocks.Contains(tile.name))
                {
                    tile.tag = "Solid";
                    tile.AddComponent<BoxCollider2D>();
                }     
                else if (CrackedBlocks.Contains(tile.name))
                {
                    GenerateBlock(tile,"Cracked", "Squares/cracked", true);
                }
                else if (RampsLeft.Contains(tile.name))
                {                     
                    GenerateBlock(tile, "RampL", "Squares/rampL", true);
                }
                else if (RampsRight.Contains(tile.name))
                {
                    GenerateBlock(tile, "RampR", "Squares/rampR", true);
                }
                else if (Spikes.Contains(tile.name)) 
                {
                    GenerateBlock(tile, "Spike", "Squares/spike", true);
                }
                else if (OneWayL.Contains(tile.name))
                {
                    GenerateBlock(tile, "OneWayL", "Squares/onewayL", true);
                }
                else if (OneWayR.Contains(tile.name))
                {
                    GenerateBlock(tile, "OneWayR", "Squares/onewayR", true);
                }
                else if (Diamonds.Contains(tile.name))
                {

                    GenerateBlock(tile, "EmptyTile", "Squares/empty", false);
                    GenerateSpecialTiles(referenceTile, posX, posY, "Diamond");
                }
                else if (Teleports.Contains(tile.name))
                {
                    GenerateBlock(tile, "Teleport", "Squares/Teleport", true);
                    if (GameObject.FindGameObjectsWithTag("Teleport").Length > 1)
                    {
                        //Blue Teleport
                        tile.GetComponent<Image>().color = new Color32(0, 200, 255, 255);
                    }
                    else
                    {
                        //Green Teleport
                        tile.GetComponent<Image>().color = new Color32(0, 255, 50, 255);

                    }

                }
                else if (StartPosition.Contains(tile.name))
                {
                    //Generate empty space
                    GenerateBlock(tile, "EmptyTile", "Squares/empty", false);
                    //tile.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
                    //Write location of peabody
                    BlockMechanics.PeabodyLocationCol = col;
                    BlockMechanics.PeabodyLocationRow = row;
                    //Generate Peabody
                    GenerateSpecialTiles(referenceTile, posX, posY, "Peabody");
                }
                else
                {
                    GenerateBlock(tile, "EmptyTile", "Squares/empty", false);
                }



            }

            //map coordinates of each row
            string Tilename = row + "&0";
            rowborders[row] = (GameObject.Find(Tilename).transform.position.y) - (float)(cubeSize * 0.5);
            //Debug.Log("bottom line of row " + row + " is:" + rowborders[row]);

        }
        Destroy(referenceTile);
        BlockMechanics.LoadData();
        GameObject.Find("/Canvas/Panel/GameObject/Diamond").transform.SetAsLastSibling();
        GameObject.Find("/Canvas/Panel/GameObject/Peabody").transform.SetAsLastSibling();
        Swipingmechanism.Peabody = GameObject.FindGameObjectsWithTag("Peabody");

        //If Level contains tutorial panel, show it
        if (TutorialIncluded)
        {
            Tutorial();
        }
        //Load Peabody variable in Block Mechanics Class
        BlockMechanics.Peabody = GameObject.FindGameObjectsWithTag("Peabody");


    }

    // ensure that report button is always in visible area in upper center of the screen
    void PositionReportButton()
    {
        GameObject.Find("/Canvas/Panel/UserReportingPrefab/UserReportButton").transform.position = new Vector2(Screen.safeArea.width + Screen.safeArea.position.x - cubeSize/2, Screen.safeArea.height + Screen.safeArea.position.y - cubeSize / 4);
        //GameObject.Find("/Canvas/Panel/UserReportingPrefab/UserReportForm/Panel/SubmitButton").GetComponent<Button>().interactable = false;
    }

    //Generate GUI that shows how many diamonds are left to collect
    void GenerateDiamondPoints(GameObject referenece)
    {
        for (int i = 2; i < Diamonds.Count + 2; i++)
        {
            GameObject DiamondPoint = (GameObject)Instantiate(referenece, transform);
            DiamondPoint.transform.localScale = new Vector2(cubeSize / 2, cubeSize / 2);
            Sprite Texture = Resources.Load<Sprite>("Squares/diamond");
            DiamondPoint.GetComponent<Image>().sprite = Texture;
            DiamondPoint.tag = "DiamondPoint";
            DiamondPoint.name = ("DiamondPoint");
            //DiamondPoint.transform.SetParent(GameObject.Find("/Canvas/Panel/GameObject/InfoBars").transform);
            DiamondPoint.GetComponent<Image>().color = new Color32(25, 25, 25, 255);
            DiamondPoint.transform.SetParent(GameObject.Find("/Canvas/Panel/GameObject/InfoBars").transform);
            DiamondPoint.transform.position = new Vector2(Screen.safeArea.position.x + i * (cubeSize / 2), Screen.safeArea.height + Screen.safeArea.position.y - cubeSize / 2);

        }
    }

    //Generate Diamonds, Peabody
    private void GenerateSpecialTiles(GameObject Reftile, float X, float Y, string name)
    {
        GameObject tile = (GameObject)Instantiate(Reftile, transform);
        tile.transform.position = new Vector2(X, Y);

        //Diamonds need to be in set place in GameObjects hierarchy to ensure that they are always in background
        tile.transform.SetParent(GameObject.Find("/Canvas/Panel/GameObject/" + name).transform);
        tile.name = (name);

        //If Peabody is the special tile
        if (name == "Peabody")
        {

            GenerateCollisionBoxes("Right",tile,0.8f, 0.5f);
            GenerateCollisionBoxes("Left", tile, -0.8f, 0.5f);


        }
        //If Diamond is the special tile
        else if (name == "Diamond")
        {
            GenerateCollisionBoxes("Collider", tile, 0, 0.5f);
        }
        

        Sprite Texture = Resources.Load<Sprite>("Squares/" + name);
        tile.GetComponent<Image>().sprite = Texture;
        tile.tag = name;
    }

    //Method for creating collision boxes
    void GenerateCollisionBoxes(string name, GameObject Praenttile, float offset, float scale)
    {
        GameObject Collider;
        Collider = new GameObject();
        Collider.transform.position = Praenttile.transform.position;
        Collider.name = name;
        Collider.transform.SetParent(Praenttile.transform);
        Collider.transform.localScale = new Vector2(scale, 0.3f);


        Collider.AddComponent<BoxCollider2D>();
        Collider.GetComponent<Collider2D>().isTrigger = true;
        Collider.GetComponent<Collider2D>().offset = new Vector2(offset, 0);
        Collider.AddComponent<Rigidbody2D>();
        Collider.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
        Collider.AddComponent<ColliderInfoCollection>();

        //If created collision box is for Peabody
        if (name == "Right" ||  name == "Left")
        {

            Collider.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.Interpolate;
        }

    }
    
    //Method for creating of usual blocks - basically all blocks except Peabody and Diamonds
    void GenerateBlock(GameObject tile, string tag, string texture, bool Colider)
    {
        Sprite Texture = Resources.Load<Sprite>(texture);
        tile.GetComponent<Image>().sprite = Texture;
        tile.tag = tag;

        //some boxes require colider
        if (Colider)
        {

            // add colider box
            tile.AddComponent<BoxCollider2D>();

            // spikes have smaller colider boxes
            if (tag == "Spike" || (tag == "Teleport"))
            {
                tile.GetComponent<BoxCollider2D>().size = new Vector3(0.5f, 0.5f, 1);
            }
        }
    }

    //If tutorial is ticked as "included" in the Level editor, show the Tutorial Panel at the start of the game
    static void Tutorial()
    {
        GameObject.Find("/Canvas/Panel/Tutorial").SetActive(true);
        BlockMechanics.PauseGame();
    }
    //Method for closing Tutorial panel when button is pressed
    public static void CloseTutorial()
    {
        GameObject.Find("/Canvas/Panel/Tutorial").SetActive(false);
        BlockMechanics.ResumeGame();
    }
}
