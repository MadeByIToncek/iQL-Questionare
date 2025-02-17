using System.Collections.Generic;
using Godot;

namespace Questionare.scripts;

public partial class PieChart : Control
{
	// Called when the node enters the scene tree for the first time.
	
	public List<Answer> Counts = new();
	public bool ShowingAnswers = true;
	public override void _Ready()
	{
	}


	public override void _Draw() {
		List<float> ratios = new();
		int total = 0;
		
		foreach (Answer a in Counts) {
			total += a.Count;
		}
		foreach (Answer a in Counts) {
			ratios.Add((float)a.Count / total);
		}
		
		ratios.Sort();
		ratios.Reverse();
		
		float sumAngle = 0;
		for (var i = 0; i < ratios.Count; i++) {
			float angle = ratios[i]*360; 
			DrawArcPoly(new Vector2(Size.X/2f, Size.Y/2f),Size.X/2f,sumAngle,sumAngle+angle,1024,GetColor((float)i/ratios.Count));
			sumAngle += angle;
		}
	}

	private Color GetColor(float ratiosCount) {
		return Color.FromHsv(ratiosCount,1,1);
	}

	private void DrawArcPoly(Vector2 center, float r, float startAngle, float endAngle, int resolution, Color color) {
		Vector2[] polyPoints = new Vector2[resolution+1];
		polyPoints[0] = center;
		for (int i = 1; i < resolution+1; i++) {
			var anglePoint = Mathf.DegToRad(startAngle + i * (endAngle - startAngle) / resolution-1);
			polyPoints[i] = center + new Vector2(Mathf.Cos(anglePoint), Mathf.Sin(anglePoint))*r;
		}
		DrawColoredPolygon(polyPoints, color);
	}
}

public record Answer(int Count, bool Correct) { }