using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEngine.ParticleSystemModule;
public class GameScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera camera;
   
    public Transform Meep_selection_vertical;

    Vector3 worldPosition;
    public List<Player> players;
    public List<GameObject> UnshuffledTiles;
    public List<GameObject> RiverTiles;
    public List<GameObject> Tiles_Deck;
    public GameObject RiverSourceTile, RiverLakeTile, MeepPos_btn;
    public Tile[,] map = new Tile[35, 35];
    public float ScrollSpeed = 1;
    public int numOfPlayers = 2;
    public int player_counter = 0;
    public Stage STAGE = Stage.Main_Menu;

    public GameObject currentPlacingTile = null;

    public Texture2D cursorTexture;
    static System.Random rng = new System.Random();
    public Direction river_bend_direction = Direction.South;
    public Text TileCounter;
    private List<(MeepPositions, Direction)> valid_positions;
    private List<GameObject> Meep_selec_btns = new List<GameObject>();
    private (int x, int y) curr_tile;
    private Vector3 camerascrollvector = new Vector3 (1 , 0 , 0);
    private const float cam_min_z = -56f, cam_max_z = 46f,cam_min_x = -56f, cam_max_x = 46f; 
    private const float camera_pan_speed = 0.1f;
    private float time_counter = 0;
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(camera.gameObject);
    }
    void Start()
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        Cursor.lockState = CursorLockMode.Confined;


    }
    void GenerateMap()
    {
        GameObject source_tile = Instantiate(RiverSourceTile);
        source_tile.transform.position = new Vector3(0, 0, 0);
        map[17, 17] = source_tile.GetComponent<Tile>();


    }
    // Update is called once per frame
    void Update()
    {
        time_counter+=Time.deltaTime;
        switch (STAGE)
        {
            case Stage.Main_Menu:
                //waiting for input from user
                PanCamera();
                break;
            case Stage.Start_Game:
                if (SceneManager.GetActiveScene().name == "Game")
                {GenerateMap();
                GenerateTileDeck();
                CentreCamera();
                TileCounter = GameObject.Find("DeckText").GetComponent<Text>();
                Debug.Log(Tiles_Deck.Count);
                TileCounter.text = string.Format("{0}", Tiles_Deck.Count);
                Debug.Log(Tiles_Deck.Count);
                STAGE = Stage.Start_Turn;
                Meep_selection_vertical = GameObject.Find("MeepSelectionVertical").transform;
                }
                break;
            case Stage.Start_Turn:
                Debug.Log(Tiles_Deck.Count);
                if (Tiles_Deck.Count == 0)
                {
                    Debug.Log("ENDEDED");
                    STAGE = Stage.End_Game;
                    break;
                }
                currentPlacingTile = Draw_From_Deck();
                STAGE = Stage.Place_Tile;
                break;
            case Stage.Place_Tile:

                PlaceTile(currentPlacingTile.GetComponent<Tile>());
                MoveCamera();
                ZoomCamera();

                if (Input.GetMouseButtonDown(0) && ValidPosition(currentPlacingTile))
                {
                    curr_tile = SetTile(currentPlacingTile);
                    TileCounter.text = string.Format("{0}", Tiles_Deck.Count);
                    
                    valid_positions = ValidateLocations(currentPlacingTile.GetComponent<Tile>().get_Meep_Locations(), curr_tile);
                    STAGE = Stage.Start_Meep_Round;
                }

                break;
            case Stage.Start_Meep_Round:
                if (valid_positions == null || valid_positions.Count == 0)
                {
                    STAGE = Stage.End_Turn;
                    break;
                }
                // create UI
                foreach ((MeepPositions p, Direction d) v in valid_positions)
                {
                    Debug.Log(v.p + " , " + v.d);
                    var btn = Instantiate(MeepPos_btn, Meep_selection_vertical);
                    // todo: set position
                    btn.GetComponent<MeepPositionSelect>().Set_Params(this, v);
                    Debug.Log(v.p + " , " + v.d);
                    Meep_selec_btns.Add(btn);
                }
                var cancelbtn = Instantiate(MeepPos_btn, Meep_selection_vertical);
                // todo: set position
                cancelbtn.GetComponent<MeepPositionSelect>().Set_As_None(this);
                Meep_selec_btns.Add(cancelbtn);
                STAGE = Stage.Place_Meep;
                break;
            case Stage.Place_Meep:
                MoveCamera();
                ZoomCamera();
                break;
            case Stage.End_Turn:
                player_counter = (player_counter + 1) % numOfPlayers;
                valid_positions = null;
                STAGE = Stage.Start_Turn;
                break;
            case Stage.End_Game:
                break;

        }

    }
    public void SkipMeepPlacement()
    {
        if (STAGE == Stage.Place_Meep)
        {
            STAGE = Stage.End_Turn;
            DestroyMeepSelectBtns();
        }
    }
    public void PlaceMeep(MeepPositions p, Direction d)
    {
        if (STAGE != Stage.Place_Meep)
            return;
        Claim(curr_tile, p, d);
        STAGE = Stage.End_Turn;
        DestroyMeepSelectBtns();
    }
    private void DestroyMeepSelectBtns()
    {
        foreach (var b in Meep_selec_btns)
        {
            Destroy(b);
        }
        Meep_selec_btns.Clear();
        Meep_selec_btns.TrimExcess();
    }
    private void Claim((int x, int y) pos, MeepPositions position, Direction d_in)
    {
        Tile tile = map[pos.x, pos.y];
        if (tile == null)
            return;
        if (tile.claimed.Contains((position, d_in)))
            return;

        //check if tile is valid
        if (Tile.SideMatchesPosition(tile.sides[(int)d_in], position))
            tile.claimed.Add((position, d_in));
        else
            return;
        for (int i = 0; i < 4; i++)
        {
        
            if (Tile.SideMatchesPosition(tile.sides[i], position))
            {
                (int x, int y) = Tile.DirectionToIntPair((Direction)i);
                Claim((pos.x + x, pos.y + y), position, Tile.InvertDirection((Direction)i));
            }

        }

    }
    public List<(MeepPositions, Direction)> ValidateLocations(List<(MeepPositions, List<Direction>)> locations, (int x, int y) tile_pos)
    {
        List<(MeepPositions, Direction)> output = new List<(MeepPositions, Direction)>();
        if (locations.Count == 0) return null;
        foreach ((MeepPositions position, List<Direction> directions) meep in locations)
        {
            Debug.Log(meep.position);


            switch (meep.position)
            {
                case MeepPositions.OnCastle:
                case MeepPositions.OnRoad:
                    foreach (var dir in meep.directions)
                    {
                        (int x, int y) = Tile.DirectionToIntPair(dir);
                        var l = new List<(int x, int y)> { tile_pos };
                        if (recurse_validate_meeps((tile_pos.x + x, tile_pos.y + y), meep.position, Tile.InvertDirection(dir), ref l))
                        {
                            output.Add((meep.position, dir));
                            Debug.Log("VALID " + meep.position + " " + dir);
                        }
                    }
                    break;

                case MeepPositions.OnTower:
                    output.Add((meep.position, Direction.Centre));
                    break;
            }
        }
        return output;
    }
    public bool recurse_validate_meeps((int x, int y) tile_pos, MeepPositions pos_type, Direction d, ref List<(int x, int y)> visited)
    {
        //edge cases here need to think about dead ends that loop
        if (visited.Contains(tile_pos))
            return true;
        var tile = map[tile_pos.x, tile_pos.y];
        if (tile == null)
            return true;

        foreach ((MeepPositions position, Direction direction) meep in tile.claimed)
        {
            if (meep.direction == d && meep.position == pos_type)
                return false;
        }
        bool result = true;
        visited.Add(tile_pos);
        for (int i = 0; i < 4; i++)
        {
            if (Tile.SideMatchesPosition(tile.sides[i], pos_type))
            {
                (int x, int y) = Tile.DirectionToIntPair((Direction)i);
                result |= recurse_validate_meeps((tile_pos.x + x, tile_pos.y + y), pos_type, Tile.InvertDirection((Direction)i), ref visited);
            }
        }
        return result;

    }
    public void Start_Game()
    {
        STAGE = Stage.Start_Game;


        players = new List<Player>();
        for (int i = 0; i < numOfPlayers; i++)
        {
            players.Add(new Player());
        }
        SceneManager.LoadScene("Scenes/Game");
        player_counter = 0;

    }
    public bool ValidPosition(GameObject _tile)
    {

        int x = (int)Mathf.Round(_tile.transform.position.x / 3) + 17;
        int y = (int)Mathf.Round(_tile.transform.position.z / 3) + 17;
        Debug.Log(string.Format("x : {0}, y : {1}, position : {2}", x, y, _tile.transform.position));
        if (map[x, y] != null)
            return false;


        Tile tile = _tile.GetComponent<Tile>();
        if (tile.is_river)
        {
            if (tile.is_river_bend)
            {
                // TODO : make sure river never turns twice in same dir
            }
            // TODO make sure river tiles only connect with river tiles
        }
        bool is_adjacent = false;
        if (y > 0)
        {
            Tile tile_below = map[x, y - 1];

            if (tile_below != null)
            {
                Debug.Log(string.Format("tile_below side: {0}", tile_below.sides[0]));
                if (tile.sides[(int)Direction.South] == tile_below.sides[(int)Direction.North])
                {
                    is_adjacent = true;
                }
                else
                {
                    return false;
                }
            }

        }

        if (y < map.GetLength(1))
        {
            Tile tile_above = map[x, y + 1];
            if (tile_above != null)
            {
                Debug.Log(string.Format("tile_above side: {0}", tile_above.sides[2]));
                if (tile.sides[(int)Direction.North] == tile_above.sides[(int)Direction.South])
                {
                    is_adjacent = true;
                }
                else
                {
                    return false;
                }
            }
        }

        if (x > 0)
        {
            Tile tile_left = map[x - 1, y];
            if (tile_left != null)
            {
                Debug.Log(string.Format("tile_left side: {0}", tile_left.sides[1]));

                if (tile.sides[(int)Direction.West] == tile_left.sides[(int)Direction.East])
                {
                    is_adjacent = true;
                }
                else
                {
                    return false;
                }
            }

        }

        if (x < map.GetLength(0))
        {
            Tile tile_right = map[x + 1, y];
            if (tile_right != null)
            {
                Debug.Log(string.Format("tile_right side: {0}", tile_right.sides[3]));

                if (tile.sides[(int)Direction.East] == tile_right.sides[(int)Direction.West])
                {
                    is_adjacent = true;
                }
                else
                {
                    return false;
                }
            }
        }
        return is_adjacent;
    }
    public (int x, int y) SetTile(GameObject tile)
    {
        int x = (int)Mathf.Round(tile.transform.position.x / 3) + 17;
        int y = (int)Mathf.Round(tile.transform.position.z / 3) + 17;
        var t = tile.GetComponent<Tile>();
        map[x, y] = t;

        t.GetComponent<ParticleSystem>().Play ();
        
        return (x, y);
    }
    public GameObject Draw_From_Deck()
    {
        GameObject tile = Instantiate(Tiles_Deck[0]);
        Tiles_Deck.RemoveAt(0);

        return tile;
    }
    void GenerateTileDeck()
    {
        List<GameObject> land_tiles = ShuffleList<GameObject>(UnshuffledTiles);
        List<GameObject> river_tiles = ShuffleList<GameObject>(RiverTiles);
        river_tiles.Add(RiverLakeTile);
        Tiles_Deck = river_tiles;
        Tiles_Deck.AddRange(land_tiles);
        Debug.Log(Tiles_Deck.Count);
    }
    List<T> ShuffleList<T>(List<T> list)
    {
        List<T> outputList = new List<T>();
        while (list.Count > 0)
        {
            int index = rng.Next(list.Count);
            outputList.Add(list[index]);
            list.RemoveAt(index);
        }
        return outputList;
    }
    void MoveCamera()
    {
        float scrollSpeed = ScrollSpeed * Time.deltaTime;
        float allowance = 0.02f;
        if (Input.mousePosition.y >= Screen.height * (1 - allowance))
        {
            camera.transform.Translate(scrollSpeed, 0, scrollSpeed, Space.World);
        }
        if (Input.mousePosition.x >= Screen.width * (1 - allowance))
        {
            camera.transform.Translate(scrollSpeed, 0, -scrollSpeed, Space.World);
        }
        if (Input.mousePosition.y <= Screen.height * allowance)
        {
            camera.transform.Translate(-scrollSpeed, 0, -scrollSpeed, Space.World);
        }
        if (Input.mousePosition.x <= Screen.width * allowance)
        {
            camera.transform.Translate(-scrollSpeed, 0, scrollSpeed, Space.World);
        }
        if (camera.transform.position.z > cam_max_z)
            camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 46f);
        if (camera.transform.position.z < cam_min_z)
            camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, -56f);
        if (camera.transform.position.x > cam_max_x)
            camera.transform.position = new Vector3(46f, camera.transform.position.y, camera.transform.position.z);
        if (camera.transform.position.x < cam_min_x)
            camera.transform.position = new Vector3(-56f, camera.transform.position.y, camera.transform.position.z);
        if (Input.GetAxis("Centre")>0f)
            CentreCamera();

    }
    void ZoomCamera()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            camera.orthographicSize--;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // forward
        {
            camera.orthographicSize++;
        }
        if (camera.orthographicSize < 3)
            camera.orthographicSize = 3;
        if (camera.orthographicSize > 21)
            camera.orthographicSize = 21;
    }
   
    void CentreCamera()
    {
        
        // Centre camera over current tile 
        camera.transform.position = new Vector3(1 , 12 , 1);
    }

    void PanCamera(){
        
        
        
        camerascrollvector = new Vector3(18*Mathf.Cos(time_counter*camera_pan_speed) + 1, 12, 18*Mathf.Sin(time_counter*camera_pan_speed)+ 1);
        camera.transform.position = camerascrollvector;
    }

    void PlaceTile(Tile tile)
    {
        if (Input.GetMouseButtonDown(1))
            tile.Rotate();

        Plane plane = new Plane(Vector3.up, 0);

        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            worldPosition = ray.GetPoint(distance);
            worldPosition = new Vector3(Mathf.Round(worldPosition.x / 3) * 3f, 0, Mathf.Round(worldPosition.z / 3) * 3f);
            tile.transform.position = worldPosition;
        }
    }
    public enum Stage
    {
        Main_Menu,
        Start_Game,
        Start_Turn,
        Place_Tile,
        Start_Meep_Round,
        Place_Meep,
        End_Turn,
        End_Game
    }
}


