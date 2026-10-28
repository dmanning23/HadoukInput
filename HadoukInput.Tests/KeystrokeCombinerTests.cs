using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HadoukInput.Tests
{
    /// <summary>
    /// Characterization tests for how InputWrapper combines keystrokes.
    /// These pin down the current behaviour of the combination tables so they can be refactored safely.
    /// </summary>
    [TestFixture]
    public class KeystrokeCombinerTests
    {
        /// <summary>
        /// A controller that reports whatever keystrokes the test says were pressed this frame
        /// </summary>
        class FakeController : IControllerWrapper
        {
            public HashSet<EKeystroke> Pressed { get; } = new HashSet<EKeystroke>();

            public bool AnyDirectionHeld => false;
            public ThumbsticksWrapper Thumbsticks => null!;
            public bool UseKeyboard => false;

            public bool CheckKeystroke(EKeystroke keystroke) => Pressed.Contains(keystroke);
            public bool CheckKeystroke(EKeystroke keystroke, bool flipped, Vector2 direction) => Pressed.Contains(keystroke);
            public bool CheckKeystrokeHeld(EKeystroke keystroke) => Pressed.Contains(keystroke);
            public void ResetController() => Pressed.Clear();
            public Keys MappedKey(int gamePadIndex, ControllerAction action) => Keys.None;
            public void Update(IInputState inputState) { }
        }

        FakeController controller;
        InputWrapper input;
        float time;

        [SetUp]
        public void Setup()
        {
            controller = new FakeController();
            time = 0f;
            input = new InputWrapper(controller, () => time);
        }

        #region Helpers

        /// <summary>
        /// Run one frame at the given time with the given keystrokes pressed
        /// </summary>
        void Frame(float at, params EKeystroke[] pressed)
        {
            time = at;
            controller.Pressed.Clear();
            foreach (var key in pressed)
            {
                controller.Pressed.Add(key);
            }
            input.Update(null, false);
        }

        /// <summary>
        /// Run an empty frame after the buffer window so everything buffered lands in the queue
        /// </summary>
        void Flush()
        {
            Frame(time + 0.1f);
        }

        static List<EKeystroke> Parse(string text)
        {
            return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => Enum.Parse<EKeystroke>(x))
                .ToList();
        }

        List<EKeystroke> Queued() => Parse(input.ToString());

        List<EKeystroke> Buffered() => Parse(input.GetBufferedInput());

        /// <summary>
        /// Press the keys in one frame, flush, and return the queue
        /// </summary>
        List<EKeystroke> PressTogether(params EKeystroke[] keys)
        {
            Frame(0f, keys);
            Flush();
            return Queued();
        }

        #endregion //Helpers

        #region Single keys

        [TestCase(EKeystroke.Up)]
        [TestCase(EKeystroke.Neutral)]
        [TestCase(EKeystroke.UpR)]
        [TestCase(EKeystroke.A)]
        [TestCase(EKeystroke.RTrigger)]
        [TestCase(EKeystroke.ARelease)]
        [TestCase(EKeystroke.ForwardShoulder)]
        public void SingleKeyPassesThrough(EKeystroke key)
        {
            PressTogether(key).ShouldBe(new[] { key });
        }

        [Test]
        public void KeyStaysInBufferUntilWindowExpires()
        {
            Frame(0f, EKeystroke.A);
            Buffered().ShouldBe(new[] { EKeystroke.A });
            Queued().ShouldBeEmpty();

            Frame(0.04f);
            Buffered().ShouldBe(new[] { EKeystroke.A });
            Queued().ShouldBeEmpty();

            Frame(0.06f);
            Buffered().ShouldBeEmpty();
            Queued().ShouldBe(new[] { EKeystroke.A });
        }

        [Test]
        public void SameKeyTwiceInWindowIsBufferedOnce()
        {
            Frame(0f, EKeystroke.A);
            Frame(0.02f, EKeystroke.A);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.A });
        }

        [Test]
        public void QueuedInputExpires()
        {
            PressTogether(EKeystroke.A).ShouldBe(new[] { EKeystroke.A });
            Frame(0.6f);
            Queued().ShouldBeEmpty();
        }

        [Test]
        public void ClearEmptiesBufferAndQueue()
        {
            Frame(0f, EKeystroke.A);
            Flush();
            Frame(time, EKeystroke.B);
            input.Clear();
            Queued().ShouldBeEmpty();
            Buffered().ShouldBeEmpty();
        }

        #endregion //Single keys

        #region Button + direction

        static readonly EKeystroke[] Buttons =
        {
            EKeystroke.A, EKeystroke.B, EKeystroke.X, EKeystroke.Y,
            EKeystroke.LShoulder, EKeystroke.RShoulder, EKeystroke.LTrigger, EKeystroke.RTrigger,
        };

        static readonly EKeystroke[] Directions =
        {
            EKeystroke.Up, EKeystroke.Down, EKeystroke.Forward, EKeystroke.Back,
        };

        static IEnumerable<TestCaseData> ButtonDirectionCases()
        {
            foreach (var button in Buttons)
            {
                foreach (var direction in Directions)
                {
                    var combined = Enum.Parse<EKeystroke>($"{button}{direction}");
                    yield return new TestCaseData(button, direction, combined)
                        .SetName($"ButtonDirection_{button}_{direction}");
                }
            }
        }

        [TestCaseSource(nameof(ButtonDirectionCases))]
        public void ButtonAndDirectionInSameFrameCombine(EKeystroke button, EKeystroke direction, EKeystroke combined)
        {
            input.KeystrokeCombiner.Add(combined);
            PressTogether(button, direction).ShouldBe(new[] { combined });
        }

        [TestCaseSource(nameof(ButtonDirectionCases))]
        public void DirectionThenButtonInsideWindowCombine(EKeystroke button, EKeystroke direction, EKeystroke combined)
        {
            input.KeystrokeCombiner.Add(combined);
            Frame(0f, direction);
            Frame(0.02f, button);
            Flush();
            Queued().ShouldBe(new[] { combined });
        }

        [TestCaseSource(nameof(ButtonDirectionCases))]
        public void ButtonThenDirectionInsideWindowCombine(EKeystroke button, EKeystroke direction, EKeystroke combined)
        {
            input.KeystrokeCombiner.Add(combined);
            Frame(0f, button);
            Frame(0.02f, direction);
            Flush();
            Queued().ShouldBe(new[] { combined });
        }

        [Test]
        public void DirectionThenButtonOutsideWindowStaySeparate()
        {
            Frame(0f, EKeystroke.Up);
            Frame(0.06f);
            Frame(0.1f, EKeystroke.A);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.Up, EKeystroke.A });
        }

        [Test]
        public void KnownQuirk_BufferOnlyDrainsAfterPolling()
        {
            input.KeystrokeCombiner.Add(EKeystroke.AUp);
            //Expired buffered input is moved to the queue after the controller is polled,
            //so with no frame in between, a key pressed long after the first still combines with it.
            Frame(0f, EKeystroke.Up);
            Frame(0.1f, EKeystroke.A);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.AUp });
        }

        [Test]
        public void RepeatedDirectionAfterComboIsRedundant()
        {
            input.KeystrokeCombiner.Add(EKeystroke.AUp);
            Frame(0f, EKeystroke.Up, EKeystroke.A);
            Frame(0.02f, EKeystroke.Up);
            Frame(0.03f, EKeystroke.A);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.AUp });
        }

        [Test]
        public void SecondDirectionDoesNotJoinButtonCombo()
        {
            input.KeystrokeCombiner.Add(EKeystroke.AUp);

            //AUp + Forward isn't a combination, so Forward is buffered on its own
            Frame(0f, EKeystroke.Up, EKeystroke.A);
            Frame(0.02f, EKeystroke.Forward);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.AUp, EKeystroke.Forward });
        }

        [TestCase(EKeystroke.Neutral)]
        [TestCase(EKeystroke.UpR)]
        [TestCase(EKeystroke.ForwardShoulder)]
        [TestCase(EKeystroke.ARelease)]
        public void ButtonDoesNotCombineWithNonDirections(EKeystroke other)
        {
            PressTogether(EKeystroke.A, other).ShouldBe(new[] { EKeystroke.A, other }, ignoreOrder: true);
        }

        [Test]
        public void ReleaseDoesNotCombineWithDirection()
        {
            PressTogether(EKeystroke.ARelease, EKeystroke.Up).ShouldBe(new[] { EKeystroke.ARelease, EKeystroke.Up }, ignoreOrder: true);
        }

        #endregion //Button + direction

        #region Button + button

        [Test]
        public void TwoButtonsDoNotCombine()
        {
            PressTogether(EKeystroke.A, EKeystroke.B).ShouldBe(new[] { EKeystroke.A, EKeystroke.B }, ignoreOrder: true);
        }

        [Test]
        public void TriggerAndShoulderDoNotCombine()
        {
            //No way to express trigger + shoulder alt fire in the current tables
            PressTogether(EKeystroke.RTrigger, EKeystroke.RShoulder).ShouldBe(new[] { EKeystroke.RTrigger, EKeystroke.RShoulder }, ignoreOrder: true);
        }

        #endregion //Button + button

        #region Right thumbstick diagonals

        [TestCase(EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR)]
        [TestCase(EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR)]
        [TestCase(EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR)]
        [TestCase(EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR)]
        public void RightStickDiagonalsCombine(EKeystroke vertical, EKeystroke horizontal, EKeystroke combined)
        {
            input.KeystrokeCombiner.Add(combined);

            PressTogether(vertical, horizontal).ShouldBe(new[] { combined });
        }

        [TestCase(EKeystroke.UpR, EKeystroke.DownR)]
        [TestCase(EKeystroke.ForwardR, EKeystroke.BackR)]
        [TestCase(EKeystroke.UpR, EKeystroke.NeutralR)]
        public void OpposingRightStickDirectionsDoNotCombine(EKeystroke first, EKeystroke second)
        {
            PressTogether(first, second).ShouldBe(new[] { first, second }, ignoreOrder: true);
        }

        [Test]
        public void LeftStickDirectionsDoNotCombine()
        {
            //there are no left stick diagonals
            PressTogether(EKeystroke.Up, EKeystroke.Forward).ShouldBe(new[] { EKeystroke.Up, EKeystroke.Forward }, ignoreOrder: true);
        }

        #endregion //Right thumbstick diagonals

        #region Left stick + right stick

        static readonly EKeystroke[] LeftDirections =
        {
            EKeystroke.Up, EKeystroke.Down, EKeystroke.Forward, EKeystroke.Back, EKeystroke.Neutral,
        };

        static IEnumerable<TestCaseData> TwinStickCases()
        {
            var rights = new[] { "Up", "Down", "Forward", "Back", "Neutral" };
            foreach (var left in LeftDirections)
            {
                foreach (var right in rights)
                {
                    var rightKey = Enum.Parse<EKeystroke>($"{right}R");
                    var combined = Enum.Parse<EKeystroke>($"{left}{right}");
                    yield return new TestCaseData(new[] { left, rightKey }, combined)
                        .SetName($"TwinStick_{left}_{rightKey}");
                }
            }
        }

        static IEnumerable<TestCaseData> TwinStickDiagonalCases()
        {
            var diagonals = new[]
            {
                (EKeystroke.UpR, EKeystroke.ForwardR, "UpForward"),
                (EKeystroke.DownR, EKeystroke.ForwardR, "DownForward"),
                (EKeystroke.UpR, EKeystroke.BackR, "UpBack"),
                (EKeystroke.DownR, EKeystroke.BackR, "DownBack"),
            };
            foreach (var left in LeftDirections)
            {
                foreach (var (vertical, horizontal, name) in diagonals)
                {
                    var combined = Enum.Parse<EKeystroke>($"{left}{name}");
                    yield return new TestCaseData(new[] { left, vertical, horizontal }, combined)
                        .SetName($"TwinStickDiagonal_{left}_{name}R");
                }
            }
        }

        [TestCaseSource(nameof(TwinStickCases))]
        [TestCaseSource(nameof(TwinStickDiagonalCases))]
        public void LeftAndRightStickInSameFrameCombine(EKeystroke[] keys, EKeystroke combined)
        {
            input.KeystrokeCombiner.Add(combined);
            PressTogether(keys).ShouldBe(new[] { combined });
        }

        [Test]
        public void KnownBug_LeftStickThenRightDiagonal()
        {
            input.KeystrokeCombiner.Add(EKeystroke.UpUpForward);
            //ForwardR is polled before UpR, so it grabs the buffered Up to make UpForward,
            //and UpR is left on its own.
            //Expected to become UpUpForward once combinations are order independent.
            Frame(0f, EKeystroke.Up);
            Frame(0.02f, EKeystroke.UpR, EKeystroke.ForwardR);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.UpUpForward });
        }

        [Test]
        public void RightDiagonalThenLeftStickCombine()
        {
            input.KeystrokeCombiner.Add(EKeystroke.UpUpForward);
            Frame(0f, EKeystroke.UpR, EKeystroke.ForwardR);
            Frame(0.02f, EKeystroke.Up);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.UpUpForward });
        }

        [Test]
        public void RightStickComponentAfterTwinStickComboIsRedundant()
        {
            input.KeystrokeCombiner.Add(EKeystroke.UpUpForward);
            Frame(0f, EKeystroke.Up, EKeystroke.UpR, EKeystroke.ForwardR);
            Frame(0.02f, EKeystroke.ForwardR);
            Frame(0.03f, EKeystroke.UpForwardR);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.UpUpForward });
        }

        [Test]
        public void ArrivalOrderChangesTwinStickResult()
        {
            input.KeystrokeCombiner.Add(EKeystroke.UpUpForward);
            //Left stick first, then the right stick diagonal one axis at a time.
            //Up + UpR becomes UpUp, and UpUp can't combine with ForwardR, so the diagonal is lost.
            //The same keys pressed in one frame produce UpUpForward (see LeftAndRightStickInSameFrameCombine).
            //Expected to become UpUpForward once combinations are order independent.
            Frame(0f, EKeystroke.Up);
            Frame(0.01f, EKeystroke.UpR);
            Frame(0.02f, EKeystroke.ForwardR);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.UpUpForward });
        }

        #endregion //Left stick + right stick

        #region Move list

        [Test]
        public void ComboDoesntSwallowButtonThatMoveListNeeds()
        {
            //A game that fires on RTrigger misses the shot if a direction is held
            input.AddMove("fire", EKeystroke.RTrigger);
            PressTogether(EKeystroke.RTrigger, EKeystroke.Up);
            input.GetNextMove().ShouldBe("fire");
        }

        [Test]
        public void PlainButtonMatchesMoveList()
        {
            input.AddMove("fire", EKeystroke.RTrigger);
            PressTogether(EKeystroke.RTrigger);
            input.GetNextMove().ShouldBe("fire");
        }

        [Test]
        public void CombinedKeystrokeMatchesMoveList()
        {
            input.AddMove("uppercut", EKeystroke.AUp);
            PressTogether(EKeystroke.A, EKeystroke.Up);
            input.GetNextMove().ShouldBe("uppercut");
        }

        [Test]
        public void HadoukenWithComboedFinalButton()
        {
            input.KeystrokeCombiner.Add(EKeystroke.AForward);
            //Holding forward while pressing A turns the A into AForward, so the hadouken doesn't match
            input.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
            Frame(0f, EKeystroke.Down);
            Frame(0.06f);
            Frame(0.1f, EKeystroke.Forward, EKeystroke.A);
            Flush();
            Queued().ShouldBe(new[] { EKeystroke.Down, EKeystroke.AForward });
            input.GetNextMove().ShouldBeEmpty();
        }

        [Test]
        public void SpecialGun()
        {
            input.AddMove("SpecialGun", EKeystroke.RTriggerRShoulder);
            PressTogether(EKeystroke.RTrigger, EKeystroke.RShoulder);
            input.GetNextMove().ShouldBe("SpecialGun");
        }

        [Test]
        public void SpecialMissile()
        {
            input.AddMove("SpecialMissile", EKeystroke.LTriggerRShoulder);
            PressTogether(EKeystroke.LTrigger, EKeystroke.RShoulder);
            input.GetNextMove().ShouldBe("SpecialMissile");
        }

        [Test]
        public void SuperWeapon()
        {
            input.AddMove("SuperWeapon", EKeystroke.LTriggerRTriggerRShoulder);
            PressTogether(EKeystroke.LTrigger, EKeystroke.RTrigger, EKeystroke.RShoulder);
            input.GetNextMove().ShouldBe("SuperWeapon");
        }

        [Test]
        public void SuperWeaponNotSpecial()
        {
            input.AddMove("SuperWeapon", EKeystroke.LTriggerRTriggerRShoulder);
            input.AddMove("SpecialGun", EKeystroke.RTriggerRShoulder);
            input.AddMove("SpecialMissile", EKeystroke.LTriggerRShoulder);
            PressTogether(EKeystroke.LTrigger, EKeystroke.RTrigger, EKeystroke.RShoulder);
            input.GetNextMove().ShouldBe("SuperWeapon");
        }

        [Test]
        public void SpecialNotSuper()
        {
            input.AddMove("SuperWeapon", EKeystroke.LTriggerRTriggerRShoulder);
            input.AddMove("SpecialGun", EKeystroke.RTriggerRShoulder);
            input.AddMove("SpecialMissile", EKeystroke.LTriggerRShoulder);
            PressTogether(EKeystroke.LTrigger, EKeystroke.RShoulder);
            input.GetNextMove().ShouldBe("SpecialMissile");
        }

        #endregion //Move list
    }
}
