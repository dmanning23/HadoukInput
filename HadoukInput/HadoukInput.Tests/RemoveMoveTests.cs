using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;

namespace HadoukInput.Tests
{
	[TestFixture]
	public class RemoveMoveTests
	{
		MoveList moves;

		[SetUp]
		public void Setup()
		{
			moves = new MoveList();
		}

		[Test]
		public void RemoveSimple()
		{
			moves.AddMove("punch", EKeystroke.A);
			moves.RemoveMove("punch", EKeystroke.A);

			moves.Moves.Count.ShouldBe(0);
		}

		[Test]
		public void RemoveSimple_1()
		{
			moves.AddMove("punch", EKeystroke.A);
			moves.AddMove("kick", EKeystroke.B);
			moves.RemoveMove("punch", EKeystroke.A);

			moves.Moves.Count.ShouldBe(1);
			moves.Moves.ContainsKey(EKeystroke.B).ShouldBeTrue();
			moves.Moves[EKeystroke.B].MoveName.ShouldBe("kick");
		}

		[Test]
		public void RemoveComplex()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.RemoveMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);

			moves.Moves.Count.ShouldBe(0);
		}

		[Test]
		public void RemoveComplex_1()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.AddMove("kick", EKeystroke.B);
			moves.RemoveMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);

			moves.Moves.Count.ShouldBe(1);
			moves.Moves.ContainsKey(EKeystroke.B).ShouldBeTrue();
			moves.Moves[EKeystroke.B].MoveName.ShouldBe("kick");
		}

		[Test]
		public void RemoveComplex_2()
		{
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.AddMove("hurricane kick", EKeystroke.Down, EKeystroke.Back, EKeystroke.A);
			moves.RemoveMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);

			moves.Moves.Count.ShouldBe(1);
			moves.Moves.ContainsKey(EKeystroke.Down).ShouldBeTrue();

			var down = moves.Moves[EKeystroke.Down];
			down.MoveName.ShouldBeNullOrEmpty();
			down.Moves.Count.ShouldBe(1);

			var back = down.Moves[EKeystroke.Back];
			back.MoveName.ShouldBeNullOrEmpty();
			back.Moves.Count.ShouldBe(1);

			var kick = back.Moves[EKeystroke.A];
			kick.MoveName.ShouldBe("hurricane kick");
		}

	}
}