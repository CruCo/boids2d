using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class Boid : RigidBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    [Export]
    private string groupName = "boids";

    [Export]
    private int minSpeed = 80;

    [Export]
    private int maxSpeed = 120;

    [Export]
    private int torque = 25;

    [Export]
    public bool Chosen { get; set; } = false;

    private int perceptionRadius = 150;
    private int evasionDistance = 80;
    private Color vis_color = new Color(.867f, .91f, .247f, 0.1f);
    private RayCast2D rayCast;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AddToGroup(groupName);
        AddRayCast();
        ApplyInitialImpluse();
    }

    private void AddRayCast()
    {
        rayCast = new RayCast2D();
        AddChild(rayCast);
        rayCast.Enabled = true;
    }

    private void ApplyInitialImpluse()
    {
        var rng = new RandomNumberGenerator();
        var impulse = new Vector2(rng.RandiRange(minSpeed, maxSpeed), 0).Rotated(Rotation);
        ApplyImpulse(new Vector2(), impulse);
    }

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        base._IntegrateForces(state);
        TeleportOnScreenExit(state);
        MaintainSpeed();
        HashSet<Vector2> nodesInPerception = GetNodesInPerception();
        List<Vector2> closest = GetClosestPoints(nodesInPerception, 1);
        Evade(closest, state);
    }

    private void Evade(List<Vector2> closest, Physics2DDirectBodyState state)
    {
        closest.ForEach(node => {
            var distanceToNode = Position.DistanceTo(node);
            if (distanceToNode < evasionDistance) {
                var angle = GetAngleTo(node);
                state.AngularVelocity = (-angle)*(1/(distanceToNode/2))*torque;
            }
        });
        state.LinearVelocity = new Vector2(minSpeed, 0).Rotated(Rotation);
    }

    private List<Vector2> GetClosestPoints(HashSet<Vector2> nodesInPerception, int amount)
    {
        List<Vector2> closest = new List<Vector2>();
        if (nodesInPerception.Count > 0)
            closest = nodesInPerception.OrderBy(node => Position.DistanceTo(node)).Take(amount).ToList();

        return closest;
    }

    private HashSet<Vector2> GetNodesInPerception()
    {
        var radStep = 180 / Mathf.Pi / perceptionRadius;
        var i = 0;
        var setOfColliders = new HashSet<Vector2>();
        while (i <= perceptionRadius)
        {
            var st = (float)(radStep * i) % (2 * Mathf.Pi);
            if (st < 5 * Mathf.Pi / 4 || st > 7 * Mathf.Pi / 4)
            {
                rayCast.CastTo = new Vector2(0, -perceptionRadius).Rotated(st);
                rayCast.ForceRaycastUpdate();
                if (rayCast.IsColliding())
                {
                    setOfColliders.Add(rayCast.GetCollisionPoint());
                }
            }
            i++;
        }
        return setOfColliders;
    }

    public override void _Draw()
    {
        base._Draw();
        if (Chosen)
        {
            DrawCircle(new Vector2(), perceptionRadius, vis_color);
            var radStep = 180 / Mathf.Pi / perceptionRadius;
            var i = 0;
            while (i <= perceptionRadius)
            {
                var st = (float)(radStep * i) % (2 * Mathf.Pi);
                if (st < 5 * Mathf.Pi / 4 || st > 7 * Mathf.Pi / 4)
                {
                    DrawLine(new Vector2(0, 0), new Vector2(0, -perceptionRadius).Rotated(st), new Color("#ff8888"), 1);
                }
                i++;
            }
        }
    }

    private void TeleportOnScreenExit(Physics2DDirectBodyState state)
    {
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

    private void MaintainSpeed()
    {
        if (LinearVelocity.Length() < minSpeed)
            AppliedForce = new Vector2(minSpeed, 0).Rotated(Rotation);
        else
            AppliedForce = new Vector2();
    }
}
