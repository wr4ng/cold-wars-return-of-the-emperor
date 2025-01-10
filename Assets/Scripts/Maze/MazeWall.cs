using UnityEngine;

public class MazeWall: MonoBehaviour{

	public GameObject wallPrefab;
	private int wallLength = 4;
	private int wallWidth = 1;
	private int groundWidth, groundHeight;
	void Start()
	{
        Instantiate(wallPrefab, Vector3.zero, Quaternion.identity);
		MazeGenerator gen = new();
		int[,] maze;
		int width = Random.Range(5,10);
        int height = Random.Range(5,10);
        maze = new int[width,height];
        gen.Divide(maze ,0,0,width,height, MazeGenerator.HORIZONTAL);
		PlaceLabyrinth(maze,width,height);
		// PlaceLabyrinth(new int[10,10], 6, 6);
	}

	public void PlaceLabyrinth(int[,] maze, int width, int height){
		groundWidth = width*4;
		groundHeight = height*4;
		for (int i = 0; i < width; i++){
			Vector3 pos = new Vector3(-groundWidth/2 + wallLength*i+wallLength/2,0,-groundWidth/2);	
            Instantiate(wallPrefab, Vector3.zero , Quaternion.identity);
        }
		for (int y = 0; y < height; y++){
			Vector3 edge = new Vector3(-groundWidth/2,0,-groundWidth/2+ wallLength*y+wallLength/2);	
			Instantiate(wallPrefab, edge, Quaternion.Euler(0,90,0));
			for(int x = 0; x < width; x++){
				bool bottom = y+1 >= height;
				bool south = (maze[x,y] & MazeGenerator.S) == MazeGenerator.S || bottom;
				bool south2 = x+1 < width && (maze[x+1,y] & MazeGenerator.S) == MazeGenerator.S || bottom;
				bool east = (maze[x,y] & MazeGenerator.E) == MazeGenerator.E || x+1 >= width;

				if(south){
					Vector3 pos = new Vector3(-groundWidth/2 + wallLength*x+ wallLength/2,0,-groundHeight/2 + wallLength*y+wallLength);	
					Instantiate(wallPrefab, pos, Quaternion.identity);
				}
				if(east){
					Vector3 pos = new Vector3(-groundWidth/2 + wallLength*x+wallLength,0,-groundWidth/2+ wallLength*y+wallLength/2);	
					Instantiate(wallPrefab, pos, Quaternion.Euler(0,90,0));
				}
			}
		}
	}
}