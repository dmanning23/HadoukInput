using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;

namespace HadoukInput.Tests
{
	[TestFixture]
	public class MoveParsingTests
	{
		MoveList moves;
		List<InputItem> input;

		[SetUp]
		public void Setup()
		{
			moves = new MoveList();
			moves.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
			moves.AddMove("hurricane kick", EKeystroke.Down, EKeystroke.Back, EKeystroke.A);
			input = new List<InputItem>();
		}

		[Test]
		public void GetHadouken()
		{
			input.Add(new InputItem(0f, EKeystroke.Down));
			input.Add(new InputItem(0f, EKeystroke.Forward));
			input.Add(new InputItem(0f, EKeystroke.A));
			var next = moves.GetNextMove(input);
			next.ShouldBe("hadouken");
		}

		[Test]
		public void GetHurricaneKick()
		{
			input.Add(new InputItem(0f, EKeystroke.Down));
			input.Add(new InputItem(0f, EKeystroke.Back));
			input.Add(new InputItem(0f, EKeystroke.A));
			var next = moves.GetNextMove(input);
			next.ShouldBe("hurricane kick");
		}

		[Test]
		public void CleansInputList()
		{
			input.Add(new InputItem(0f, EKeystroke.Down));
			input.Add(new InputItem(0f, EKeystroke.Forward));
			input.Add(new InputItem(0f, EKeystroke.A));
			var next = moves.GetNextMove(input);
			input.Count.ShouldBe(0);
		}

		[Test]
		public void CleansInputList2()
		{
			input.Add(new InputItem(0f, EKeystroke.Down));
			input.Add(new InputItem(0f, EKeystroke.Forward));
			input.Add(new InputItem(0f, EKeystroke.A));
			input.Add(new InputItem(0f, EKeystroke.A));
			var next = moves.GetNextMove(input);
			input.Count.ShouldBe(1);
			input[0].Keystroke.ShouldBe(EKeystroke.A);
		}

		[Test]
		public void DirtyInput()
		{
			input.Add(new InputItem(0f, EKeystroke.Down));
			input.Add(new InputItem(0f, EKeystroke.Forward));
			input.Add(new InputItem(0f, EKeystroke.Back));
			input.Add(new InputItem(0f, EKeystroke.A));
			var next = moves.GetNextMove(input);
			next.ShouldBeNullOrEmpty();
		}

		[Test]
		public void DirtyInput_InFront()
		{
			input.Add(new InputItem(0f, EKeystroke.Down));
			input.Add(new InputItem(0f, EKeystroke.Forward));
			input.Add(new InputItem(0f, EKeystroke.Down));
			input.Add(new InputItem(0f, EKeystroke.Back));
			input.Add(new InputItem(0f, EKeystroke.A));
			var next = moves.GetNextMove(input);

			next.ShouldBe("hurricane kick");
			input.Count.ShouldBe(2);
			input[0].Keystroke.ShouldBe(EKeystroke.Down);
			input[1].Keystroke.ShouldBe(EKeystroke.Forward);
		}

		[Test]
		public void DirtyInput_InBack()
		{
			input.Add(new InputItem(0f, EKeystroke.Down));
			input.Add(new InputItem(0f, EKeystroke.Back));
			input.Add(new InputItem(0f, EKeystroke.A));
			input.Add(new InputItem(0f, EKeystroke.Down));
			input.Add(new InputItem(0f, EKeystroke.Forward));
			var next = moves.GetNextMove(input);

			next.ShouldBe("hurricane kick");
			input.Count.ShouldBe(2);
			input[0].Keystroke.ShouldBe(EKeystroke.Down);
			input[1].Keystroke.ShouldBe(EKeystroke.Forward);
		}
	}
}
