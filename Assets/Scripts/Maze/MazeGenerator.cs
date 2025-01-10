using System.Threading;
using UnityEngine;

public class MazeGenerator : MonoBehaviour{
    public const int HORIZONTAL = 1;
    private const int VERTICAL = 2;
    public const int S = 1;
    public const int E = 2;
    private const int wallLength = 4;
    
    public GameObject wallPrefab;
    private int groundWidth;
    private int groundHeight;

    int[,] maze;

    public void setSeed(int seed){
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

    private void Divide(int[,] maze, int x, int y, int width, int height, int orientation){
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

        // Just here for readability
        int hW = groundWidth/2;
        int hH = groundHeight/2;
        int hWL = wallLength/2;
        // Offset
        (int i, int j) = hori ? (-hW + hWL, hH - wallLength) : (-hW + wallLength, hH - hWL);

        for (int k = 0; k < length; k++){
            if (wallX != passX || wallY != passY){
                maze[wallX,wallY] |= direction; // Adds to array which isnt used except to print maze in console

                // Place wall
                (int xPos, int yPos) = (wallLength*wallX, wallLength*wallY);
                Quaternion angle = (orientation == HORIZONTAL) ? Quaternion.identity : Quaternion.Euler(0,90,0);
                Instantiate(wallPrefab, new(i + xPos, 0, j - yPos), angle);
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
        Divide(maze, newX, newY, newWidth, newHeight, DetermineOrientation(newWidth, newHeight));

        // Other chamber
        (newX, newY) = hori ? (x,wallY+1) : (wallX + 1, y);
        (newWidth, newHeight) = hori ? (width, y+height-wallY-1) : (x+width-wallX-1, height);
        Divide(maze, newX, newY, newWidth, newHeight, DetermineOrientation(newWidth, newHeight));
    }

    private void placeBorder(int width, int height){
        // Just here for readability
        int hW = groundWidth/2;
        int hH = groundHeight/2;
        int hWL = wallLength/2;
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
        groundWidth = width * wallLength;
        groundHeight = height * wallLength;

        placeBorder(width,height);
        Divide(maze, 0, 0, width, height, HORIZONTAL);
        // PrintMaze(maze);
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.L)){
            GenerateMaze(maze,3,10);
        }
    }
}