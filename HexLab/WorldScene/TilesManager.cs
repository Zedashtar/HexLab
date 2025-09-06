using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using HexUtilities;

public partial class TilesManager : Node3D
{

	public TileMapResource tilemap;
	public HexGrid grid;

	public Node3D tile_container;

	[ExportGroup("Testing")]
	[Export] public PackedScene test_tile;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if ((HexGrid)GetNode("%HexGrid") == null) throw new Exception("TilesManager: No HexGrid found in scene tree.");
		if ((Node3D)GetNode("TileContainer") == null) throw new Exception("TilesManager: No TileContainer found in children.");
		grid = (HexGrid)GetNode("%HexGrid");
		tile_container = (Node3D)GetNode("TileContainer");
		//if tilemap Loader passes a tilemap, Load tilemap Here
		//Else
		tilemap = new TileMapResource();
		//Init_Test();
		//DebugDisplayContent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	private void DebugDisplayContent()
	{
		if (tilemap.tiles.Count == 0) return;
		foreach (Vector3 key in tilemap.tiles.Keys)
		{
			Debug.WriteLine(key.ToString() + " : " + tilemap.tiles[key]);
		}
	}

	private void _base_add_tile(Hex h)
	{
		Tile _tile = (Tile)test_tile.Instantiate();
		tile_container.AddChild(_tile);
		_tile.Position = grid.layout.GridToWorldspace(h);
		tilemap.tiles.Add(h.ToVector3(), _tile.type.ToString());
	}

	public virtual void AddTileSingle(Hex h)
	{
		if (!tilemap.tiles.ContainsKey(h.ToVector3()))
		{
			_base_add_tile(h);
		}
	}

	public virtual void AddTileMultiple(List<Hex> hexes)
	{
		foreach (Hex h in hexes)
		{
			if (!tilemap.tiles.ContainsKey(h.ToVector3()))
			{
				_base_add_tile(h);
			}
		}
	}

	public virtual void ClearTiles()
	{
		tilemap.tiles.Clear();
		foreach (Node3D n in tile_container.GetChildren())
		{
			n.QueueFree();
		}
	}


}
