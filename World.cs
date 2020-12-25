using Godot;
using System;

public class World : Node2D
{
    [Export]
    private PackedScene boidScene = ResourceLoader.Load("Boid.tscn") as PackedScene;
    private Boid chosenOne;
    private int boidCount = 200;
    private string groupName = "boids";

    public override void _Ready() {
        base._Ready();
        AddBoids();
        StartTextUpdateTimer(CreateTimer());
        PickChosenOne();
    }

    private void AddBoids() {
        var random = new Random();
        var screensize = GetViewport().Size;
        var rng = new RandomNumberGenerator();
        for (int i = 0; i < boidCount; i++) {
            var boid = boidScene.Instance() as Boid;
            boid.GlobalPosition = new Vector2(rng.RandfRange(0f, screensize.x), rng.RandfRange(0, screensize.y));
            var direction = (random.NextDouble()* (Math.PI * 2))-Math.PI;
            boid.GlobalRotation = (float)direction;
            AddChild(boid);
        }
    }

    private void StartTextUpdateTimer(Timer timer) {
        timer.Connect("timeout", this, "_On_Timer_Timeout");
        timer.WaitTime = 1.0f;
        timer.OneShot = false;
        timer.Start();
    }

    private Timer CreateTimer() {
        var timer = new Timer();
        AddChild(timer);
        return timer;
    }

    private void _On_Timer_Timeout() {
        RichTextLabel richTextLabel = GetNode("Speed") as RichTextLabel;
        richTextLabel.Text = "Chosen speed: " + chosenOne.LinearVelocity.Length();
    }

    private void PickChosenOne() {
        var index = new RandomNumberGenerator().RandiRange(0, GetTree().GetNodesInGroup(groupName).Count);
        chosenOne = GetTree().GetNodesInGroup(groupName)[index] as Boid;
        chosenOne.Chosen = true;
    }
}
