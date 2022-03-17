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
		static readonly Func EmptyFunc = new Func();

		public int currentIndex { get; private set; } = EmptyIndex;
		public int nextIndex { get; private set; } = EmptyIndex;
		public int prevIndex { get; private set; } = EmptyIndex;

		public int length => _funcArray?.Length ?? 0;

		public bool isRepeat { get; private set; }

		private Func[] _funcArray = null!;

		public void Init(Func[] objects, int initialState = EmptyIndex)
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

				if (EmptyIndex != nextIndex)
				{
					var next = nextIndex;
					nextIndex = EmptyIndex;
					if (current.canChangeStateDelegate(next))
					{
						current.exit();

						prevIndex = currentIndex;
						currentIndex = next;
						current = GetFunc(currentIndex);

						current.enter();
					}
				}

				current.execute(deltaTime);
			} while (isRepeat);
		}

		public void ChangeState(int next)
		{
			nextIndex = next;
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