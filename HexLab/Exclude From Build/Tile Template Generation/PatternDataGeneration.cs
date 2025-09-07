using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HexUtilities;
using Newtonsoft.Json;


public partial class PatternDataGeneration : Node
{

    [Export] int max_generation_size = 3;
    [Export] PatternEditor pattern_editor;
    [Export] PopupWindowYN popup_window;
    Layout layout = new Layout(Layout.flat, new Vector2(0.5f, 0.5f), Vector3.Zero);

    [Signal] public delegate void GenerationCompleteEventHandler();
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


    public void StartGeneration()
    {
        Data = pattern_editor.savedPatterns.Data;

        if (Data == null || Data.Length == 0)
        {
            GD.Print("No existing pattern data found. Initializing with base patterns.");
            Data = patterns_base;

        }


        GD.Print("-------------------");
        GD.Print("Starting Full Generation up to size [" + max_generation_size + "]");


        //Ensure Data array is large enough to hold patterns up to max_generation_size
        if (Data.Length - 1 < max_generation_size)
        {
            GD.Print("pattern_data max reccorded pattern size is  [" + (Data.Length - 1) + "] is less than max generation size [" + max_generation_size + "]. Extending array.");
            Hex[][][] array_extension = Enumerable.Repeat(new Hex[0][], max_generation_size - Data.Length + 1).ToArray();
            Data = Data.Concat(array_extension).ToArray();
            GD.Print("Array extended to max pattern size [" + (Data.Length - 1) + "]");

        }

        for (int n = 2; n < max_generation_size; n++)
        {
            patternBuilder(n);
        }

        GD.Print("Full Generation Complete");
        GD.Print("Saving to Json File");
        pattern_editor.SavePatternsToFile(Data);
        GD.Print("Updating Autoload Data");
        pattern_editor.savedPatterns.LoadPatternsFromFile();
        GD.Print("-------------------");
        EmitSignal(SignalName.GenerationComplete);
    }

    void patternBuilder(int _n)
    {
        GD.Print("-------------------");
        GD.Print("Starting Generation for size [" + (_n + 1) + "]");

        List<Hex[]> output = new List<Hex[]>();
        foreach (Hex[] pattern in Data[_n])
        {
            output = output.Concat(NplusOne_Pattern(pattern)).ToList();
        }

        output = GenerationCleanup(output);


        GD.Print("Generated " + output.Count + " new patterns of size [" + (_n + 1) + "]");
        foreach (Hex[] pattern in output)
        {
            GD.Print("Pattern: " + String.Join(", ", pattern.Select(h => h.ToString())));
        }
        GD.Print("-------------------");

        List<Hex[][]> new_data = Data.ToList();

        Data[_n + 1] = output.ToArray();
    }

    #region Helper Functions
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


    List<Hex[]> NplusOne_Pattern(Hex[] _pattern)
    {
        List<Hex[]> n_plus_one_patterns = new List<Hex[]>();



        //Generate all possible n+1 patterns by adding one adjacent hex to the existing pattern
        List<Hex> _pattern_adjacents = N_AdjacencyList(_pattern);
        int _n = _pattern.Length;
        foreach (Hex hex in _pattern_adjacents)
        {
            List<Hex> new_pattern = _pattern.ToList();
            new_pattern.Add(hex);

            // Center the pattern around the barycenter
            List<Hex> centered_pattern = PatternUtility.Center(new_pattern);

            n_plus_one_patterns.Add(centered_pattern.ToArray());


        }

        n_plus_one_patterns = GenerationCleanup(n_plus_one_patterns);

        return n_plus_one_patterns;

    }

    List<Hex[]> GenerationCleanup(List<Hex[]> _pattern_list)
    {
        _pattern_list = RemoveDuplicates(_pattern_list);
        _pattern_list = RemoveIBM(_pattern_list);
        _pattern_list = RemoveIBR(_pattern_list);

        // When centering patterns, and the calculated barycenter is exactly between two hexes, the result of the centering is inconsistent.
        // This can lead to patterns that are idantical but offset by one hex and not being removed by the other cleanup functions.
        // Remove IBT calculates all possible translations and runs the other cleanup functions.

        _pattern_list = RemoveIBT(_pattern_list);

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
        GD.Print("Removed " + (_patterns.Count - unique_patterns.Count) + " duplicates.");
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


        GD.Print("Removed " + (_patterns.Count - unique_patterns.Count) + " IBM.");
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

        GD.Print("Removed " + (_patterns.Count - unique_patterns.Count) + " IBR.");
        return unique_patterns;
    }

    List<Hex[]> RemoveIBT(List<Hex[]> _patterns) //Remove Identical by Translation, also needs to rerun all removes functions
    {
        List<Hex[]> unique_patterns = new List<Hex[]>();
        for (int i = _patterns.Count - 1; i >= 0; i--)
        {
            if (_patterns.Slice(0, i) == null) { break; }
            if (!HasIBT(_patterns[i], _patterns.Slice(0, i))) { unique_patterns.Add(_patterns[i]); }
        }

        unique_patterns.Reverse();

        GD.Print("Removed " + (_patterns.Count - unique_patterns.Count) + " IBT.");
        return unique_patterns;
    }



    #endregion

    #region Condition Checkers

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

    bool HasIBT(Hex[] _target_pattern, List<Hex[]> _list_to_check)
    {
        List<Hex[]> target_translations =
        [
            PatternUtility.Translate(_target_pattern.ToList(), new Hex(1, -1, 0)).ToArray(), // +S
            PatternUtility.Translate(_target_pattern.ToList(), new Hex(0, -1, 1)).ToArray(), // +Q
            PatternUtility.Translate(_target_pattern.ToList(), new Hex(-1, 0, 1)).ToArray(), // +R
            PatternUtility.Translate(_target_pattern.ToList(), new Hex(-1, 1, 0)).ToArray(), // -S
            PatternUtility.Translate(_target_pattern.ToList(), new Hex(0, 1, -1)).ToArray(), // -Q
            PatternUtility.Translate(_target_pattern.ToList(), new Hex(1, 0, -1)).ToArray(), // -R
        ];

        foreach (Hex[] pattern in target_translations)
        {
            if (HasDuplicate(pattern, _list_to_check))
                return true;
        }

        foreach (Hex[] pattern in target_translations)
        {
            if (HasIBM(pattern, _list_to_check))
                return true;
        }

        foreach (Hex[] pattern in target_translations)
        {
            if (HasIBR(pattern, _list_to_check))
                return true;
        }
        return false;




    }


    #endregion

    #region Signal Handlers

    void _set_generation_size(float _size)
    {
        max_generation_size = (int)_size;
    }
    
    void _on_start_generation_button_down()
    {
        popup_window.Open();
    }

    
    #endregion


}