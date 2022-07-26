using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class MainMenuState : GameManagerState
{
    private MainMenu _menu;
    private Node2D _menuAnimation;

    public override void OnStart(Dictionary<string, object> message)
    {
        base.OnStart(message);

        foreach(Control c in GetNode("../../../../Game/CanvasLayerMainMenu").GetChild(0).GetChildren().OfType<Control>().ToList<Control>())
        {
            GD.Print("control ", c.Name);
            c.Show();
        }
    }

    public override void OnExit(string nextState)
    {
        base.OnExit(nextState);

        foreach(Control c in GetNode("../../../../Game/CanvasLayerMainMenu").GetChild(0).GetChildren().OfType<Control>().ToList<Control>())
        {
            GD.Print("control ", c.Name);
            c.Hide();
        }

        _menuAnimation.QueueFree();
    }


    public override void _Ready(){
        base._Ready();
        _menu = GetNode<MainMenu>("../../../../Game/CanvasLayerMainMenu/MainMenu");
        _menuAnimation = ResourceLoader.Load<PackedScene>("res://Nodes/Menus/MainMenuAnimation.tscn").Instance() as Node2D;
        GetTree().Root.AddChild(_menuAnimation);
    }

    public void ChangeToDebugLoopState()
    {
        GM.ChangeState("GameLoopState",
        new Dictionary<string, object>(){
            {"map_resource", "res://Scenes/DBG.tscn"}, 
            {"player_resource", "res://Nodes/Player.tscn"},
            {"bot_resource", "res://Nodes/PlayerBot.tscn"},
            {"prize_resource", "res://Nodes/Prize.tscn"},
            {"special_type", "DBG"},
// TODO: Give color value of player
        });
    }
    public void ChangeToGameLoopState()
    {
        GM.ChangeState("GameLoopState",
        new Dictionary<string, object>(){
            {"map_resource", "res://Scenes/Demo.tscn"},
            {"player_resource", "res://Nodes/Player.tscn"},
            {"bot_resource", "res://Nodes/PlayerBot.tscn"},
            {"prize_resource", "res://Nodes/Prize.tscn"},
            {"special_type", "immortal"},
// TODO: Give color value of player
        });
    }

    public override void UpdateState(float delta)
    {
        base.OnUpdate();
        if (Input.IsActionJustPressed("ui_down")){
            GD.Print("down");
            _menu.handleInput("ui_down");
        }
        else if (Input.IsActionJustPressed("ui_up")){
            GD.Print("up");
            _menu.handleInput("ui_up");
        }
        else if (Input.IsActionJustPressed("ui_accept")){
            GD.Print("select");
            string action = _menu.ParseSelection();
            if (action == "new_game"){
                ChangeToGameLoopState();
            }
            else if (action == "options"){
                GD.Print("Options");
            }
            else if (action == "dbg"){
                ChangeToDebugLoopState();
            }
            else if (action == "quit"){
                GetTree().Quit();
            }
        }
    }


}
