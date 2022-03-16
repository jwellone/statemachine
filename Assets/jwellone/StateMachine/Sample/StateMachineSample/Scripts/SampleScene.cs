using System;
using UnityEngine;
using UnityEngine.UI;
using jwellone.StateMachine;

#nullable enable

namespace jwellone.StateMachineSample
{
	public class SampleScene : MonoBehaviour
	{
		[SerializeField] private Text _stateText = null!;
		[SerializeField] private Text _text = null!;
		[SerializeField] private InputField _inputField = null!;

		private readonly CountDownTimerStateMachine _stateMachine = new CountDownTimerStateMachine();

		void Update()
		{
			_stateMachine.Execute(Time.deltaTime);
			_stateText.text = _stateMachine.stateText;
			_text.text = Math.Ceiling(_stateMachine.time).ToString();
		}

		public void OnClickPlay()
		{
			if (float.TryParse(_inputField.text, out var time))
			{
				_stateMachine.SendTrigger(new PlayTrigger(time));
			}
		}

		public void OnClickStop()
		{
			_stateMachine.SendTrigger(new StopTrigger());
		}

		public void OnClickPause()
		{
			_stateMachine.SendTrigger(new PauseTrigger());
		}
	}

	public sealed class StopTrigger : ITrigger
	{
	}

	public sealed class PlayTrigger : ITrigger
	{
		public readonly float time;

		public PlayTrigger(float time)
		{
			this.time = time;
		}
	}

	public sealed class PauseTrigger : ITrigger
	{
	}

	public class CountDownTimerStateMachine : StateMachine<CountDownTimerStateMachine>
	{
		public float time { get; private set; }
		public string? stateText => currentState?.GetType().Name;

		public CountDownTimerStateMachine()
		{
			GenerateAndAdd<StopState>();
			GenerateAndAdd<PlayState>();
			GenerateAndAdd<PauseState>();

			ChangeState<StopState>();
		}

		class StopState : State
		{
			public override void Enter()
			{
				Debug.Log($"<color=yellow>{GetType().Name}.Enter</color>");
				owner.time = 0f;
			}

			public override void Exit()
			{
				Debug.Log($"<color=yellow>{GetType().Name}.Exit</color>");
			}

			protected override void BuildTriggerAction()
			{
				BindTriggerAction<PlayTrigger>((t) =>
				{
					var playerTrigger = (PlayTrigger) t;
					owner.time = playerTrigger.time;
					owner.ChangeState<PlayState>();
				});
			}
		}

		class PlayState : State
		{
			public override void Enter()
			{
				Debug.Log($"<color=green>{GetType().Name}.Enter</color>");
			}

			public override void Execute(float deltaTime)
			{
				owner.time -= deltaTime;
				if (owner.time > 0f)
				{
					return;
				}

				owner.time = 0f;
				owner.ChangeState<StopState>();
			}

			public override void Exit()
			{
				Debug.Log($"<color=green>{GetType().Name}.Exit</color>");
			}

			protected override void BuildTriggerAction()
			{
				BindTriggerAction<PauseTrigger>((t) => { owner.ChangeState<PauseState>(); });
				BindTriggerAction<StopTrigger>((t) => { owner.ChangeState<StopState>(); });
			}
		}

		class PauseState : State
		{
			public override void Enter()
			{
				Debug.Log($"{GetType().Name}.Enter");
			}

			public override void Exit()
			{
				Debug.Log($"{GetType().Name}.Exit");
			}

			protected override void BuildTriggerAction()
			{
				BindTriggerAction<PlayTrigger>((t) => { owner.ChangeState<PlayState>(); });
				BindTriggerAction<PauseTrigger>((t) => { owner.ChangeState<PlayState>(); });
				BindTriggerAction<StopTrigger>((t) => { owner.ChangeState<StopState>(); });
			}
		}
	}
}