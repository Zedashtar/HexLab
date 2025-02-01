using Godot;
using System;
using HexUtilities;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;



public partial class HexGrid : Node3D
{

	private enum _orient { Pointy, Flat }
	public Layout layout;
	public Hex mouse_hexPosition;

	[ExportGroup("Layout")]
	[Export] private _orient orientation = _orient.Flat;
	[Export] private Vector2 tile_size = new Vector2(1, 1);

	[ExportGroup("Testing")]
	[Export] private PackedScene test_tile;
	[Export] private Label coordinate_display;




    public override void _EnterTree()
    {
        base._EnterTree();
		HexUtilities.Orientation _o = orientation == _orient.Flat ? Layout.flat : Layout.pointy;
		layout = new Layout(_o, tile_size, Position);

    } 

	

	public override void _Ready()
	{
		Init_Test();
	}

	public override void _Process(double delta)
	{
		GetMousePosition();
	}


	private void Init_Test()
	{
		if (test_tile != null)
		{
			List<Hex> hexes = new List<Hex> {new Hex(layout.worldspace_origin)};
			hexes.AddRange(hexes[0].Adjacents());
			foreach (Hex h in hexes)
			{
				Debug.WriteLine(h.q + " " + h.r + " " + h.s);
				Node3D _tile = (Node3D)test_tile.Instantiate();
				AddChild(_tile);
				_tile.Position = layout.GridToWorldspace(h);
			}
		}

	}

	private void GetMousePosition()
	{
		Vector2 mouse_position = GetViewport().GetMousePosition();
		Plane grid_plane = new Plane(Vector3.Up, layout.worldspace_origin.Y);
		Vector3 world_pos = (Vector3)grid_plane.IntersectsRay(GetViewport().GetCamera3D().ProjectRayOrigin(mouse_position), GetViewport().GetCamera3D().ProjectRayNormal(mouse_position));
		mouse_hexPosition = layout.WorldspaceToGrid(world_pos);
		coordinate_display.Text = mouse_hexPosition.ToString();
	}



}

