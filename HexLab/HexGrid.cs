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
	[Export] private Label coordinate_display;




	public override void _EnterTree()
	{
		base._EnterTree();
		HexUtilities.Orientation _o = orientation == _orient.Flat ? Layout.flat : Layout.pointy;
		layout = new Layout(_o, tile_size, Position);

	} 

	

	public override void _Ready()
	{

	}

	public override void _Process(double delta)
	{
		GetMousePosition();
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
