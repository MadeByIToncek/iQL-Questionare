using System.Collections.Generic;
using CSharpChatReceiver;
using CSharpChatReceiver.records;
using Godot;

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

	[Export] public Button ClearVotesButton;
	[Export] public Button PrevQuestionButton;
	[Export] public Button NextQuestionButton;
	[Export] public Button FinishQuestionButton;
	[Export] public Button EditQuestionsButton;

	[Export] public Node2D Question;
	[Export] public Node2D Answers;
	[Export] public Node2D Box;

	private readonly List<ChatItem> _messages = new(64);
	private int _totalCount;
	private int _red, _yellow, _green, _blue;

	private int _questionPtr;
	private int _questionCount;

	private bool _isShowingAnswer;

	private ChatReceiver _chatReceiver;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Timer.Timeout += () => { _chatReceiver?.Execute(); };
		ResetButton.Pressed += ResetSystem;
		ClearVotesButton.Pressed += ClearVotes;
		PrevQuestionButton.Pressed += () => {
			if (_questionPtr > 0) {
				_questionPtr--;
			}

			SwitchToNewQuestion();
		};
		NextQuestionButton.Pressed += () => {
			if (_questionPtr < _questionCount) {
				_questionPtr++;
			}

			SwitchToNewQuestion();
		};
		FinishQuestionButton.Pressed += FinishQuestion;
		EditQuestionsButton.Pressed += () => {
			GetTree().ChangeSceneToFile("res://editor.tscn");
		};
		
		ResetColors();
	}

	private void FinishQuestion() {
		SetAllUiUsability(false);
		_isShowingAnswer = true;
		Tween tween = GetTree()
			.CreateTween()
			.SetParallel();

		int correctAnswer = 1;

		tween.TweenProperty(Red, "modulate", correctAnswer == 1 ? _redColor : _redGrayColor, .3);
		tween.TweenProperty(Yellow, "modulate", correctAnswer == 2 ? _yellowColor : _yellowGrayColor, .3);
		tween.TweenProperty(Green, "modulate", correctAnswer == 3 ? _greenColor : _greenGrayColor, .3);
		tween.TweenProperty(Blue, "modulate", correctAnswer == 4 ? _blueColor : _blueGrayColor, .3);

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

		Tween fadeout = GetTree().CreateTween()
			.SetParallel();
		fadeout.TweenProperty(Question, "modulate", new Color(1, 1, 1, 0), .5);
		fadeout.TweenProperty(Box, "modulate", new Color(1, 1, 1, 0), .5);
		fadeout.TweenProperty(Answers, "modulate", new Color(1, 1, 1, 0), .5);

		fadeout.Finished += () => {
			ResetColors();
			SwapQuestion();
			GetTree().CreateTimer(.75).Timeout += () => {
				Tween fadeinTitle = GetTree().CreateTween()
					.SetParallel();
				fadeinTitle.TweenProperty(Question, "modulate", new Color(1, 1, 1, 1), .5);
				fadeinTitle.Finished += () => {
					GetTree().CreateTimer(2).Timeout += () => {
						Tween fadeInTheRest = GetTree().CreateTween();
						fadeInTheRest.TweenProperty(Answers, "modulate", new Color(1, 1, 1, 1), .5);
						fadeInTheRest.TweenInterval(.5);
						fadeInTheRest.TweenProperty(Box, "modulate", new Color(1, 1, 1, 1), 1.5);
						fadeInTheRest.Finished += () => { SetAllUiUsability(true); };
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
	}

	private void SwapQuestion() {
		//todo
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
		_totalCount = 0;
		_red = 0;
		_yellow = 0;
		_green = 0;
		_blue = 0;
		UpdateRatios();
	}

	private void ResetSystem() {
		GD.Print("Reset");
		GD.Print(YoutubeStreamId.Text);
		_totalCount = 0;
		_red = 0;
		_yellow = 0;
		_green = 0;
		_blue = 0;
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
					if (int.TryParse(msg, out int result)) {
						switch (result) {
							case 1:
								_red++;
								break;
							case 2:
								_yellow++;
								break;
							case 3:
								_green++;
								break;
							case 4:
								_blue++;
								break;
						}

						_totalCount++;
					}
				}

				UpdateChatDisplay();
				GD.Print(args.Item.ChatAuthor.Name + " => " + args.Item.Message);
			}
		};
		_chatReceiver.Start();
		Timer.Start();
	}

	private void UpdateRatios() {
		float redratio = (float)_red / _totalCount,
			yellowratio = (float)_yellow / _totalCount,
			greenratio = (float)_green / _totalCount,
			blueratio = (float)_blue / _totalCount;

		// redratio = .25f;
		// yellowratio = .25f;
		// greenratio = .25f;
		// blueratio = .25f;

		if (_totalCount == 0) {
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