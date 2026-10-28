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
    /// Tests for the button-held keystrokes (AHeld, BHeld, etc.)
    /// These run through the real ControllerWrapper, so held keystrokes are polled the same way they are in a game.
    /// </summary>
    [TestFixture]
    public class HeldButtonTests
    {
        /// <summary>
        /// An input state that reports whatever buttons the test says are down this frame and last frame
        /// </summary>
        class FakeInputState : IInputState
        {
            public HashSet<Buttons> Current { get; set; } = new HashSet<Buttons>();
            public HashSet<Buttons> Previous { get; set; } = new HashSet<Buttons>();

            public bool CheckControllers { get; set; }
            public float DeadZone => 0.25f;
            public float DeadZoneSquared => DeadZone * DeadZone;
            public KeyboardState CurrentKeyboardState => new KeyboardState();
            public KeyboardState LastKeyboardState => new KeyboardState();

            public bool IsConnected(int controllerIndex) => true;
            public Vector2 LeftThumbstick(int controllerIndex) => Vector2.Zero;
            public Vector2 RightThumbstick(int controllerIndex) => Vector2.Zero;
            public bool DPadUp(int controllerIndex) => Current.Contains(Buttons.DPadUp);
            public bool DPadDown(int controllerIndex) => Current.Contains(Buttons.DPadDown);
            public bool DPadLeft(int controllerIndex) => Current.Contains(Buttons.DPadLeft);
            public bool DPadRight(int controllerIndex) => Current.Contains(Buttons.DPadRight);
            public void Update() { }
            public bool IsNewKeyPress(Keys key) => false;
            public bool ButtonDown(int playerIndex, Buttons button) => Current.Contains(button);
            public bool PrevButtonDown(int playerIndex, Buttons button) => Previous.Contains(button);
        }

        //Player 2 doesn't use the keyboard, so only the fake gamepad buttons matter
        const int PlayerIndex = 1;

        FakeInputState inputState;
        ControllerWrapper controller;
        InputWrapper input;
        float time;

        [SetUp]
        public void Setup()
        {
            inputState = new FakeInputState();
            controller = new ControllerWrapper(PlayerIndex);
            time = 0f;
            input = new InputWrapper(controller, () => time);
        }

        #region Helpers

        static readonly (EKeystroke held, EKeystroke press, EKeystroke release, Buttons button)[] HeldButtons =
        {
            (EKeystroke.AHeld, EKeystroke.A, EKeystroke.ARelease, Buttons.A),
            (EKeystroke.BHeld, EKeystroke.B, EKeystroke.BRelease, Buttons.B),
            (EKeystroke.XHeld, EKeystroke.X, EKeystroke.XRelease, Buttons.X),
            (EKeystroke.YHeld, EKeystroke.Y, EKeystroke.YRelease, Buttons.Y),
            (EKeystroke.LShoulderHeld, EKeystroke.LShoulder, EKeystroke.LShoulderRelease, Buttons.LeftShoulder),
            (EKeystroke.RShoulderHeld, EKeystroke.RShoulder, EKeystroke.RShoulderRelease, Buttons.RightShoulder),
            (EKeystroke.LTriggerHeld, EKeystroke.LTrigger, EKeystroke.LTriggerRelease, Buttons.LeftTrigger),
            (EKeystroke.RTriggerHeld, EKeystroke.RTrigger, EKeystroke.RTriggerRelease, Buttons.RightTrigger),
        };

        static IEnumerable<TestCaseData> HeldCases()
        {
            foreach (var (held, _, _, button) in HeldButtons)
            {
                yield return new TestCaseData(held, button).SetName($"{{m}}_{held}");
            }
        }

        static IEnumerable<TestCaseData> HeldPressReleaseCases()
        {
            foreach (var (held, press, release, button) in HeldButtons)
            {
                yield return new TestCaseData(held, press, release, button).SetName($"{{m}}_{held}");
            }
        }

        static IEnumerable<TestCaseData> HeldOtherButtonCases()
        {
            foreach (var (held, _, _, button) in HeldButtons)
            {
                foreach (var (_, _, _, other) in HeldButtons.Where(x => x.button != button))
                {
                    yield return new TestCaseData(held, other).SetName($"{{m}}_{held}_{other}");
                }
            }
        }

        /// <summary>
        /// Update just the controller with the given buttons down this frame
        /// </summary>
        void ControllerFrame(params Buttons[] down)
        {
            inputState.Previous = inputState.Current;
            inputState.Current = new HashSet<Buttons>(down);
            controller.Update(inputState);
        }

        /// <summary>
        /// Run one frame of the input wrapper at the given time with the given buttons down
        /// </summary>
        void Frame(float at, params Buttons[] down)
        {
            time = at;
            inputState.Previous = inputState.Current;
            inputState.Current = new HashSet<Buttons>(down);
            input.Update(inputState, false);
        }

        /// <summary>
        /// Hold the buttons from the start time to the end time, one frame every 1/60th of a second
        /// </summary>
        void Hold(float from, float to, params Buttons[] down)
        {
            for (var at = from; at < to; at += 1f / 60f)
            {
                Frame(at, down);
            }
        }

        static List<EKeystroke> Parse(string text)
        {
            return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => Enum.Parse<EKeystroke>(x))
                .ToList();
        }

        List<EKeystroke> Queued() => Parse(input.ToString());

        List<EKeystroke> Buffered() => Parse(input.GetBufferedInput());

        #endregion //Helpers

        #region ControllerWrapper

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystrokeHeld_TrueOnFirstFrame(EKeystroke held, Buttons button)
        {
            ControllerFrame(button);
            controller.CheckKeystrokeHeld(held).ShouldBeTrue();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystrokeHeld_TrueWhileHeld(EKeystroke held, Buttons button)
        {
            ControllerFrame(button);
            ControllerFrame(button);
            ControllerFrame(button);
            controller.CheckKeystrokeHeld(held).ShouldBeTrue();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystrokeHeld_FalseWhenNotPressed(EKeystroke held, Buttons button)
        {
            ControllerFrame();
            controller.CheckKeystrokeHeld(held).ShouldBeFalse();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystrokeHeld_FalseAfterRelease(EKeystroke held, Buttons button)
        {
            ControllerFrame(button);
            ControllerFrame();
            controller.CheckKeystrokeHeld(held).ShouldBeFalse();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystroke_TrueOnFirstFrame(EKeystroke held, Buttons button)
        {
            ControllerFrame(button);
            controller.CheckKeystroke(held).ShouldBeTrue();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystroke_TrueWhileHeld(EKeystroke held, Buttons button)
        {
            ControllerFrame(button);
            ControllerFrame(button);
            ControllerFrame(button);
            controller.CheckKeystroke(held).ShouldBeTrue();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystroke_FalseWhenNotPressed(EKeystroke held, Buttons button)
        {
            ControllerFrame();
            controller.CheckKeystroke(held).ShouldBeFalse();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystroke_FalseAfterRelease(EKeystroke held, Buttons button)
        {
            ControllerFrame(button);
            ControllerFrame();
            controller.CheckKeystroke(held).ShouldBeFalse();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystroke_FalseAfterReset(EKeystroke held, Buttons button)
        {
            ControllerFrame(button);
            controller.ResetController();
            controller.CheckKeystroke(held).ShouldBeFalse();
            controller.CheckKeystrokeHeld(held).ShouldBeFalse();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystrokeWithDirection_TrueWhileHeld(EKeystroke held, Buttons button)
        {
            ControllerFrame(button);
            ControllerFrame(button);
            controller.CheckKeystroke(held, false, Vector2.UnitX).ShouldBeTrue();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystrokeWithDirection_TrueWhileHeldFlipped(EKeystroke held, Buttons button)
        {
            ControllerFrame(button);
            ControllerFrame(button);
            controller.CheckKeystroke(held, true, -Vector2.UnitX).ShouldBeTrue();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void CheckKeystrokeWithDirection_FalseWhenNotPressed(EKeystroke held, Buttons button)
        {
            ControllerFrame();
            controller.CheckKeystroke(held, false, Vector2.UnitX).ShouldBeFalse();
        }

        [TestCaseSource(nameof(HeldOtherButtonCases))]
        public void OtherButtonDoesNotTriggerHeld(EKeystroke held, Buttons other)
        {
            ControllerFrame(other);
            ControllerFrame(other);
            controller.CheckKeystroke(held).ShouldBeFalse();
            controller.CheckKeystrokeHeld(held).ShouldBeFalse();
        }

        [TestCaseSource(nameof(HeldPressReleaseCases))]
        public void PressOnlyFiresOnFirstFrameButHeldContinues(EKeystroke held, EKeystroke press, EKeystroke release, Buttons button)
        {
            ControllerFrame(button);
            controller.CheckKeystroke(press).ShouldBeTrue();
            controller.CheckKeystroke(held).ShouldBeTrue();

            ControllerFrame(button);
            controller.CheckKeystroke(press).ShouldBeFalse();
            controller.CheckKeystroke(held).ShouldBeTrue();

            ControllerFrame();
            controller.CheckKeystroke(held).ShouldBeFalse();
            controller.CheckKeystroke(release).ShouldBeTrue();
        }

        [Test]
        public void HeldWorksWithRemappedButton()
        {
            //Swap A and B so the held keystrokes follow the mapping, not the physical button
            var map = Mappings.ButtonMaps[PlayerIndex];
            map.A = Buttons.B;
            map.B = Buttons.A;
            try
            {
                ControllerFrame(Buttons.B);
                controller.CheckKeystroke(EKeystroke.AHeld).ShouldBeTrue();
                controller.CheckKeystroke(EKeystroke.BHeld).ShouldBeFalse();
            }
            finally
            {
                map.A = Buttons.A;
                map.B = Buttons.B;
            }
        }

        #endregion //ControllerWrapper

        #region KeystrokeCombiner

        [TestCaseSource(nameof(HeldCases))]
        public void HeldIsRedundantWithItself(EKeystroke held, Buttons button)
        {
            var combiner = new KeystrokeCombiner();
            combiner.Add(held);
            combiner.IsRedundant(held, held).ShouldBeTrue();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void HeldIsNotRedundantBeforeItIsAdded(EKeystroke held, Buttons button)
        {
            var combiner = new KeystrokeCombiner();
            combiner.IsRedundant(held, held).ShouldBeFalse();
        }

        [TestCaseSource(nameof(HeldPressReleaseCases))]
        public void HeldDoesNotSwallowPressOrRelease(EKeystroke held, EKeystroke press, EKeystroke release, Buttons button)
        {
            var combiner = new KeystrokeCombiner();
            combiner.Add(held);
            combiner.Add(press);
            combiner.Add(release);
            combiner.IsRedundant(held, press).ShouldBeFalse();
            combiner.IsRedundant(held, release).ShouldBeFalse();
            combiner.IsRedundant(press, held).ShouldBeFalse();
            combiner.IsRedundant(release, held).ShouldBeFalse();
        }

        [TestCaseSource(nameof(HeldPressReleaseCases))]
        public void HeldDoesNotChordWithPress(EKeystroke held, EKeystroke press, EKeystroke release, Buttons button)
        {
            var combiner = new KeystrokeCombiner();
            combiner.Add(held);
            combiner.Add(press);
            combiner.TryCombine(held, press, out var combined).ShouldBeFalse();
            combined.ShouldBe(held);
            combiner.TryCombine(press, held, out combined).ShouldBeFalse();
            combined.ShouldBe(press);
        }

        [TestCaseSource(nameof(HeldCases))]
        public void HeldDoesNotChordWithDirection(EKeystroke held, Buttons button)
        {
            var combiner = new KeystrokeCombiner();
            combiner.Add(held);
            combiner.Add(EKeystroke.Up);
            combiner.TryCombine(held, EKeystroke.Up, out _).ShouldBeFalse();
            combiner.TryCombine(EKeystroke.Up, held, out _).ShouldBeFalse();
        }

        [Test]
        public void TwoHeldButtonsDoNotChord()
        {
            var combiner = new KeystrokeCombiner();
            combiner.Add(EKeystroke.LTriggerHeld);
            combiner.Add(EKeystroke.RShoulderHeld);
            combiner.TryCombine(EKeystroke.LTriggerHeld, EKeystroke.RShoulderHeld, out _).ShouldBeFalse();
        }

        #endregion //KeystrokeCombiner

        #region Input buffer

        [TestCaseSource(nameof(HeldPressReleaseCases))]
        public void PressFrameBuffersHeldAndPress(EKeystroke held, EKeystroke press, EKeystroke release, Buttons button)
        {
            //shoulders also fire ForwardShoulder/BackShoulder, so just check both are there
            Frame(0f, button);
            Buffered().ShouldContain(held);
            Buffered().ShouldContain(press);
        }

        [TestCaseSource(nameof(HeldCases))]
        public void HeldIsBufferedOnceInsideWindow(EKeystroke held, Buttons button)
        {
            Frame(0f, button);
            Frame(0.02f, button);
            Frame(0.04f, button);
            Buffered().Count(x => x == held).ShouldBe(1);
        }

        [TestCaseSource(nameof(HeldCases))]
        public void HeldKeepsQueueingWhileHeld(EKeystroke held, Buttons button)
        {
            Hold(0f, 0.3f, button);
            Queued().Count(x => x == held).ShouldBeGreaterThan(1);
        }

        [TestCaseSource(nameof(HeldCases))]
        public void HeldStopsQueueingAfterRelease(EKeystroke held, Buttons button)
        {
            Hold(0f, 0.2f, button);
            Frame(0.2f);
            Frame(0.3f);
            var count = Queued().Count(x => x == held);

            Frame(0.35f);
            Frame(0.4f);
            Queued().Count(x => x == held).ShouldBeLessThanOrEqualTo(count);
        }

        [TestCaseSource(nameof(HeldPressReleaseCases))]
        public void ReleaseIsQueuedAfterHeld(EKeystroke held, EKeystroke press, EKeystroke release, Buttons button)
        {
            Hold(0f, 0.2f, button);
            Frame(0.2f);
            Frame(0.3f);
            var queued = Queued();
            queued.LastIndexOf(held).ShouldBeLessThan(queued.IndexOf(release));
        }

        #endregion //Input buffer

        #region Move list

        [TestCaseSource(nameof(HeldCases))]
        public void HeldMoveMatchesWhileHeld(EKeystroke held, Buttons button)
        {
            input.AddMove("charge", held);
            Hold(0f, 0.2f, button);
            input.GetNextMove().ShouldBe("charge");
        }

        [TestCaseSource(nameof(HeldCases))]
        public void HeldMoveDoesNotMatchWithoutButton(EKeystroke held, Buttons button)
        {
            input.AddMove("charge", held);
            Hold(0f, 0.2f);
            input.GetNextMove().ShouldBeEmpty();
        }

        [TestCaseSource(nameof(HeldOtherButtonCases))]
        public void HeldMoveDoesNotMatchOtherButton(EKeystroke held, Buttons other)
        {
            input.AddMove("charge", held);
            Hold(0f, 0.2f, other);
            input.GetNextMove().ShouldBeEmpty();
        }

        [TestCaseSource(nameof(HeldCases))]
        public void HeldMoveMatchesAgainWhileStillHeld(EKeystroke held, Buttons button)
        {
            //Holding the button keeps queueing the held keystroke, so the move can fire repeatedly (autofire)
            input.AddMove("charge", held);
            Hold(0f, 0.2f, button);
            input.GetNextMove().ShouldBe("charge");
            Hold(0.2f, 0.4f, button);
            input.GetNextMove().ShouldBe("charge");
        }

        [TestCaseSource(nameof(HeldPressReleaseCases))]
        public void ChargeAndReleaseMove(EKeystroke held, EKeystroke press, EKeystroke release, Buttons button)
        {
            input.AddMove("charge shot", held, release);
            Hold(0f, 0.3f, button);
            input.GetNextMove().ShouldBeEmpty();

            Frame(0.3f);
            Frame(0.4f);
            input.GetNextMove().ShouldBe("charge shot");
        }

        [TestCaseSource(nameof(HeldPressReleaseCases))]
        public void TapDoesNotNeedHeldMove(EKeystroke held, EKeystroke press, EKeystroke release, Buttons button)
        {
            //A plain press move still works when the button is only tapped
            input.AddMove("tap", press);
            Frame(0f, button);
            Frame(0.02f);
            Frame(0.1f);
            input.GetNextMove().ShouldBe("tap");
        }

        [TestCaseSource(nameof(HeldPressReleaseCases))]
        public void PressMoveStillMatchesWhenButtonIsHeld(EKeystroke held, EKeystroke press, EKeystroke release, Buttons button)
        {
            input.AddMove("tap", press);
            Hold(0f, 0.2f, button);
            input.GetNextMove().ShouldBe("tap");
        }

        [Test]
        public void HeldButtonPlusPressedButton()
        {
            //hold the trigger to aim, tap A to fire
            input.AddMove("aimed shot", EKeystroke.LTriggerHeld, EKeystroke.A);
            Hold(0f, 0.2f, Buttons.LeftTrigger);
            Frame(0.2f, Buttons.LeftTrigger, Buttons.A);
            Hold(0.21f, 0.3f, Buttons.LeftTrigger);
            var queued = input.ToString();
            input.GetNextMove().ShouldBe("aimed shot", queued);
        }

        [Test]
        public void HadoukenStillWorksWithHeldButtonsPolled()
        {
            //AHeld is polled every frame now, so make sure it doesn't get between Forward and A
            input.AddMove("hadouken", EKeystroke.Down, EKeystroke.Forward, EKeystroke.A);
            Frame(0f, Buttons.DPadDown);
            Frame(0.1f, Buttons.DPadRight);
            Frame(0.2f, Buttons.DPadRight, Buttons.A);
            Frame(0.3f);
            var queued = input.ToString();
            input.GetNextMove().ShouldBe("hadouken", queued);
        }

        [Test]
        public void ButtonSequenceWhileFirstButtonHeld()
        {
            //AHeld is polled every frame now, so make sure it doesn't get between A and B
            input.AddMove("one two", EKeystroke.A, EKeystroke.B);
            Frame(0f, Buttons.A);
            Frame(0.1f, Buttons.A, Buttons.B);
            Frame(0.2f);
            var queued = input.ToString();
            input.GetNextMove().ShouldBe("one two", queued);
        }

        #endregion //Move list
    }
}
