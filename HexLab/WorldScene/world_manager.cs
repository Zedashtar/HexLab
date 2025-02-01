using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HexUtilities;

public partial class world_manager : Node3D
{

	public WorldResource world;
	private HexGrid grid;

	private Node3D tile_container;

	[ExportGroup("Testing")]
	[Export] private PackedScene test_tile;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		grid = (HexGrid)GetNode("%HexGrid");
		tile_container = (Node3D)GetNode("TileContainer");
		//if World Loader passes a World, Load World Here
		//Else
		world = new WorldResource();
		Init_Test();
		DebugDisplayContent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}




	private void Init_Test()
	{
		if (test_tile != null)
		{
			List<Hex> hexes = new List<Hex> {new Hex(grid.layout.worldspace_origin)};
			hexes.AddRange(hexes[0].Adjacents());
			foreach (Hex h in hexes)
			{
				Tile _tile = (Tile)test_tile.Instantiate();
				tile_container.AddChild(_tile);
				_tile.Position = grid.layout.GridToWorldspace(h);
				world.tiles.Add(h.ToVector3(), _tile.type.ToString());
			}
		}

	}

	private void DebugDisplayContent()
	{
		foreach (Vector3 key in world.tiles.Keys)
		{
			Debug.WriteLine(key.ToString() + " : " + world.tiles[key]);
		}
	}
}
