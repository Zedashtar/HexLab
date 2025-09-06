using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


//public partial class HexUtilities : RefCounted
namespace HexUtilities
{
	#region Hex
	public struct Hex
	{

		public int q;
		public int r;
		public int s;

		public Hex(int _q, int _r, int _s)
		{
			q = _q;
			r = _r;
			s = _s;
			if (q + r + s != 0) throw new ArgumentException("q + r + s must be 0");
		}

		public Hex(int _q, int _r)
		{
			q = _q;
			r = _r;
			s = -_q - _r;
		}

		public Hex() { } //Parameterless constructor for deserialization

		public Hex(Vector3 v)
		{
			q = (int)v.X;
			r = (int)v.Y;
			s = (int)v.Z;
			if (q + r + s != 0) throw new ArgumentException("q + r + s must be 0");
		}

		public static Hex operator +(Hex a, Hex b)
		{
			return new Hex(a.q + b.q, a.r + b.r, a.s + b.s);
		}

		public static Hex operator -(Hex a, Hex b)
		{
			return new Hex(a.q - b.q, a.r - b.r, a.s - b.s);
		}

		public static Hex operator *(Hex a, int k)
		{
			return new Hex(a.q * k, a.r * k, a.s * k);
		}

		public static bool operator ==(Hex a, Hex b)
		{
			return a.q == b.q && a.r == b.r && a.s == b.s;
		}

		public static bool operator !=(Hex a, Hex b)
		{
			return !(a == b);
		}

		public Hex Add(Hex b)
		{
			return new Hex(q + b.q, r + b.r, s + b.s);
		}

		public Hex Subtract(Hex b)
		{
			return new Hex(q - b.q, r - b.r, s - b.s);
		}


		public Hex Scale(int k)
		{
			return new Hex(q * k, r * k, s * k);
		}

		public Hex Negate()
		{
			return new Hex(-q, -r, -s);
		}


		static public Hex[] hex_directions = new Hex[] { new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1), new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1) };

		static public Hex Direction(int direction)
		{
			return hex_directions[(6 + (direction % 6)) % 6];
		}

		public Hex Neighbor(int direction)
		{
			return Add(Hex.Direction(direction));
		}


		public Hex[] Adjacents()
		{
			Hex[] adjacent = new Hex[6];
			for (int i = 0; i < 6; i++)
			{
				adjacent[i] = Neighbor(i);
			}
			return adjacent;
		}

		public int Length()
		{
			return (int)((Math.Abs(q) + Math.Abs(r) + Math.Abs(s)) / 2);
		}

		public int Distance(Hex b)
		{
			return Subtract(b).Length();
		}

		public enum RotateDirection { Clockwise, CounterClockwise }

		public Hex Rotate(RotateDirection direction) //Always rotate around the origin
		{
			switch (direction)
			{
				case RotateDirection.Clockwise:
					return new Hex(-r, -s, -q);
				case RotateDirection.CounterClockwise:
					return new Hex(-s, -q, -r);
				default:
					throw new ArgumentException("Invalid rotation direction");
			}
		}

		public Hex RotateAround(Hex center, RotateDirection direction)
		{
			return Subtract(center).Rotate(direction).Add(center);
		}


		public Hex reflectQ() { return new Hex(q, s, r); }
		public Hex reflectR() { return new Hex(s, r, q); }
		public Hex reflectS() { return new Hex(r, q, s); }

		public override String ToString()
		{
			return "(" + q + ", " + r + ", " + s + ")";
		}

		public Vector3I ToVector3()
		{
			return new Vector3I(q, r, s);
		}



	}
	#endregion
	#region FractionalHex
	struct FractionalHex
	{
		public FractionalHex(double q, double r, double s)
		{
			this.q = q;
			this.r = r;
			this.s = s;
			if (Math.Round(q + r + s) != 0) throw new ArgumentException("q + r + s must be 0");
		}
		public readonly double q;
		public readonly double r;
		public readonly double s;

		public Hex HexRound()
		{
			int qi = (int)(Math.Round(q));
			int ri = (int)(Math.Round(r));
			int si = (int)(Math.Round(s));
			double q_diff = Math.Abs(qi - q);
			double r_diff = Math.Abs(ri - r);
			double s_diff = Math.Abs(si - s);
			if (q_diff > r_diff && q_diff > s_diff)
			{
				qi = -ri - si;
			}
			else if (r_diff > s_diff)
			{
				ri = -qi - si;
			}
			else
			{
				si = -qi - ri;
			}
			return new Hex(qi, ri, si);
		}

	}

	#endregion

	#region Orientation
	public struct Orientation
	{
		public Orientation(double f0, double f1, double f2, double f3, double b0, double b1, double b2, double b3, double start_angle)
		{
			// f for Forward, b for Backward (inverse matrix)
			this.f0 = f0;
			this.f1 = f1;
			this.f2 = f2;
			this.f3 = f3;
			this.b0 = b0;
			this.b1 = b1;
			this.b2 = b2;
			this.b3 = b3;
			this.start_angle = start_angle;
		}
		public readonly double f0;
		public readonly double f1;
		public readonly double f2;
		public readonly double f3;
		public readonly double b0;
		public readonly double b1;
		public readonly double b2;
		public readonly double b3;
		public readonly double start_angle;
	}
	#endregion
	#region Layout
	public struct Layout
	{
		public Layout(Orientation orientation, Vector2 size, Vector3 origin)
		{
			this.orientation = orientation;
			this.tile_size = size;
			this.worldspace_origin = origin;
		}

		public readonly Orientation orientation;
		public readonly Vector2 tile_size;
		public readonly Vector3 worldspace_origin;
		static public Orientation pointy = new Orientation(Math.Sqrt(3.0), Math.Sqrt(3.0) / 2.0, 0.0, 3.0 / 2.0, Math.Sqrt(3.0) / 3.0, -1.0 / 3.0, 0.0, 2.0 / 3.0, 0.5);
		static public Orientation flat = new Orientation(3.0 / 2.0, 0.0, Math.Sqrt(3.0) / 2.0, Math.Sqrt(3.0), 2.0 / 3.0, 0.0, -1.0 / 3.0, Math.Sqrt(3.0) / 3.0, 0.0);


		public Vector3 GridToWorldspace(Hex h)
		{
			Orientation M = orientation;
			double x = (M.f0 * h.q + M.f1 * h.r) * tile_size.X;
			double z = (M.f2 * h.q + M.f3 * h.r) * tile_size.Y;
			return new Vector3((float)x + worldspace_origin.X, worldspace_origin.Y, (float)z + worldspace_origin.Z);
		}

		public Hex WorldspaceToGrid(Vector3 p)
		{
			Orientation M = orientation;
			Vector3 pt = new Vector3((p.X - worldspace_origin.X) / tile_size.X, worldspace_origin.Y, (p.Z - worldspace_origin.Z) / tile_size.Y);
			double q = M.b0 * pt.X + M.b1 * pt.Z;
			double r = M.b2 * pt.X + M.b3 * pt.Z;
			return new FractionalHex(q, r, -q - r).HexRound();
		}



	}
	#endregion

	#region Patterns

	public static class PatternUtility
	{

		static public List<Hex> Rotate(List<Hex> pattern, Hex.RotateDirection direction)
		{
			List<Hex> results = new List<Hex>();
			results = pattern.Select(h => h.Rotate(Hex.RotateDirection.Clockwise)).ToList();
			return results;
		}

		static public List<Hex> Translate(List<Hex> pattern, Hex direction)
		{
			List<Hex> results = new List<Hex>();
			results = pattern.Select(h => h.Add(direction)).ToList();
			return results;
		}




		static public List<Hex> GenerateHexRing(int radius)
		{
			Hex center = new Hex(0, 0, 0);
			List<Hex> results = new List<Hex>();
			if (radius == 0)
			{
				results.Add(center);
				return results;
			}
			Hex hex = center.Add(Hex.Direction(4).Scale(radius));
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < radius; j++)
				{
					results.Add(hex);
					hex = hex.Neighbor(i);
				}
			}
			return results;
		}

		static public List<Hex> GenerateHexSpiral(int radius)
		{
			Hex center = new Hex(0, 0, 0);
			List<Hex> results = new List<Hex>();
			results.Add(center);
			for (int k = 1; k <= radius; k++)
			{
				results.AddRange(GenerateHexRing(k));
			}
			return results;
		}

	}



	#endregion
}
