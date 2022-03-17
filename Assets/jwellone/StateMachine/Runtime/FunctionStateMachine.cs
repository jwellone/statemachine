using System;

#nullable enable

namespace jwellone.StateMachine
{
	public class FunctionStateMachine
	{
		public class Func
		{
			public delegate bool CanChangeStateDelegate(int nextState);

			static void Empty(float deltaTime)
			{
			}

			static void Empty()
			{
			}

			static bool CanChangeState(int nextState)
			{
				return true;
			}

			public readonly Action enter;
			public readonly Action<float> execute;
			public readonly Action exit;
			public readonly CanChangeStateDelegate canChangeStateDelegate;

			public Func(Action? enter = null, Action<float>? execute = null, Action? exit = null,
				CanChangeStateDelegate? canChangeStateDelegate = null)
			{
				this.enter = enter ?? Empty;
				this.execute = execute ?? Empty;
				this.exit = exit ?? Empty;
				this.canChangeStateDelegate = canChangeStateDelegate ?? CanChangeState;
			}
		}

		private const int EmptyIndex = -1;
		private const int StateEmpty = -9999;
		static readonly Func EmptyFunc = new Func();

		public int currentIndex { get; private set; } = EmptyIndex;
		public int nextIndex { get; private set; } = EmptyIndex;
		public int prevIndex { get; private set; } = EmptyIndex;

		public int length => _funcArray?.Length ?? 0;

		public bool isRepeat { get; private set; }

		private Func[] _funcArray = null!;

		public void Init(Func[] objects, int initialState = StateEmpty)
		{
			_funcArray = objects;
			ChangeState(initialState);
		}

		public void Execute(float deltaTime)
		{
			do
			{
				isRepeat = false;

				var current = GetFunc(currentIndex);

				if (StateEmpty != nextIndex)
				{
					current.exit();

					prevIndex = currentIndex;
					currentIndex = nextIndex;
					nextIndex = EmptyIndex;
					current = GetFunc(currentIndex);
					nextIndex = StateEmpty;

					current.enter();
				}

				current.execute(deltaTime);
			} while (isRepeat);
		}

		public bool ChangeState(int next, bool isForce = false)
		{
			if (!isForce && next == currentIndex || !GetFunc(currentIndex).canChangeStateDelegate(next))
			{
				return false;
			}

			nextIndex = next;
			return true;
		}

		public void OnRepeat()
		{
			isRepeat = true;
		}

		Func GetFunc(int index)
		{
			return (0 > index || _funcArray.Length <= index)
				? EmptyFunc
				: _funcArray[index];
		}
	}
}