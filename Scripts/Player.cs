using Godot;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
    [Signal]
    public delegate void collectedPrize(string playerName);

    [Signal]
    public delegate void playerDied(string playerName);

    public bool isDead = false;
    public bool isReadyToRespawn;

    public int moveSpeed = 50;
    private int _moveSpeedLimit = 320;
    private int _health = 100;

    protected AnimationTree _animTree;
    protected AnimationNodeStateMachinePlayback _animStateMachine;
    protected Vector2 _movement = new Vector2();
    protected Vector2 _direction = new Vector2();

    private const string _BombResource = "res://Nodes/Bomb.tscn";
    private PackedScene _packedSceneBomb;

    public int amountOfBombs = 1;
    public int bombPowerUp = 0;
    public int flamePowerUp = 0;
    public int speedPowerUpValue = 20; // How many units should add per powerUp
    public int flamePowerUpValue = 1;
    public int bombPowerUpValue = 1;

    protected bool _isInvincible = true;
    protected AnimatedSprite _sprite;
    private bool _isFlickerOn = true;

    public bool isImmortal = false; // if set Die() does nothing 
    public bool isCollectPrize = false; // if set Prize() does nothing

    protected Timer _timer_invincibility;

    public string name = "default";
    
    public Color color = new Color(1.0f,1.0f,1.0f, 1.0f);
    

    public void Init(string name)
    {
        this.name = name;
    }

    protected void SetColor(Color color){
        _sprite.Modulate = color;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _animTree = (AnimationTree)GetNode("AnimationTree");
        _animStateMachine = (AnimationNodeStateMachinePlayback)_animTree.Get("parameters/playback");

        _packedSceneBomb = ResourceLoader.Load<PackedScene>(_BombResource);
        Timer timer_reload = GetNode<Timer>("Regenerate");
        timer_reload.Connect("timeout", this, "Regenerate");
        _timer_invincibility = GetNode<Timer>("Invincibility");
        _timer_invincibility.Connect("timeout", this, "RemoveInvincibility");
        Timer timer_invincibility_flicker = GetNode<Timer>("InvincibilityFlicker");
        timer_invincibility_flicker.Connect("timeout", this, "Flicker");
        _sprite = (AnimatedSprite)this.GetNode("./AnimatedSprite");
        SetColor(color);
    }


    private void Regenerate(){
        if (amountOfBombs < ( 1 + (bombPowerUp * bombPowerUpValue))){
            amountOfBombs++;
        }
        if (_health < 100){
            _health += 10;
        }
    }

    private void Flicker()
    {
        if (_isInvincible)
        {
            if (_isFlickerOn)
            {
                _sprite.Modulate = new Color(_sprite.Modulate.r, _sprite.Modulate.g, _sprite.Modulate.b, 0.5f);
                _isFlickerOn = false;
            }
            else
            {
                _sprite.Modulate = new Color(_sprite.Modulate.r, _sprite.Modulate.g, _sprite.Modulate.b, 1f);
                _isFlickerOn = true;
            }
        }
        else
        {
            if (!_isFlickerOn)
            {
                _isFlickerOn = true;
                _sprite.Modulate = new Color(_sprite.Modulate.r, _sprite.Modulate.g, _sprite.Modulate.b, 1f);
            }
        }
    }

    public void ApplyInvincibility(float duration){
        _isInvincible = true;
        _timer_invincibility.Start(duration);
    }

    private void RemoveInvincibility(){
        _isInvincible = false;
    }

    private void Die(){
        if (isImmortal){return;};
        isDead = true;
        _isInvincible = true;
        isReadyToRespawn = false;
        EmitSignal("playerDied", name);
        // PlayDeath animation
        // set isReadyToRespawn to true somehwere
        isReadyToRespawn = true;
    }

    public void Respawn(){
        isDead = false;
        ApplyInvincibility(15.0f);
    }

    protected void HitByFire(){
        _health = _health - 100;
        if (_health <= 0){
            Die();
        }
        else {
            ApplyInvincibility(5.0f);
        }
    }

    private void Prize(){
        if (!isCollectPrize){return;};
        _isInvincible = true;
        EmitSignal("collectedPrize", name);
    }

//    public override void _Input(InputEvent @event)
//    {
//        // Mouse in viewport coordinates.
//        if (@event is InputEventMouseButton eventMouseButton)
//        {
//            //            GD.Print("Mouse Click/Unclick at: ", eventMouseButton.Position);
//            //            GD.Print("Viewport Resolution is: ", GetViewportRect().Size);
//            //            GD.Print("MousePositon is: ", GetViewport().GetMousePosition());
//            GD.Print("Direction Mouse: ", Position.DirectionTo(eventMouseButton.Position).Normalized());
//            _animTree.Set("parameters/Idle/blend_position", Position.DirectionTo(eventMouseButton.Position).Normalized());
//            _animTree.Set("parameters/Walk/blend_position", Position.DirectionTo(eventMouseButton.Position).Normalized());
//        }
//        else if (@event is InputEventMouseMotion eventMouseMotion)
//        {
//           // _animTree.Set("parameters/Idle/blend_position", Position.DirectionTo(eventMouseMotion.Position).Normalized());
//            // _animTree.Set("parameters/Walk/blend_position", Position.DirectionTo(eventMouseButton.Position).Normalized());
//            //            GD.Print("Mouse Motion at: ", eventMouseMotion.Position);
//            //           GD.Print("Mouse Motion at RELATIVE: ", eventMouseMotion.Relative);
//        }
//        // Print the size of the viewport.
//    }


    private void _on_PlayerArea2D_PickedUpPrize(){
        Prize();
    }

    private void _on_PlayerArea2D_PickedUpPowerUp(string typeOfPowerUp){
        if (typeOfPowerUp == "Powerup_bomb")
        {
            amountOfBombs++;
            bombPowerUp++;
        }
        else if (typeOfPowerUp == "Powerup_flame")
        {
            flamePowerUp++;
        }
        else if (typeOfPowerUp == "Powerup_speed")
        {
            moveSpeed = Math.Min(moveSpeed + speedPowerUpValue, _moveSpeedLimit);
        }
    }

    protected bool _TryPlaceBomb(){
        Bomb newBomb = _packedSceneBomb.Instance() as Bomb;
        newBomb.Init(1 + (flamePowerUp * flamePowerUpValue));
        // Change position to center
        Vector2 centeredPosition = new Vector2();
        centeredPosition.x = (float)(Math.Round(Position.x  / 32) * 32);
        centeredPosition.y = (float)(Math.Round(Position.y  / 32) * 32);
        // Check if there already is a Bomb on center position
        List<Bomb> bombs = GetTree().Root.GetChildren().OfType<Bomb>().ToList();
        foreach(Bomb bomb in bombs){
            if (bomb.Position == centeredPosition){
                return false;
            }
        }
        newBomb.Position = centeredPosition;
        GetTree().Root.AddChild(newBomb);
        return true;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (isDead){ return; }
        _movement.x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        _movement.y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
        _direction = _movement.Normalized();
        var possibleCollision = MoveAndCollide(_direction * moveSpeed * delta);

        // Update animation tree based on direction
        string animStateAnimation = _direction == Vector2.Zero ? "Idle" : "Walk";
        _animStateMachine.Travel(animStateAnimation);
        if (animStateAnimation == "Walk"){
            _animTree.Set("parameters/Idle/blend_position", _direction);
            _animTree.Set("parameters/Walk/blend_position", _direction);
        }

        // Check collisions
        if (possibleCollision != null){
            KinematicCollision2D collision = (KinematicCollision2D)possibleCollision;
            Node collider = (Node)collision.Collider;
            if (collider.Name.StartsWith("@Flame")){
                if (!(_isInvincible)){
                    HitByFire();
                }
            }
        }

        // Place bomb if key pressed or held down
        if (Input.IsActionPressed("place_bomb") && amountOfBombs > 0)
        {
            if (_TryPlaceBomb()){
                amountOfBombs--;
            }
        }
    }
}
