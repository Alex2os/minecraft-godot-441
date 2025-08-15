using Godot;
using System;

public partial class GridMap : Godot.GridMap
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void destroy_block(Vector3 world_coordinate) // with this function we can erase or destroy blocks
	{
		// Vector3I allows us to represent 3D grid coordinates. this is why we use this instead of a normal Vector3, as the setcellitem function needs this vector instead of a usual Vector3.
		Vector3I grid_map_coordinate = LocalToMap(world_coordinate); // the localtomap function allows us to convert a world coordinate into a point of the gridmap. also, we need the variable to ve a Vector3I, so we can use it in the following function
		SetCellItem(grid_map_coordinate, -1); // the setcellitem allows us to set a certain item (block, in this case) to a sell. we pass the coordinates, and we also pass the id of the block, in this case passing an id of -1 means that there's no block there. if we wanted, we could replace that block with another one, putting the id that we have in the gridmap.

	}

	public void place_block(Vector3 world_coordinate, int block_index) // this funcion, besides the world coordinate, needs a block_index, as the player can place the different types of blocks that we have in the grid map
	{
		Vector3I grid_map_coordinate = LocalToMap(world_coordinate);
		SetCellItem(grid_map_coordinate, block_index); // instead of putting -1 like the destroy function, we now put the block_index, so now we will be placing a block.
	}
}
