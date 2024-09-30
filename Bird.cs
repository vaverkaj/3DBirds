using Godot;
using System;

public partial class Bird : Node3D
{
	const int POINTS_SIZE = 30;
	const double TURN_FRACTION = 1.61;
	public RayCast3D[] rays = new RayCast3D[POINTS_SIZE];

	public RayCast3D directionRay = new RayCast3D();
	public RayCast3D dodgeWallRay = new RayCast3D();
	public RayCast3D dodgeBirdRay = new RayCast3D();
	public RayCast3D flockBirdRay = new RayCast3D();

	public Vector3 direction;
	private Birds parent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.InitializeRayCasts();
		parent = GetParent<Birds>();
	}

	private void InitializeRayCasts()
	{
		for (int i = 0; i < (POINTS_SIZE * 2); i++)
		{
			if (i >= POINTS_SIZE)
			{
				break;
			}
			double t = i / (double)(POINTS_SIZE * 2);
			double inclination = Math.Acos(1 - 2 * t);
			double azimuth = 2 * Math.PI * TURN_FRACTION * i;

			double x = Math.Cos(inclination);
			double y = Math.Sin(inclination) * Math.Cos(azimuth);
			double z = Math.Sin(inclination) * Math.Sin(azimuth);

			RayCast3D ray = new RayCast3D();
			ray.TargetPosition = new Vector3((float)x, (float)y, (float)z);
			ray.DebugShapeCustomColor = new Color(0f, 0f, 0f, 0f);
			AddChild(ray);
			rays[i] = ray;
		}

	}

	private Vector3 FindUnobscuredDirection()
	{
		Vector3 bestDirection = new Vector3(0, 0, 0);
		double furthestUnobscuredDistance = 0;
		for (int i = 0; i < POINTS_SIZE; i++)
		{
			if (rays[i].IsColliding())
			{
				var distance = rays[i].Position.DistanceTo(rays[i].GetCollisionPoint());
				if (distance > furthestUnobscuredDistance)
				{
					bestDirection = rays[i].TargetPosition;
					furthestUnobscuredDistance = distance;
				}
			}
			else
			{
				return rays[i].TargetPosition;
			}
		}
		return bestDirection;
	}

	double timer = 0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var speed = 1f;
		var dodgeWallDirection = FindUnobscuredDirection();
		var dodgeBirdDirection = CalculateDodgeDirection();
		var sameDirection = CalculateSameDirection();
		var flockDirection = CalculateFlockDirection();

		//TranslateObjectLocal(direction * new Vector3((float)(speed * delta), (float)(speed * delta), (float)(speed * delta)));
		direction = (Transform.Basis * dodgeWallDirection).Normalized();
		//direction += sameDirection.Normalized() * 4f;
		//direction += dodgeBirdDirection * 5f;
		//direction += flockDirection.Normalized() * 0.1f;
		if (timer > 1f)
		{
			timer = 0;
		}
		timer += delta;

		LookAt(ToGlobal(dodgeWallDirection), Vector3.Up);
		RotateObjectLocal(Vector3.Up, Mathf.Pi / 2);
		TranslateObjectLocal(Vector3.Right * (float)(speed * delta));
		//Position += (Transform.Basis * Vector3.Right) * (float)(speed * delta) * 2;
		//drawRays(sameDirection.Normalized(), dodgeWallDirection.Normalized(), dodgeBirdDirection.Normalized(), flockDirection.Normalized());
		//drawRays(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), Transform.Basis * rays[0].TargetPosition);
	}

	private Vector3 CalculateFlockDirection()
	{
		Vector3 flockDirection = new Vector3(0, 0, 0);
		var dirCount = 0;
		for (int i = 0; i < parent.birds.Length; i++)
		{
			if (isClose(parent.birds[i]))
			{
				if (dirCount == 0)
				{
					flockDirection = parent.birds[i].Position;
				}
				else
				{
					flockDirection += parent.birds[i].Position;
				}
				dirCount++;
			}
		}
		flockDirection /= dirCount;
		return flockDirection - Position;
	}

	private Vector3 CalculateSameDirection()
	{
		var sameDirection = new Vector3(0, 0, 0);
		var dirCount = 0;
		for (int i = 0; i < parent.birds.Length; i++)
		{
			if (isClose(parent.birds[i]))
			{
				if (dirCount == 0)
				{
					sameDirection = parent.birds[i].direction;
				}
				else
				{
					sameDirection += parent.birds[i].direction;
				}
				dirCount++;
			}
		}
		sameDirection /= dirCount;

		return sameDirection;
	}

	private void drawRays(Vector3 direction, Vector3 dodgeWall, Vector3 dodgeBird, Vector3 flockDirection)
	{
		directionRay.QueueFree();
		dodgeWallRay.QueueFree();
		dodgeBirdRay.QueueFree();
		flockBirdRay.QueueFree();
		directionRay = new RayCast3D();
		dodgeWallRay = new RayCast3D();
		dodgeBirdRay = new RayCast3D();
		flockBirdRay = new RayCast3D();
		directionRay.Position = this.Position;
		dodgeWallRay.Position = this.Position;
		dodgeBirdRay.Position = this.Position;
		flockBirdRay.Position = this.Position;
		directionRay.DebugShapeCustomColor = new Color(1f, 0f, 1f, 1f);
		dodgeWallRay.DebugShapeCustomColor = new Color(0f, 1f, 1f, 1f);
		dodgeBirdRay.DebugShapeCustomColor = new Color(1f, 1f, 0f, 1f);
		flockBirdRay.DebugShapeCustomColor = new Color(0f, 1f, 0f, 1f);

		directionRay.TargetPosition = direction;
		dodgeWallRay.TargetPosition = dodgeWall;
		dodgeBirdRay.TargetPosition = dodgeBird;
		flockBirdRay.TargetPosition = flockDirection;

		parent.AddChild(directionRay);
		parent.AddChild(dodgeWallRay);
		parent.AddChild(dodgeBirdRay);
		parent.AddChild(flockBirdRay);
	}

	private Vector3 CalculateDodgeDirection()
	{
		/*
		var mesh = this.GetChild<Node3D>(0).GetChild<MeshInstance3D>(0);
		var newMaterial = new StandardMaterial3D();
		newMaterial.AlbedoColor = new Color(0.9f, 0.6f, 0.9f);
		mesh.SetSurfaceOverrideMaterial(0, newMaterial);
		*/
		var positionAvarage = new Vector3(0, 0, 0);
		var dirCount = 0;
		for (int i = 0; i < parent.birds.Length; i++)
		{
			if (isClose(parent.birds[i]))
			{
				var distance = this.Position.DistanceTo(parent.birds[i].Position);
				if (parent.birds[i].Position - this.Position != new Vector3(0, 0, 0))
				{
					if (dirCount == 0)
					{
						positionAvarage = (parent.birds[i].Position - this.Position).Normalized() * (1 / distance);
					}
					else
					{
						positionAvarage += (parent.birds[i].Position - this.Position).Normalized() * (1 / distance);
					}
					dirCount++;
				}
			}
		}
		if (dirCount > 0)
		{
			positionAvarage /= (dirCount);
		}
		else
		{
			return new Vector3(0, 0, 0);
		}

		return -positionAvarage;
	}


	private RayCast3D[] distanceRays = new RayCast3D[Birds.BIRD_COUNT];
	public void drawDistances()
	{

		if (parent.birds[0] == this)
		{
			foreach (RayCast3D ray in distanceRays)
			{
				if (ray != null)
				{
					ray.QueueFree();
				}
			}

			var mesh = this.GetChild<Node3D>(0).GetChild<MeshInstance3D>(0);
			var newMaterial = new StandardMaterial3D();
			newMaterial.AlbedoColor = new Color(0.9f, 0.6f, 0.9f);
			mesh.SetSurfaceOverrideMaterial(0, newMaterial);

			var positionAvarage = new Vector3(0, 0, 0);
			for (int i = 0; i < parent.birds.Length; i++)
			{
				RayCast3D ray = new RayCast3D();
				ray.Position = this.Position;
				ray.TargetPosition = parent.birds[i].Position - this.Position;
				var distance = this.Position.DistanceTo(parent.birds[i].Position);
				ray.DebugShapeCustomColor = new Color(1 / distance, 1f - (1 / distance), 0f, 1f);
				parent.AddChild(ray);
				distanceRays[i] = ray;
				if (parent.birds[i] != this)
				{
					positionAvarage += ray.TargetPosition.Normalized() * (float)Math.Pow(1 / distance, 2);
				}
			}
			positionAvarage /= (parent.birds.Length - 1);

			RayCast3D directionRay = new RayCast3D();
			directionRay.Position = this.Position;
			directionRay.TargetPosition = -positionAvarage;
			directionRay.DebugShapeCustomColor = new Color(0f, 0f, 1f, 1f);
			parent.AddChild(directionRay);
			distanceRays[0].QueueFree();
			distanceRays[0] = directionRay;
		}
	}

	public bool isClose(Bird otherBird)
	{
		if (Position.DistanceTo(otherBird.Position) < 1f)
		{
			return true;
		}
		return false;
	}

}
