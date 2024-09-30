using Godot;
using System;

public partial class RotatingCamera3D : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var newRotation = this.Rotation;
		newRotation.Y = (float)(newRotation.Y + 0.1 * delta);
		this.Rotation = newRotation;
	}
} 
