using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HexUtilities;
using Newtonsoft.Json;


public partial class PatternData : Node
{

    Layout layout = new Layout(Layout.flat, new Vector2(0.5f, 0.5f), Vector3.Zero);

    string path = "res://Autoload/GameData/patterns.json";

    public Hex[][][] Data;  // Stores Tiles pattern data with following formatting : Hex[n][p][t] wher
                            // n = number of tiles in the pattern, 
                            // p = number of patterns with n tiles, 
                            // t = tile id in the pattern   


    Hex[][][] patterns_base = [
        [null],

        [[new Hex(0, 0, 0)]],

        [[new Hex(0, 0, 0), new Hex(1, -1, 0)]],

        [[new Hex(0, 0, 0), new Hex(1, -1, 0), new Hex(-1, 1, 0)],
        [new Hex(0, 0, 0), new Hex(-1, 1, 0), new Hex(1, 0, -1)],
        [new Hex(0, 0, 0), new Hex(-1, 1, 0), new Hex(0, -1, 1)]]

        ];

    public override void _Ready()
    {
        //SavePatternsToFile();
        LoadPatternsFromFile();
        NplusOne_Pattern(Data[2][0]);

    }


    public void SavePatternsToFile(string json_string = null)
    {
        if (!Engine.IsEditorHint()) { return; } // Ensure this runs only in the editor and is not modifiable by player

        using var json = FileAccess.Open(path, FileAccess.ModeFlags.WriteRead);

        if (Data == null || json_string == null) { Data = patterns_base; }

        string pattern_data = JsonConvert.SerializeObject(Data, Formatting.Indented);
        json.StoreString(pattern_data);
        json.Close();
        GD.Print("Patterns saved to file.");
    }

    public void LoadPatternsFromFile()
    {

        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr("patterns.json file does not exist at path: " + path);

            if (Engine.IsEditorHint())
            {
                GD.Print("Creating default patterns.json file.");
                SavePatternsToFile();
            }
            else
            {
                GD.PrintErr("Cannot load patterns in runtime, file does not exist and is not created in editor mode.");
                return;
            }

        }

        using var json = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (json == null)
        {
            GD.PrintErr("Failed to open patterns.json file.");
            GD.PrintErr(FileAccess.GetOpenError());
            return;
        }

        string pattern_data = json.GetAsText();
        Data = JsonConvert.DeserializeObject<Hex[][][]>(pattern_data);

        json.Close();
    }


    void PaternBuilder()
    {

    }

    List<Hex> N_AdjacencyList(Hex[] _pattern)
    {
        List<Hex> pattern_adjacents = new List<Hex>();

        foreach (Hex hex in _pattern)
        {
            foreach (Hex adj_hex in hex.Adjacents())
            {
                if (!pattern_adjacents.Contains(adj_hex) && !_pattern.Contains(adj_hex))
                {
                    pattern_adjacents.Add(adj_hex);
                }
            }
        }


        // foreach (Hex hex in pattern_adjacents)
        // {
        //     GD.Print(hex.ToString());
        // }

        return pattern_adjacents;

    }


    void NplusOne_Pattern(Hex[] _pattern)
    {
        List<Hex> _pattern_adjacents = N_AdjacencyList(_pattern);
        int _n = _pattern.Length;
        foreach (Hex hex in _pattern_adjacents)
        {
            List<Hex> new_pattern = _pattern.ToList();
            List<Hex[]> output_patterns_list = new List<Hex[]>();

            new_pattern.Add(hex);


            // Convert Hex to worldspace positions
            List<Vector3> worldspace_positions = new List<Vector3>();
            foreach (Hex h in new_pattern)
            {
                Vector3 worldspace_position = layout.GridToWorldspace(h);
                worldspace_positions.Add(worldspace_position);
            }

            //Calculate Barycenter
            Vector3 barycenter = Vector3.Zero;
            foreach (var pos in worldspace_positions)
            {
                barycenter += pos;
            }
            barycenter /= worldspace_positions.Count;


            //Get Closest Hex to Barycenter
            Hex bary_hex = layout.WorldspaceToGrid(barycenter);

            // Center the pattern around the barycenter
            List<Hex> centered_pattern = new_pattern.Select(h => h.Subtract(bary_hex)).ToList();

            if (!DuplicateCheck(centered_pattern, _n))
            {
                output_patterns_list.Add(centered_pattern.ToArray());
                foreach (Hex[] pattern in output_patterns_list)
                {
                    GD.Print("New Pattern: " + string.Join(", ", pattern.Select(h => h.ToString())));
                }
            }


        }
    }

    bool DuplicateCheck(List<Hex> _pattern, int _n)
    {
        // Check if the pattern already exists in the data
        if (ContentCheck(_pattern, _n)) { return true; }

        if (MirrorCheck(_pattern, _n)) { return true; } // Check for mirrored patterns

        if (RotateCheck(_pattern, _n)) { return true; } // Check for rotated patterns

        

        return false; // If no duplicates, mirrored or rotated patterns found, return false

    }

    bool ContentCheck(List<Hex> _pattern, int _n)
    {
        if (Data[_n].Contains(_pattern.ToArray()) || PermutationCheck(_pattern, _n))
        {
            return true; 
        }
        return false;
    }

    bool MirrorCheck(List<Hex> _pattern, int _n)
    {

        // Check for mirrored patterns
        List<Hex> negated_Base = _pattern.Select(h => h.Negate()).ToList();
        List<Hex> mirrored_patternQ = _pattern.Select(h => h.reflectQ()).ToList();
        List<Hex> negatedQ = mirrored_patternQ.Select(h => h.Negate()).ToList();
        List<Hex> mirrored_patternR = _pattern.Select(h => h.reflectR()).ToList();
        List<Hex> negatedR = mirrored_patternR.Select(h => h.Negate()).ToList();
        List<Hex> mirrored_patternS = _pattern.Select(h => h.reflectS()).ToList();
        List<Hex> negatedS = mirrored_patternS.Select(h => h.Negate()).ToList();

        if (ContentCheck(negated_Base, _n) ||
            ContentCheck(mirrored_patternQ, _n) ||
            ContentCheck(negatedQ, _n) ||
            ContentCheck(mirrored_patternR, _n) ||
            ContentCheck(negatedR, _n) ||
            ContentCheck(mirrored_patternS, _n) ||
            ContentCheck(negatedS, _n))
        {
            return true; // If any mirrored pattern exists, return true
        }

        else return false;

    }

    bool RotateCheck(List<Hex> _pattern, int _n)
    {
        // Check for rotated patterns
        List<Hex> rotated_pattern = _pattern.Select(h => h.Rotate(Hex.RotateDirection.Clockwise)).ToList();
        if (ContentCheck(rotated_pattern, _n)) { return true; }
        if (MirrorCheck(rotated_pattern, _n)) { return true; }

        // Check for all 5 rotations
        for (int i = 0; i < 5; i++)
        {
            rotated_pattern = rotated_pattern.Select(h => h.Rotate(Hex.RotateDirection.Clockwise)).ToList();
            if (ContentCheck(rotated_pattern, _n)) { return true; }
            if (MirrorCheck(rotated_pattern, _n)) { return true; }
        }

        return false;
    }
    
    bool PermutationCheck(List<Hex> _pattern, int _n)
    {
        foreach (Hex[] existing_pattern in Data[_n])
        {
            int count = 0;
            for (int i = 0; i < _pattern.Count; i++)
            {
                if (existing_pattern.Contains(_pattern[i])) { count++; }
            }
            
            if (count == _pattern.Count) { return true; }
            
        }


        return false;
    }

}
