using System.Threading;
using UnityEngine;

public class MazeGenerator : MonoBehaviour{
    public const int HORIZONTAL = 1;
    private const int VERTICAL = 2;
    public const int S = 1;
    public const int E = 2;
    private const int wallLength = 4;
    
    public GameObject wallPrefab;
    public GameObject groundPrefab;

    // variables used when finding offset
    private int hW; // halfWidth
    private int hH; // halfHeight
    private int hWL = wallLength/2; // halfWallLength

    int[,] maze;

    public MazeGenerator(int seed = 0){
        // :(
        // Random.InitState(seed);
    }

    public void SetSeed(int seed){
        Random.InitState(seed);
    }

    public void PrintMaze(int[,] maze){
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);
        string mazeString = "";
        for (int i = 0; i < width*2; i++){
            mazeString += "_";
        }
        for (int y = 0; y < height; y++){
            mazeString += "\n|";
            for (int x = 0; x < width; x++){
                bool bottom = y+1 >= height;
                bool south = (maze[x,y] & S) == S || bottom;
                bool south2 = x+1 < width && (maze[x+1,y] & S) == S || bottom;
                bool east = (maze[x,y] & E) == E || x+1 >= width;

                mazeString += south ? "_" : " ";
                mazeString += east ? "|" : (south || south2 ? "_" : " ");
            }
        }
        Debug.Log(mazeString);
    }

    // Used to determine wall direction in new chamber, this also causes long corridors
    private static int DetermineOrientation(int w, int h){
        if (w < h) return HORIZONTAL;
        return VERTICAL;
    }

    // placeWalls input is to determine if we use seperate function to place the maze - can be removed when we figure it out.
    private void Divide(int[,] maze, int x, int y, int width, int height, int orientation, bool placeWalls = false){
        // return if corridor is one wide
        if (width <= 1 || height <= 1) return;

        // PrintMaze(maze); // Uncomment to print each iteration

        bool hori = orientation == HORIZONTAL;
        // Wall position
        int wallX = x + (hori ? 0 : Random.Range(0,width - 2));
        int wallY = y + (hori ? Random.Range(0,height - 2) : 0);

        // Passage position
        int passX = wallX + (hori ? Random.Range(0,width) : 0);
        int passY = wallY + (hori ? 0 : Random.Range(0,height));

        // wall direction
        int dx = hori ? 1 : 0;
        int dy = hori ? 0 : 1;

        int length = hori ? width : height;
        int direction = hori ? S : E;

        // Offset
        (int i, int j) = hori ? (-hW + hWL, hH - wallLength) : (-hW + wallLength, hH - hWL);

        for (int k = 0; k < length; k++){
            if (wallX != passX || wallY != passY){
                maze[wallX,wallY] |= direction; // Adds to array which isnt used except to print maze in console

                // Place wall
                if (placeWalls){
                    (int xPos, int yPos) = (wallLength*wallX, wallLength*wallY);
                    Quaternion angle = (orientation == HORIZONTAL) ? Quaternion.identity : Quaternion.Euler(0,90,0);
                    Instantiate(wallPrefab, new(i + xPos, 0, j - yPos), angle);
                }
            }
            wallX += dx;
            wallY += dy;
            continue;
        }

        // Recursively finish the maze in each of the new chambers
        // offset coordinates
        (int newX, int newY) = (x,y);
        // Dimensions of new chamber
        (int newWidth, int newHeight) = hori ? (width, wallY-y+1) : (wallX-x+1, height);
        Divide(maze, newX, newY, newWidth, newHeight, DetermineOrientation(newWidth, newHeight), placeWalls);

        // Other chamber
        (newX, newY) = hori ? (x,wallY+1) : (wallX + 1, y);
        (newWidth, newHeight) = hori ? (width, y+height-wallY-1) : (x+width-wallX-1, height);
        Divide(maze, newX, newY, newWidth, newHeight, DetermineOrientation(newWidth, newHeight), placeWalls);
    }

    private void PlaceBorder(int width, int height){
        Instantiate(groundPrefab, new(0, 0, 0), Quaternion.identity).transform.localScale = new Vector3((width*wallLength)/10.0f, 1, (height*wallLength)/10.0f);
        // Origo
        (int i, int j) = (-hW + hWL, hH - hWL);

        // Place top and bottom walls
        for(int xPos = i; xPos < i + wallLength*width; xPos += wallLength){
            Instantiate(wallPrefab, new(xPos, 0, -hH), Quaternion.identity);
            Instantiate(wallPrefab, new(xPos, 0, hH), Quaternion.identity);
        }
        // Place left and right walls
        for(int yPos = j; yPos > j - wallLength*height; yPos -= wallLength){
            Instantiate(wallPrefab, new(-hW, 0, yPos), Quaternion.Euler(0,90,0));
            Instantiate(wallPrefab, new(hW, 0, yPos), Quaternion.Euler(0,90,0));
        }
    }

    // The maze input is unnecessary, its only there if the caller wants a copy of the array. 
    // If so comment out the maze variable at the top and call.
    public void GenerateMaze(int[,] maze, int lowerBound, int upperBound){
        // Init maze Variables
        int width = Random.Range(lowerBound, upperBound);
        int height = Random.Range(lowerBound, upperBound);
        maze = new int[width, height];
        hW = width * wallLength/2;
        hH = height * wallLength/2;

        PlaceBorder(width,height);
        Divide(maze, 0, 0, width, height, HORIZONTAL, true);
        // PrintMaze(maze);
        // placeMaze(maze);
    }

    public void PlaceMaze(int[,] maze){
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);
        PlaceBorder(width, height);

        // Offset
        (int iHori, int jHori) = (-hW + hWL, hH - wallLength);
        (int iVert, int jVert) = (-hW + wallLength, hH - hWL);

        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                if ((maze[x,y] & S) == S){
                    Instantiate(wallPrefab, new(iHori + wallLength*x, 0, jHori - wallLength*y), Quaternion.identity);
                }
                if ((maze[x,y] & E) == E){
                    Instantiate(wallPrefab, new(iVert + wallLength*x, 0, jVert - wallLength*y), Quaternion.Euler(0,90,0));
                }
            }
        }
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.L)){
            GenerateMaze(maze,3,10);
        }
    }
}