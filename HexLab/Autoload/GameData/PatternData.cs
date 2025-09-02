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


    public override void _Ready()
    {
        LoadPatternsFromFile();
    }

    public void LoadPatternsFromFile()
    {

        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr("patterns.json file does not exist at path: " + path);
            return;
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
        GD.Print("Patterns loaded from file.");

    }


}