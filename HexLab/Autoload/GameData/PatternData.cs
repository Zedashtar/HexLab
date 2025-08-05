using System;
using Godot;
using HexUtilities;
using Newtonsoft.Json;


public partial class PatternData : Node
{
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
        GD.Print(json.GetLength());
        if (json == null)
        {
            GD.PrintErr("Failed to open patterns.json file.");
            GD.PrintErr(FileAccess.GetOpenError());
            return;
        }

        string pattern_data = json.GetAsText();
        GD.Print(pattern_data);
        Data = JsonConvert.DeserializeObject<Hex[][][]>(pattern_data);

        json.Close();
    }
}
