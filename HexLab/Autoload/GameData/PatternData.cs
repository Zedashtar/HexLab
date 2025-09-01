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

        return pattern_adjacents;

    }


    void NplusOne_Pattern(Hex[] _pattern)
    {
        GD.Print("-------------------");
        GD.Print("Starting Generation");
        List<Hex[]> n_plus_one_patterns = new List<Hex[]>();



        //Generate all possible n+1 patterns by adding one adjacent hex to the existing pattern
        List<Hex> _pattern_adjacents = N_AdjacencyList(_pattern);
        int _n = _pattern.Length;
        foreach (Hex hex in _pattern_adjacents)
        {
            List<Hex> new_pattern = _pattern.ToList();
            new_pattern.Add(hex);



            Hex bary_hex = CalculateBarycenter(new_pattern);

            // Center the pattern around the barycenter
            List<Hex> centered_pattern = new_pattern.Select(h => h.Subtract(bary_hex)).ToList();

            n_plus_one_patterns.Add(centered_pattern.ToArray());


        }

        n_plus_one_patterns = GenerationCleanup(n_plus_one_patterns);


        GD.Print("Generated " + n_plus_one_patterns.Count + " new patterns of size [" + (_n + 1) + "]");
        foreach (Hex[] pattern in n_plus_one_patterns)
        {
            GD.Print("Pattern: " + String.Join(", ", pattern.Select(h => h.ToString())));
        }
        GD.Print("-------------------");

    }


    Hex CalculateBarycenter(List<Hex> _pattern)
    {
        // Convert Hex to worldspace positions
        List<Vector3> worldspace_positions = new List<Vector3>();
        foreach (Hex h in _pattern)
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

        return bary_hex;
        
    }

    List<Hex[]> GenerationCleanup(List<Hex[]> _pattern_list)
    {
        _pattern_list = RemoveDuplicates(_pattern_list);
        _pattern_list = RemoveIBM(_pattern_list);
        _pattern_list = RemoveIBR(_pattern_list);

        return _pattern_list;
    }

    List<Hex[]> RemoveDuplicates(List<Hex[]> _patterns) //Remove exact duplicates in the same list
    {
        List<Hex[]> unique_patterns = new List<Hex[]>();
        for (int i = _patterns.Count - 1; i >= 0; i--)
        {
            if (_patterns.Slice(0, i) == null) { break; }
            if (!HasDuplicate(_patterns[i], _patterns.Slice(0, i))) { unique_patterns.Add(_patterns[i]); }
        }
        
        unique_patterns.Reverse();
        GD.Print("Removed "+ (_patterns.Count - unique_patterns.Count) + " duplicates.");
        return unique_patterns;
    }

    List<Hex[]> RemoveIBM(List<Hex[]> _patterns) //Remove Identical by Mirror Symetry in the same list
    {
        List<Hex[]> unique_patterns = new List<Hex[]>();
        for (int i = _patterns.Count - 1; i >= 0; i--)
        {
            if (_patterns.Slice(0, i) == null) { break; }
            if (!HasIBM(_patterns[i], _patterns.Slice(0, i))) { unique_patterns.Add(_patterns[i]); }
        }
        
        unique_patterns.Reverse();
        
        GD.Print("Removed "+ (_patterns.Count - unique_patterns.Count) + " IBM.");
        return unique_patterns;
    }

    List<Hex[]> RemoveIBR(List<Hex[]> _patterns) //Remove Identical by Rotation in the same list
    {
        List<Hex[]> unique_patterns = new List<Hex[]>();
        for (int i = _patterns.Count - 1; i >= 0; i--)
        {
            if (_patterns.Slice(0, i) == null) { break; }
            if (!HasIBR(_patterns[i], _patterns.Slice(0, i))) { unique_patterns.Add(_patterns[i]); }
        }
        
        unique_patterns.Reverse();
        
        GD.Print("Removed "+ (_patterns.Count - unique_patterns.Count) + " IBR.");
        return unique_patterns;
    }

    bool HasDuplicate(Hex[] _target_pattern, List<Hex[]> _list_to_check)
    {
        return _list_to_check.Any(p => p.Length == _target_pattern.Length && p.All(h => _target_pattern.Contains(h)));
    }

    bool HasIBM(Hex[] _target_pattern, List<Hex[]> _list_to_check)
    {
        return (_list_to_check.Any(p => p.Length == _target_pattern.Length && p.All(h => _target_pattern.Contains(h.reflectQ())) ||
                                        p.Length == _target_pattern.Length && p.All(h => _target_pattern.Contains(h.reflectR())) ||
                                        p.Length == _target_pattern.Length && p.All(h => _target_pattern.Contains(h.reflectS())) ||
                                        p.Length == _target_pattern.Length && p.All(h => _target_pattern.Contains(h.Negate())) ||
                                        p.Length == _target_pattern.Length && p.All(h => _target_pattern.Contains(h.reflectQ().Negate())) ||
                                        p.Length == _target_pattern.Length && p.All(h => _target_pattern.Contains(h.reflectR().Negate())) ||
                                        p.Length == _target_pattern.Length && p.All(h => _target_pattern.Contains(h.reflectS().Negate()))
                                         ));

    }

    bool HasIBR(Hex[] _target_pattern, List<Hex[]> _list_to_check)
    {

        List<Hex[]> target_rotations = new List<Hex[]>();
        
        // Generate all 5 rotations of the target pattern
        for (int i = 1; i < 6; i++)
        {
            int rotation_index = i;
            int j = 0;
            Hex[] rotated_pattern = _target_pattern;
            while (j < rotation_index)
            {
                rotated_pattern = rotated_pattern.Select(h => h.Rotate(Hex.RotateDirection.Clockwise)).ToArray();
                j++;
            }
            target_rotations.Add(rotated_pattern);
        }


        // Check if any rotation of the target pattern exists in the list
        foreach (Hex[] rotated_pattern in target_rotations)
        {
            if (_list_to_check.Any(p => p.Length == rotated_pattern.Length && p.All(h => rotated_pattern.Contains(h))))
            { return true; }
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
