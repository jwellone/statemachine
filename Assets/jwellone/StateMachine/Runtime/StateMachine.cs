using System;
using System.Collections.Generic;

#nullable enable

namespace jwellone.StateMachine
{
	public interface ITrigger
	{
	}

	public abstract class StateMachine<TOwner> where TOwner : StateMachine<TOwner>
	{
		protected bool isRepeat { get; private set; }
		protected State? prevState { get; private set; }
		protected State? currentState { get; private set; }
		protected State? nextState { get; private set; }

		private readonly Dictionary<Type, State> _states = new Dictionary<Type, State>();

		public abstract class State
		{
			public TOwner owner { get; protected internal set; } = null!;

			private readonly Dictionary<Type, Action<ITrigger>> _triggerActions =
				new Dictionary<Type, Action<ITrigger>>();

			protected internal void OnTrigger(in ITrigger trigger)
			{
				if (_triggerActions.Count == 0)
				{
					BuildTriggerAction();
				}

				if (_triggerActions.TryGetValue(trigger.GetType(), out var action))
				{
					action.Invoke(trigger);
				}
			}

			public virtual void Enter()
			{
			}

			public virtual void Execute(float deltaTime)
			{
			}

			public virtual void Exit()
			{
			}

			protected virtual void BuildTriggerAction()
			{
			}

			protected void OnRepeat()
			{
				owner.OnRepeat();
			}

			protected void BindTriggerAction<TTrigger>(Action<ITrigger> action) where TTrigger : ITrigger
			{
				var type = typeof(TTrigger);
				if (_triggerActions.ContainsKey(type))
				{
					return;
				}

				_triggerActions.Add(type, action);
			}
		}

		protected void GenerateAndAdd<TState>() where TState : State, new()
		{
			var type = typeof(TState);
			if (_states.ContainsKey(type))
			{
				return;
			}

			_states.Add(type, new TState {owner = (TOwner) this});
		}

		public void SendTrigger(in ITrigger trigger)
		{
			currentState?.OnTrigger(trigger);
		}

		protected internal void OnRepeat()
		{
			isRepeat = true;
		}

		protected internal void ChangeState<TState>() where TState : State
		{
			if (_states.TryGetValue(typeof(TState), out var state))
			{
				nextState = state;
			}
		}

		public void Execute(float deltaTime)
		{
			do
			{
				isRepeat = false;

				if (nextState != null)
				{
					currentState?.Exit();
					currentState = nextState;
					nextState = null;
					currentState.Enter();
				}

				currentState?.Execute(deltaTime);
			} while (isRepeat);
		}
	}
}