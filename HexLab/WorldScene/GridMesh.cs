using Godot;
using System;
using HexUtilities;


public partial class GridMesh : MeshInstance3D
{

	[Export] private Node3D camera_rig;
	[Export] private float height = 0.095f;
	private HexGrid grid;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		grid = (HexGrid)GetNode("%HexGrid");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (camera_rig != null)
		{
			Hex grid_pos = grid.layout.WorldspaceToGrid(camera_rig.Transform.Origin);
			Vector3 world_pos = grid.layout.GridToWorldspace(grid_pos);
			Position = world_pos  + Vector3.Up * height;
		}
	}
}
