using Godot;
using System;

public class Player : KinematicBody2D
{

    [Export]
    private Vector2 motion = new Vector2(0, 100);

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
      MoveAndCollide(motion * delta);
    }
}
