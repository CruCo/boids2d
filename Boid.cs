using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class Boid : RigidBody2D {
    [Export]
    private string groupName = "boids";

    [Export]
    public int MinSpeed { get; set; } = 80;

    [Export]
    public int MaxSpeed { get; set; } = 120;

    [Export]
    public int Torque { get; set; } = 25;

    [Export]
    public bool Chosen { get; set; } = false;

    private int perceptionRadius = 150;
    public int EvasionDistance { get; } = 80;

    private float radStep = 10 * Mathf.Pi / 180;

    private Color vis_color = new Color(.867f, .91f, .247f, 0.1f);
    public List<RayCast2D> RayCasts { get; } = new List<RayCast2D>();


    public override void _EnterTree() {
        AddToGroup(groupName);
        AddRayCasts();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        ApplyInitialImpluse();
    }

    public void UpdateDirection() {
        HashSet<Vector2> nodesInPerception = GetNodesInPerception();
        List<Vector2> closest = GetClosestPoints(nodesInPerception, 1);
        Evade(closest);
    }

    private void AddRayCasts() {
        float i = 0;
        while (i < 2 * Mathf.Pi) {
            if (IsInVisibleArea(i)) {
                var rayCast = new RayCast2D();
                AddChild(rayCast);
                rayCast.Enabled = true;
                rayCast.CastTo = new Vector2(0, -perceptionRadius).Rotated(i);
                RayCasts.Add(rayCast);
            }
            i += radStep;
        }
    }

    private bool IsInVisibleArea(float i) {
        // Note: Blindspot between 225° and 315°
        return (i < 5 * Mathf.Pi / 4 || i > 7 * Mathf.Pi / 4);
    }

    private void ApplyInitialImpluse() {
        var rng = new RandomNumberGenerator();
        var impulse = new Vector2(rng.RandiRange(MinSpeed, MaxSpeed), 0).Rotated(Rotation);
        ApplyImpulse(new Vector2(), impulse);
    }

    public override void _IntegrateForces(Physics2DDirectBodyState state) {
        base._IntegrateForces(state);
        TeleportOnScreenExit(state);
        MaintainSpeed();
    }

    private void Evade(List<Vector2> closest) {
        closest.ForEach(node => {
            var distanceToNode = Position.DistanceTo(node);
            if (distanceToNode < EvasionDistance) {
                var angle = GetAngleTo(node);
                this.AngularVelocity = (-angle) * (1 / (distanceToNode / 2)) * Torque;
            }
        });
        this.LinearVelocity = new Vector2(MinSpeed, 0).Rotated(Rotation);
    }

    private List<Vector2> GetClosestPoints(HashSet<Vector2> nodesInPerception, int amount) {
        List<Vector2> closest = new List<Vector2>();
        if (nodesInPerception.Count > 0)
            closest = nodesInPerception.OrderBy(node => Position.DistanceTo(node)).Take(amount).ToList();

        return closest;
    }

    private HashSet<Vector2> GetNodesInPerception() {
        var setOfColliders = new HashSet<Vector2>();
        RayCasts.ForEach(rayCast => {
            if (rayCast.IsColliding())
                setOfColliders.Add(rayCast.GetCollisionPoint());
        });
        return setOfColliders;
    }

    public override void _Draw() {
        base._Draw();
        if (Chosen) {
            DrawCircle(new Vector2(), perceptionRadius, vis_color);
            float i = 0;
            while (i < 2 * Mathf.Pi) {
                if (IsInVisibleArea(i)) {
                    DrawLine(new Vector2(0, 0), new Vector2(0, -perceptionRadius).Rotated(i), new Color("#ff8888"), 1);
                }
                i += radStep;
            }
        }
    }

    private void TeleportOnScreenExit(Physics2DDirectBodyState state) {
        var xform = state.Transform;
        var screensize = GetViewportRect().Size;
        if (xform.origin.x < 0)
            xform.origin.x = screensize.x;
        if (xform.origin.x > screensize.x)
            xform.origin.x = 0;
        if (xform.origin.y < 0)
            xform.origin.y = screensize.y;
        if (xform.origin.y > screensize.y)
            xform.origin.y = 0;

        state.Transform = xform;
    }

    private void MaintainSpeed() {
        if (LinearVelocity.Length() < MinSpeed)
            AppliedForce = new Vector2(MinSpeed, 0).Rotated(Rotation);
        else
            AppliedForce = new Vector2();
    }
}
