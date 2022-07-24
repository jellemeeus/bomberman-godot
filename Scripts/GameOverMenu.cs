using Godot;
using System;

public class GameOverMenu : MarginContainer
{
    // Called when the node enters the scene tree for the first time.
    private Label selector1;
    private Label selector2;
    private int _currentSelection = 0;

    private Label titleLabel;


    public override void _Ready()
    {
        // Init select labels
        selector1 = GetNode<Label>("./CenterContainer/VBoxContainer/CenterContainer2/VBoxContainer/CenterContainer/HBoxContainer/Selector");
        selector2 = GetNode<Label>("./CenterContainer/VBoxContainer/CenterContainer2/VBoxContainer/CenterContainer1/HBoxContainer/Selector");
        titleLabel = GetNode<Label>("./CenterContainer/VBoxContainer/#CenterContainer/Label");
        SetCurrentSelection(_currentSelection);
    }

    public void SetTitle(string title)
    {
        titleLabel.Text = title;
    }

    public void SetCurrentSelection(int index){
        selector1.Text = "";
        selector2.Text = "";
        if (_currentSelection == 0){
            selector1.Text = "-";
        }
        else if (_currentSelection == 1){
            selector2.Text = "-";
        }
    }

    public void handleInput(string ui_action)
    {
        const int nElements = 2;
        if (ui_action ==("ui_down")){
            _currentSelection = (_currentSelection + 1) % nElements;
            SetCurrentSelection(_currentSelection);
        }
        else if (ui_action ==("ui_up")){
            _currentSelection = _currentSelection == 0 ? nElements-1 : _currentSelection - 1; 
            SetCurrentSelection(_currentSelection);
        }
    }
    public string ParseSelection(){
        if (_currentSelection == 0){
            return "play_again";
        }
        else if (_currentSelection == 1){
            return "quit";
        }
        else {
            return "invalid";
        }
    }
}
