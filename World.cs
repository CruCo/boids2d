using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public class World : Node2D {
    [Export]
    private PackedScene boidScene = ResourceLoader.Load("Boid.tscn") as PackedScene;
    private Boid chosenOne;
    private int boidCount = 300;
    private string boidGroupName = "boids";

    public override void _EnterTree() {
        base._EnterTree();
        AddBoids();
    }
    public override void _Ready() {
        base._Ready();
        StartTextUpdateTimer(CreateTimer());
        PickChosenOne();
    }

    public override void _PhysicsProcess(float delta) {
        var boids = GetTree().GetNodesInGroup(boidGroupName);
        Parallel.For(0, boids.Count, index => {
            Boid boid = boids[index] as Boid;
            boid.UpdateDirection();
        });
    }

    private void AddBoids() {
        var random = new Random();
        var screensize = GetViewport().Size;
        var rng = new RandomNumberGenerator();
        for (int i = 0; i < boidCount; i++) {
            var boid = boidScene.Instance() as Boid;
            boid.GlobalPosition = new Vector2(rng.RandfRange(0f, screensize.x), rng.RandfRange(0, screensize.y));
            var direction = (random.NextDouble() * (Math.PI * 2)) - Math.PI;
            boid.GlobalRotation = (float)direction;
            AddChild(boid);
        }
    }

    private void StartTextUpdateTimer(Godot.Timer timer) {
        timer.Connect("timeout", this, "_On_Timer_Timeout");
        timer.WaitTime = 1.0f;
        timer.OneShot = false;
        timer.Start();
    }

    private Godot.Timer CreateTimer() {
        var timer = new Godot.Timer();
        AddChild(timer);
        return timer;
    }

    private void _On_Timer_Timeout() {
        RichTextLabel richTextLabel = GetNode("Speed") as RichTextLabel;
        richTextLabel.Text = "Chosen speed: " + chosenOne.LinearVelocity.Length();
    }

    private void PickChosenOne() {
        var index = new RandomNumberGenerator().RandiRange(0, GetTree().GetNodesInGroup(boidGroupName).Count);
        chosenOne = GetTree().GetNodesInGroup(boidGroupName)[index] as Boid;
        chosenOne.Chosen = true;
    }
}
