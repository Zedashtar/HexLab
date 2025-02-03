using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HexUtilities;
using Godot.Collections;

public partial class PrimedOverlay : MultiMeshInstance3D
{
	private HexGrid grid;

	[Export] private float mesh_scale = 1.0f;
	[Export] private float mesh_height = 0.0f;



	public override void _Ready()
	{
		grid = GetNode<HexGrid>("%HexGrid");
		Multimesh.InstanceCount = 0;
		TopLevel = true;
		
	}


	public void UpdateOverlay(List<Hex> _sites)
	{
		Multimesh.InstanceCount = 0;
		Multimesh.InstanceCount = _sites.Count;
		for (int i = 0; i < _sites.Count; i++)
		{
			Transform3D t = new Transform3D(Basis.Identity, Vector3.Zero);
			t.Origin = grid.layout.GridToWorldspace(_sites[i]) + Vector3.Up * mesh_height;
			Multimesh.SetInstanceTransform(i, t.ScaledLocal(Vector3.One * mesh_scale));

			GD.Print(i.ToString() + " : " + Multimesh.GetInstanceTransform(i).Origin.ToString() + " : " + Multimesh.GetInstanceTransform(i).Basis.Scale.ToString());
		}




	}
}
