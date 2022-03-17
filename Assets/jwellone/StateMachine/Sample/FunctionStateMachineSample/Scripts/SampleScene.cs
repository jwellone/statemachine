using System;
using UnityEngine;
using UnityEngine.UI;
using jwellone.StateMachine;

#nullable enable

namespace jwellone.FunctionStateMachineSample
{
	public class SampleScene : MonoBehaviour
	{
		enum State
		{
			Stop,
			Play,
			Pause
		}

		[SerializeField] private Text _stateText = null!;
		[SerializeField] private Text _text = null!;
		[SerializeField] private InputField _inputField = null!;

		private float _time;

		private readonly FunctionStateMachine _stateMachine = new FunctionStateMachine();

		void Start()
		{
			_stateMachine.Init(new[]
			{
				new FunctionStateMachine.Func(StopEnter, null, StopExit, CanChangedStateFromStop),
				new FunctionStateMachine.Func(PlayEnter, PlayExecute, PlayExit, CanChangedStateFromPlay),
				new FunctionStateMachine.Func(PauseEnter, null, PauseExit),
			}, (int)State.Stop);
		}

		void Update()
		{
			_stateMachine.Execute(Time.deltaTime);
			_stateText.text = ((State)_stateMachine.currentIndex).ToString();
			_text.text = Math.Ceiling(_time).ToString();
		}

		public void OnClickPlay()
		{
			_stateMachine.ChangeState((int)State.Play);
		}

		public void OnClickStop()
		{
			_stateMachine.ChangeState((int)State.Stop);
		}

		public void OnClickPause()
		{
			_stateMachine.ChangeState((int)State.Pause);
		}

		void StopEnter()
		{
			_time = 0f;
			Debug.Log($"<color=yellow>StopEnter</color>");
		}

		void StopExit()
		{
			Debug.Log($"<color=yellow>StopExit</color>");
		}

		bool CanChangedStateFromStop(int nextState)
		{
			if ((State)nextState != State.Play ||
				!float.TryParse(_inputField.text, out var time))
			{
				return false;
			}

			_time = time;
			return true;
		}

		void PlayEnter()
		{
			Debug.Log($"<color=green>PlayEnter</color>");
		}

		void PlayExecute(float deltaTime)
		{
			_time -= deltaTime;
			if (_time > 0f)
			{
				return;
			}

			_time = 0f;
			_stateMachine.ChangeState((int)State.Stop);
		}

		void PlayExit()
		{
			Debug.Log($"<color=green>PlayExit</color>");
		}

		bool CanChangedStateFromPlay(int nextState)
		{
			var next = (State)nextState;
			return next == State.Stop || next == State.Pause;
		}

		void PauseEnter()
		{
			Debug.Log($"PauseEnter");
			if (_stateMachine.prevIndex == (int)State.Pause)
			{
				_stateMachine.ChangeState((int)State.Play);
			}
		}

		void PauseExit()
		{
			Debug.Log($"PauseExit");
		}
	}
}