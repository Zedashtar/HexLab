using Godot;
using System.Collections.Generic;
using HexUtilities;


public partial class WorldManager : TilesManager
{

	public List<Hex> primed_sites = new List<Hex>();
	[Export] private PrimedOverlay primed_overlay;


	private void Init_Test()
	{
		if (test_tile != null)
		{
			List<Hex> hexes = new List<Hex> {new Hex(grid.layout.worldspace_origin)};
			hexes.AddRange(hexes[0].Adjacents());
			AddTileMultiple(hexes);
		}

	}



	public override void AddTileSingle(Hex h)
	{
		base.AddTileSingle(h);
		UpdatePrimedSites();
	}

	public override void AddTileMultiple(List<Hex> hexes)
	{
		base.AddTileMultiple(hexes);
		UpdatePrimedSites();
	}

	public override void ClearTiles()
	{
		base.ClearTiles();
		UpdatePrimedSites();
	}



	private void UpdatePrimedSites()
	{
		List<Hex> _sites = new List<Hex>();

		foreach (Vector3I v in tilemap.tiles.Keys)
		{
			Hex h = new Hex(v);
			Hex[] _adjacents = h.Adjacents();

			foreach (Hex j in _adjacents)
			{
				if (!tilemap.tiles.ContainsKey(j.ToVector3()) && !_sites.Contains(j)) _sites.Add(j);
			}
		}

		primed_sites = _sites;
		primed_overlay.UpdateOverlay(primed_sites);

	}
}
