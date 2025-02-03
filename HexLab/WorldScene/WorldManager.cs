using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HexUtilities;
using Godot.Collections;

public partial class WorldManager : Node3D
{

	public WorldResource world;
	private HexGrid grid;

	public List<Hex> primed_sites = new List<Hex>();
	private PrimedOverlay primed_overlay;
	private Node3D tile_container;


	[ExportGroup("Testing")]
	[Export] private PackedScene test_tile;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		grid = (HexGrid)GetNode("%HexGrid");
		tile_container = (Node3D)GetNode("TileContainer");
		primed_overlay = (PrimedOverlay)GetNode("PrimedOverlay");
		//if World Loader passes a World, Load World Here
		//Else
		world = new WorldResource();
		Init_Test();
		//DebugDisplayContent();
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
			AddTileMultiple(hexes);
		}

	}

	private void DebugDisplayContent()
	{
		if (world.tiles.Count == 0) return;
		foreach (Vector3 key in world.tiles.Keys)
		{
			Debug.WriteLine(key.ToString() + " : " + world.tiles[key]);
		}
	}

	private void _base_add_tile(Hex h)
	{
		Tile _tile = (Tile)test_tile.Instantiate();
		tile_container.AddChild(_tile);
		_tile.Position = grid.layout.GridToWorldspace(h);
		world.tiles.Add(h.ToVector3(), _tile.type.ToString());
	}

	private void AddTileSingle(Hex h)
	{
		if (!world.tiles.ContainsKey(h.ToVector3()))
		{
			_base_add_tile(h);
			UpdatePrimedSites();
		}
	}

	private void AddTileMultiple(List<Hex> hexes)
	{
		foreach (Hex h in hexes)
		{
			if (!world.tiles.ContainsKey(h.ToVector3()))
			{
				_base_add_tile(h);
			}
		}

		UpdatePrimedSites();
	}



	private void UpdatePrimedSites()
	{
		List<Hex> _sites = new List<Hex>();

		foreach (Vector3I v in world.tiles.Keys)
		{
			Hex h = new Hex(v);
			Hex[] _adjacents = h.Adjacents();

			foreach (Hex j in _adjacents)
			{
				if (!world.tiles.ContainsKey(j.ToVector3())	&& !_sites.Contains(j))  _sites.Add(j);
			}	
		}
		
		primed_sites =_sites;
		primed_overlay.UpdateOverlay(primed_sites);

	}
}
