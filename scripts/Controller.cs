using System;
using System.Collections.Generic;
using System.IO;
using CSharpChatReceiver;
using CSharpChatReceiver.records;
using Godot;
using Newtonsoft.Json;
using Questionare.scripts;
using ErrorEventArgs = CSharpChatReceiver.ErrorEventArgs;
using PieChart = Questionare.scripts.PieChart;

namespace Questionare;

public partial class Controller : Node2D {
	[Export] public Node2D TopLeft;
	[Export] public Node2D BottomRight;
	[Export] public ColorRect Red;
	[Export] public ColorRect Yellow;
	[Export] public ColorRect Green;
	[Export] public ColorRect Blue;
	[Export] public Button ResetButton;
	[Export] public LineEdit YoutubeStreamId;
	[Export] public Window Window;
	[Export] public Timer Timer;
	[Export] public RichTextLabel ChatHistory;
	[Export] public Label HowToVote;
	[Export] public Label Question;
	[Export] public Label A1;
	[Export] public Label A2;
	[Export] public Label A3;
	[Export] public Label A4;

	[Export] public Button ClearVotesButton;
	[Export] public Button PrevQuestionButton;
	[Export] public Button NextQuestionButton;
	[Export] public Button FinishQuestionButton;
	[Export] public Button Test;
	[Export] public Button EditQuestionsButton;

	[Export] public Node2D QuestionBox;
	[Export] public Node2D Answers;
	[Export] public Node2D Box;

	private readonly List<ChatItem> _messages = new(64);
	private readonly Dictionary<string,int> _votes = new(64);

	private List<Question> _questions;
	private int _questionPtr;

	private bool _isShowingAnswer;
	private bool _isSuppressed = true;
	private int _correctAnswer;

	private ChatReceiver _chatReceiver;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		SetAllUiUsability(false);
		if (!File.Exists("q.json")) {
			GetTree().ChangeSceneToFile("res://scenes/editor.tscn");
			return;
		}
		using StreamReader r = new("q.json");
		string json = r.ReadToEnd();
		r.Close();
		_questions = JsonConvert.DeserializeObject<List<Question>>(json);
		
		
		Timer.Timeout += () => { _chatReceiver?.Execute(); };
		ResetButton.Pressed += ResetSystem;
		ClearVotesButton.Pressed += ClearVotes;
		PrevQuestionButton.Pressed += () => {
			if (_questionPtr > 0) {
				if (!_isSuppressed) {
					_questionPtr--;
				}
				_isSuppressed = false;

				SwitchToNewQuestion();
			}
			else {
				ClearScreen();
			}

		};
		NextQuestionButton.Pressed += () => {
			if (_questionPtr < _questions.Count-1) {
				if (!_isSuppressed) {
					_questionPtr++;
				}
				_isSuppressed = false;

				SwitchToNewQuestion();
			}
			else {
				ClearScreen();
			}

		};
		FinishQuestionButton.Pressed += FinishQuestion;
		EditQuestionsButton.Pressed += () => {
			GetTree().ChangeSceneToFile("res://scenes/editor.tscn");
		};

		Test.Pressed += () => {
			int answer = 0;
			switch (Random.Shared.Next(0, 4)) {
				case 0:
					answer = 1;
					break;
				case 1:
					answer = 2;
					break;
				case 2:
					answer = 3;
					break;
				case 3:
					answer = 4;
					break;
			}
			
			_votes.Add("test"+Random.Shared.Next(0, 10), answer);
			UpdateRatios();
		};
		
		ClearVotes();
		ClearScreen();
		ResetColors();
		SetAllUiUsability(true);
	}

	private void ClearScreen() {
		SetAllUiUsability(false);
		_isSuppressed = true;
		Tween fadeout = GetTree().CreateTween()
			.SetParallel();
		fadeout.TweenProperty(QuestionBox, "modulate", new Color(1, 1, 1, 0), .5);
		fadeout.TweenProperty(Box, "modulate", new Color(1, 1, 1, 0), .5);
		fadeout.TweenProperty(Answers, "modulate", new Color(1, 1, 1, 0), .5);
		fadeout.Finished += () => SetAllUiUsability(true);
		fadeout.Play();
	}

	private void FinishQuestion() {
		SetAllUiUsability(false);
		_isShowingAnswer = true;
		Tween tween = GetTree()
			.CreateTween()
			.SetParallel();
		
		tween.TweenProperty(A1, "modulate", _correctAnswer == 1 ? _redColor : _redGrayColor, .3);
		tween.TweenProperty(A2, "modulate", _correctAnswer == 2 ? _yellowColor : _yellowGrayColor, .3);
		tween.TweenProperty(A3, "modulate", _correctAnswer == 3 ? _greenColor : _greenGrayColor, .3);
		tween.TweenProperty(A4, "modulate", _correctAnswer == 4 ? _blueColor : _blueGrayColor, .3);
		
		tween.TweenProperty(Red, "modulate", _correctAnswer == 1 ? _redColor : _redGrayColor, .3);
		tween.TweenProperty(Yellow, "modulate", _correctAnswer == 2 ? _yellowColor : _yellowGrayColor, .3);
		tween.TweenProperty(Green, "modulate", _correctAnswer == 3 ? _greenColor : _greenGrayColor, .3);
		tween.TweenProperty(Blue, "modulate", _correctAnswer == 4 ? _blueColor : _blueGrayColor, .3);

		tween.Finished += () => { SetAllUiUsability(true); };

		tween.Play();
	}

	//FF2200
	private readonly Color _redColor = new("#FF2200");

	//FFCC00
	private readonly Color _yellowColor = new("#FFCC00");

	//44FF00
	private readonly Color _greenColor = new("#44FF00");

	//0099FF
	private readonly Color _blueColor = new("#0099FF");

	//606060
	private readonly Color _redGrayColor = new("#606060");

	//C3C3C3
	private readonly Color _yellowGrayColor = new("#C3C3C3");

	//AAAAAA
	private readonly Color _greenGrayColor = new("#AAAAAA");

	//767676
	private readonly Color _blueGrayColor = new("#767676");

	private void SwitchToNewQuestion() {
		SetAllUiUsability(false);
		_isShowingAnswer = false;
		_isSuppressed = true;

		Tween fadeout = GetTree().CreateTween()
			.SetParallel();
		fadeout.TweenProperty(QuestionBox, "modulate", new Color(1, 1, 1, 0), .5);
		fadeout.TweenProperty(Box, "modulate", new Color(1, 1, 1, 0), .5);
		fadeout.TweenProperty(Answers, "modulate", new Color(1, 1, 1, 0), .5);

		fadeout.Finished += () => {
			ResetColors();
			ClearVotes();
			SwapQuestion();
			GetTree().CreateTimer(.75).Timeout += () => {
				Tween fadeinTitle = GetTree().CreateTween()
					.SetParallel();
				fadeinTitle.TweenProperty(QuestionBox, "modulate", new Color(1, 1, 1, 1), .5);
				fadeinTitle.Finished += () => {
					GetTree().CreateTimer(2).Timeout += () => {
						Tween fadeInTheRest = GetTree().CreateTween();
						fadeInTheRest.TweenProperty(Answers, "modulate", new Color(1, 1, 1, 1), .5);
						fadeInTheRest.TweenInterval(.5);
						fadeInTheRest.TweenProperty(Box, "modulate", new Color(1, 1, 1, 1), 1.5);

						fadeInTheRest.Finished += () => {
							_isShowingAnswer = false;
							_isSuppressed = false;
							ClearVotes();
							SetAllUiUsability(true);
						};
						fadeInTheRest.Play();
					};
				};
				fadeinTitle.Play();
			};
		};
		fadeout.Play();
	}

	private void ResetColors() {
		Red.Modulate = _redColor;
		Yellow.Modulate = _yellowColor;
		Green.Modulate = _greenColor;
		Blue.Modulate = _blueColor;
		
		A1.Modulate = _redColor;
		A2.Modulate = _yellowColor;
		A3.Modulate = _greenColor;
		A4.Modulate = _blueColor;
	}

	private void SwapQuestion() {
		Question q = _questions[_questionPtr];
		
		Question.SetText(q.Q);
		A1.SetText("1) "+q.A1);
		A2.SetText("2) "+q.A2);
		A3.SetText("3) "+q.A3);
		A4.SetText("4) "+q.A4);
		
		_correctAnswer = q.Correct;
	}

	private void SetAllUiUsability(bool usable) {
		YoutubeStreamId.Editable = usable;
		ResetButton.Disabled = !usable;
		ClearVotesButton.Disabled = !usable;
		PrevQuestionButton.Disabled = !usable;
		NextQuestionButton.Disabled = !usable;
		FinishQuestionButton.Disabled = !usable;
		EditQuestionsButton.Disabled = !usable;
	}

	private void ClearVotes() {
		_votes.Clear();
		UpdateRatios();
	}

	private void ResetSystem() {
		GD.Print("Reset");
		GD.Print(YoutubeStreamId.Text);
		ClearVotes();
		UpdateRatios();
		if (YoutubeStreamId.Text.Trim() == "") return;
		Timer.Stop();
		_messages.Clear();
		UpdateChatDisplay();
		_chatReceiver = new ChatReceiver(YoutubeStreamId.Text);
		_chatReceiver.StartupEvent += (_, _) => { GD.Print("Connected!"); };
		_chatReceiver.ShutdownEvent += (_, _) => { GD.Print("Shuting down...!"); };
		_chatReceiver.ErrorEvent += (_, e) => {
			if (e is ErrorEventArgs eargs) {
				GD.Print($"Error: {eargs.Msg}!");
			}
		};
		_chatReceiver.MessageEvent += (_, e) => {
			if (e is MessageEventArgs args) {
				_messages.Add(args.Item);
				if (_messages.Count > 64) {
					_messages.RemoveAt(0);
				}

				if (args.Item.Message.Trim().Length == 1) {
					string msg = args.Item.Message.Trim();
					if (int.TryParse(msg, out int result) && !(_isShowingAnswer || _isSuppressed)) {
						int vote = result switch {
							1 => 1,
							2 => 2,
							3 => 3,
							4 => 4,
							_ => -1
						};
						_votes[args.Item.ChatAuthor.Name] = vote;
					} else GD.Print("Mist");
				}

				UpdateRatios();
				UpdateChatDisplay();
				GD.Print(args.Item.ChatAuthor.Name + " => " + args.Item.Message);
			}
		};
		_chatReceiver.Start();
		Timer.Start();
	}

	private void UpdateRatios() {
		int red = 0, yellow = 0, green = 0, blue = 0, count = 0;
		
		foreach ((string _, int vote) in _votes) {
			switch (vote) {
				case 1:
					red++;
					count++;
					break;
				case 2:
					yellow++;
					count++;
					break;
				case 3:
					green++;
					count++;
					break;
				case 4:
					blue++;
					count++;
					break;
			}
		}
		
		float redratio = (float)red / count,
			yellowratio = (float)yellow / count,
			greenratio = (float)green / count,
			blueratio = (float)blue / count;

		// redratio = .25f;
		// yellowratio = .25f;
		// greenratio = .25f;
		// blueratio = .25f;

		if (count == 0) {
			redratio = 0;
			yellowratio = 0;
			greenratio = 0;
			blueratio = 0;
		}

		float w = BottomRight.Position.X - TopLeft.Position.X;
		float h = BottomRight.Position.Y - TopLeft.Position.Y;

		float redWidth = w * redratio;
		float yellowWidth = w * yellowratio;
		float greenWidth = w * greenratio;
		float blueWidth = w * blueratio;

		Vector2 redsiz = (new(redWidth, h));
		Vector2 ylwsiz = (new(yellowWidth + redWidth, h));
		Vector2 grnsiz = (new(greenWidth + yellowWidth + redWidth, h));
		Vector2 blusiz = (new(blueWidth + greenWidth + yellowWidth + redWidth, h));

		Tween trans = GetTree().CreateTween()
			.SetParallel()
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		trans.TweenProperty(Red, "size", redsiz, .8);
		trans.TweenProperty(Yellow, "size", ylwsiz, .8);
		trans.TweenProperty(Green, "size", grnsiz, .8);
		trans.TweenProperty(Blue, "size", blusiz, .8);

		trans.Play();
	}

	private void UpdateChatDisplay() {
		string output = "";
		foreach (ChatItem t in _messages) {
			output += $"[b]{t.ChatAuthor.Name}[/b] >> {t.Message}\n";
		}

		ChatHistory.SetText(output);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) { }
}