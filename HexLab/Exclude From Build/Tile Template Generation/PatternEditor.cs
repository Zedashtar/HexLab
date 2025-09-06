using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PatternEditor : CanvasLayer
{

    PatternData savedPatterns;
    [Export] WorldManager worldManager;
    [Export] OptionButton sizeDropdown;
    [Export] OptionButton indexDropdown;

    public int selectedPatternSize = 1;
    int PatternSizeDimension;
    public int selectedPatternIndex = 0;
    int PatternIndexDimension;


    public override void _Ready()
    {
        savedPatterns = GetNode<PatternData>("/root/GameData/Patterns");
        if (savedPatterns == null) { GD.Print("Failed to get PatternData autoload"); return; }

        UpdateDimensions();
    }

    public void UpdateDimensions()
    {
        if (savedPatterns.Data.Length == 0)
        {
            GD.Print("Pattern Data is empty or did not load correctly, returned array length is 0");
            return;
        }

        PatternSizeDimension = savedPatterns.Data.Length - 1;
        PatternIndexDimension = savedPatterns.Data[selectedPatternSize].Length;
        UpdateSizeDropdown();


    }

    private void UpdateSizeDropdown()
    {
        sizeDropdown.Clear();
        for (int i = 0; i < PatternSizeDimension; i++)
        {
            sizeDropdown.AddItem((i + 1).ToString());
        }


        selectedPatternSize = 1;
        sizeDropdown.Select(0);
        UpdateIndexDropdown();


    }

    private void UpdateIndexDropdown()
    {
        PatternIndexDimension = savedPatterns.Data[selectedPatternSize].Length;
        indexDropdown.Clear();
        for (int i = 0; i < PatternIndexDimension; i++)
        {
            indexDropdown.AddItem(i.ToString());
        }

        selectedPatternIndex = 0;
        indexDropdown.Select(0);
        _display_selected_pattern();

    }

    public void NavigatePatternSize(int step)
    {
        int newIndex = (sizeDropdown.Selected + step) % PatternSizeDimension;
        GD.Print("New Index: " + newIndex);
        if (newIndex < 0) { newIndex = PatternSizeDimension + newIndex; } // wrap around to end of list if going negative
        sizeDropdown.Select(newIndex);
        selectedPatternSize = sizeDropdown.Selected + 1; // +1 because pattern sizes start at 1 tile, not 0
        UpdateIndexDropdown();

    }

    public void NavigatePatternIndex(int step)
    {
        int newIndex = (indexDropdown.Selected + step) % PatternIndexDimension;
        if (newIndex < 0) { newIndex = PatternIndexDimension + newIndex; } // wrap around to end of list if going negative
        indexDropdown.Select(newIndex);
        selectedPatternIndex = newIndex;
        _display_selected_pattern();

    }

    void _display_selected_pattern()
    {
        worldManager.ClearTiles();
        worldManager.AddTileMultiple(savedPatterns.Data[selectedPatternSize][selectedPatternIndex].ToList());
    }



    #region Signals Handlers
    void _on_pattern_size_item_selected(int index)
    {
        selectedPatternSize = index + 1; // +1 because pattern sizes start at 1 tile, not 0
        UpdateIndexDropdown();
        _display_selected_pattern();
    }

    void _on_previous_size_button_down()
    {
        NavigatePatternSize(-1);
    }
    void _on_next_size_button_down()
    {
        NavigatePatternSize(1);
    }

    void _on_pattern_index_item_selected(int index)
    {
        selectedPatternIndex = index;
        _display_selected_pattern();
    }
    void _on_previous_index_button_down()
    {
        NavigatePatternIndex(-1);
    }
    void _on_next_index_button_down()
    {
        NavigatePatternIndex(1);
    }
    

    #endregion

}
