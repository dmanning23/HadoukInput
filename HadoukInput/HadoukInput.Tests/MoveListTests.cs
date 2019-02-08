using NUnit.Framework;
using Shouldly;

namespace HadoukInput.Tests
{
	[TestFixture]
	public class MoveListTests
	{
		MoveList moves;

		[SetUp]
		public void Setup()
		{
			moves = new MoveList();
		}

		[Test]
		public void AddMove()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
		}

		[Test]
		public void AddMove1()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
		}

		[Test]
		public void AddMove2()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
			down.Moves.ContainsKey(EKeystroke.Forward).ShouldBeTrue();
		}

		[Test]
		public void AddMove3()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
			down.Moves.ContainsKey(EKeystroke.Forward).ShouldBeTrue();

			var forward = down.Moves[EKeystroke.Forward];
			forward.MoveName.ShouldBeNullOrEmpty();
		}

		[Test]
		public void AddMove4()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
			down.Moves.ContainsKey(EKeystroke.Forward).ShouldBeTrue();

			var forward = down.Moves[EKeystroke.Forward];
			forward.MoveName.ShouldBeNullOrEmpty();
			forward.Moves.ContainsKey(EKeystroke.A).ShouldBeTrue();
		}

		[Test]
		public void AddMove5()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
			down.Moves.ContainsKey(EKeystroke.Forward).ShouldBeTrue();

			var forward = down.Moves[EKeystroke.Forward];
			forward.MoveName.ShouldBeNullOrEmpty();
			forward.Moves.ContainsKey(EKeystroke.A).ShouldBeTrue();

			var a = forward.Moves[EKeystroke.A];
			a.MoveName.ShouldBe("hadouken");
		}

		[Test]
		public void AddMove6()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.AddMove("hurricane kick", EKeystroke.Down, EKeystroke.Back, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
		}

		[Test]
		public void AddMove7()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.AddMove("hurricane kick", EKeystroke.Down, EKeystroke.Back, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
		}

		[Test]
		public void AddMove8()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.AddMove("hurricane kick", EKeystroke.Down, EKeystroke.Back, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
			down.Moves.ContainsKey(EKeystroke.Forward).ShouldBeTrue();
			down.Moves.ContainsKey(EKeystroke.Back).ShouldBeTrue();
		}

		[Test]
		public void AddMove9()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.AddMove("hurricane kick", EKeystroke.Down, EKeystroke.Back, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
			down.Moves.ContainsKey(EKeystroke.Forward).ShouldBeTrue();
			down.Moves.ContainsKey(EKeystroke.Back).ShouldBeTrue();

			var back = down.Moves[EKeystroke.Back];
			back.MoveName.ShouldBeNullOrEmpty();
		}

		[Test]
		public void AddMove10()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.AddMove("hurricane kick", EKeystroke.Down, EKeystroke.Back, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
			down.Moves.ContainsKey(EKeystroke.Forward).ShouldBeTrue();
			down.Moves.ContainsKey(EKeystroke.Back).ShouldBeTrue();

			var back = down.Moves[EKeystroke.Back];
			back.MoveName.ShouldBeNullOrEmpty();
			back.Moves.ContainsKey(EKeystroke.A).ShouldBeTrue();
		}

		[Test]
		public void AddMove11()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.AddMove("hurricane kick", EKeystroke.Down, EKeystroke.Back, EKeystroke.A);

			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();
			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
			down.Moves.ContainsKey(EKeystroke.Forward).ShouldBeTrue();
			down.Moves.ContainsKey(EKeystroke.Back).ShouldBeTrue();

			var back = down.Moves[EKeystroke.Back];
			back.MoveName.ShouldBeNullOrEmpty();
			back.Moves.ContainsKey(EKeystroke.A).ShouldBeTrue();

			var a = back.Moves[EKeystroke.A];
			a.MoveName.ShouldBe("hurricane kick");
		}
	}
}
