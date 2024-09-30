using Godot;
using System;

public partial class Birds : Node
{

    public const int BIRD_COUNT = 1;
	public Bird[] birds = new Bird[BIRD_COUNT];
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var rnd = new RandomNumberGenerator();
        var birdScene = (PackedScene)ResourceLoader.Load("res://BirdMesh3D.tscn");
        for (int i = 0; i < BIRD_COUNT; i++)
        {
            var bird = (Bird)birdScene.Instantiate();
			bird.Position = new Vector3(0,3,0);
            bird.RotationDegrees = new Vector3(rnd.Randf() * 360, rnd.Randf() * 360, rnd.Randf() * 360);
            AddChild(bird);
			birds[i] = bird;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

}
