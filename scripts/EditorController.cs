using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Newtonsoft.Json;

namespace Questionare.scripts;

public partial class EditorController : Control {
	[Export] public Button PreviousQuestion;
	[Export] public Button CreateQuestion;
	[Export] public Button Exit;
	[Export] public Button NextQuestion;
	[Export] public Label Saved;
	[Export] public LineEdit Q;
	[Export] public LineEdit A1;
	[Export] public LineEdit A2;
	[Export] public LineEdit A3;
	[Export] public LineEdit A4;
	[Export] public LineEdit Aint;
	[Export] public CheckButton Int;
	
	private List<Question> _questions;
	private int _pointer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		if (!File.Exists("q.json")) {
			using StreamWriter w = File.CreateText("q.json");
			
			List<Question> questions = new();
			questions.Add(new Question {
				Type = QuestionType.FourChoices,
				Q = "Toto je testovací otázka, správně je A",
				A1 = "A",
				A2 = "B",
				A3 = "C",
				A4 = "D",
				Correct = 1
			});
			w.Write(JsonConvert.SerializeObject(questions));
			w.Close();
		}

		using StreamReader r = new("q.json");
		
		string json = r.ReadToEnd();
		r.Close();
		_questions = JsonConvert.DeserializeObject<List<Question>>(json);

		NextQuestion.Pressed += () => {
			if (_pointer > _questions.Count - 1) {
				SaveAndUpdateQuestion(0);
				return;
			}
			SaveAndUpdateQuestion(1);
		};

		PreviousQuestion.Pressed += () => {
			if (_pointer <= 0) {
				SaveAndUpdateQuestion(0);
				return;
			}

			SaveAndUpdateQuestion(-1);
		};

		Int.Pressed += () => {
			SaveAndUpdateQuestion(0);
		};

		CreateQuestion.Pressed += () => {
			_questions.Add(new Question {
				Type = QuestionType.FourChoices,
				Q = "<nová otázka>",
				A1 = " ",
				A2 = " ",
				A3 = " ",
				A4 = " ",
				Correct = 1
			});
			SaveAndUpdateQuestion(_questions.Count - 1 - _pointer);
		};

		Exit.Pressed += () => {
			SaveAndUpdateQuestion(0);
			GetTree().ChangeSceneToFile("res://scenes/main.tscn");
		};
 		
		SaveAndUpdateQuestion(0,false);
	}

	private void SaveAndUpdateQuestion(int offset,bool save = true) {
		DisableAllUi();

		if (save) {
			Question sq = _questions[_pointer];
			sq.Type = Int.ButtonPressed ? QuestionType.Integer : QuestionType.FourChoices;
			sq.Q = Q.Text;
			sq.A1 = A1.Text;
			sq.A2 = A2.Text;
			sq.A3 = A3.Text;
			sq.A4 = A4.Text;
			sq.Correct = int.Parse(Aint.Text);
			_questions[_pointer] = sq;

			using StreamWriter w = File.CreateText("q.json");

			w.Write(JsonConvert.SerializeObject(_questions));
			w.Close();

			GetTree().CreateTimer(.7).Timeout += () => {
				Saved.SetModulate(new Color(1,1,1,0));
			};
			Saved.SetModulate(new Color(1,1,1,1));
		}

		_pointer+=offset;
		Question q = _questions[_pointer];

		Q.Text = q.Q;
		A1.Text = q.A1;
		A2.Text = q.A2;
		A3.Text = q.A3;
		A4.Text = q.A4;
		Aint.Text = q.Correct.ToString();
		Int.ButtonPressed = q.Type == QuestionType.Integer;
		
		switch (q.Type) {
			case QuestionType.FourChoices:
				A1.Editable = true;
				A2.Editable = true;
				A3.Editable = true;
				A4.Editable = true;
				break;
			case QuestionType.Integer:
				A1.Editable = false;
				A2.Editable = false;
				A3.Editable = false;
				A4.Editable = false;
				break;
		}
		Q.Editable = true;
		Int.Disabled = true;
		Aint.Editable = true;
		PreviousQuestion.Disabled = false;
		CreateQuestion.Disabled = false;
		NextQuestion.Disabled = false;
	}

	private void DisableAllUi() {
		Q.Editable = false;
		A1.Editable = false;
		A2.Editable = false;
		A3.Editable = false;
		A4.Editable = false;
		Aint.Editable = false;
		Int.Disabled = true;
		PreviousQuestion.Disabled = true;
		CreateQuestion.Disabled = true;
		NextQuestion.Disabled = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

public class Question {
	public QuestionType Type { get; set; }
	public string Q { get; set; }
	public string A1 { get; set; }
	public string A2 { get; set; }
	public string A3 { get; set; }
	public string A4 { get; set; }
	public int Correct { get; set; }
}

public enum QuestionType {
	FourChoices,
	Integer,
}