using UnityEngine;

public class MazeGenerator : MonoBehaviour{
    public const int HORIZONTAL = 1;
    private const int VERTICAL = 2;

    public const int S = 1;
    public const int E = 2;
    // public int[,] maze;
    private int seed = 0;
    private int height,width;

    public void setSeed(int seed){
        this.seed = seed;
    }
    public void PrintMaze(int[,] maze){
        string m = "";
        for (int i = 0; i < width*2; i++){
            m += "_";
        }
        for (int y = 0; y < height; y++){
            m += "\n|";
            for (int x = 0; x < width; x++){
                bool bottom = y+1 >= height;
                bool south = (maze[x,y] & S) == S || bottom;
                bool south2 = (x+1 < width && (maze[x+1,y] & S) == S || bottom);
                bool east = (maze[x,y] & E) == E || x+1 >= width;

                m += south ? "_" : " ";
                m += east ? "|" : (south || south2 ? "_" : " ");
            }
        }
        Debug.Log(m);
    }

    private int ChooseOrientation(int w, int h){
        if (w < h) return HORIZONTAL;
        if (w > h) return VERTICAL;
        return Random.Range(1,2);
    }

    public void Divide(int[,] maze, int x, int y, int width, int height, int orientation){
        if (width < 2 || height < 2) return;

        // PrintMaze(maze);
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

        // Put in Wall
        int length = hori ? width : height;
        int direction = hori ? S : E;

        for (int i = 0; i < length; i++){
            if (wallX != passX || wallY != passY){
                maze[wallX,wallY] |= direction;
            }
            wallX += dx;
            wallY += dy;
            continue;
        }

        int newX = x;
        int newY = y;
        (int newWidth, int newHeight) = hori ? (width, wallY-y+1) : (wallX-x+1, height);
        Divide(maze, newX, newY, newWidth, newHeight, ChooseOrientation(newWidth, newHeight));

        newX = hori ? x : wallX+1;
        newY = hori ? wallY+1 : y;
        (newWidth, newHeight) = hori ? (width, y+height-wallY-1) : (x+width-wallX-1, height);
        Divide(maze, newX, newY, newWidth, newHeight, ChooseOrientation(newWidth, newHeight));
    }

    public void GenerateMaze(int[,] maze){
        width = Random.Range(5,10);
        height = Random.Range(5,10);
        maze = new int[width,height];
        Divide(maze ,0,0,width,height, HORIZONTAL);
        PrintMaze(maze);
        // MazeWall dinmor = new();
        // dinmor.PlaceLabyrinth(maze, width,height);
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.L)){
            Debug.Log("Generating Maze");
            
            // GenerateMaze();
            
        }
    }
}