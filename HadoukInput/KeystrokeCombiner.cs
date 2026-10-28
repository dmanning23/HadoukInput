
using System;
using System.Collections.Generic;

namespace HadoukInput
{
    public class KeystrokeCombiner
    {
        // combo -> the base keystrokes it's made of
        private Dictionary<EKeystroke, EKeystroke[]> Combinations { get; set; } = new Dictionary<EKeystroke, EKeystroke[]>();
        private Dictionary<EKeystroke, Dictionary<EKeystroke, EKeystroke>> Chords { get; set; } = new Dictionary<EKeystroke, Dictionary<EKeystroke, EKeystroke>>();

        public void Add(EKeystroke combo)
        {
            switch (combo)
            {
                //All the basic keystrokes

                // keyboard\dpad\left-thumbstick directions
                case EKeystroke.Up:
                    {
                        AddCombination(EKeystroke.Up, [EKeystroke.Up]);
                    }
                    break;
                case EKeystroke.Down:
                    {
                        AddCombination(EKeystroke.Down, [EKeystroke.Down]);
                    }
                    break;
                case EKeystroke.Forward:
                    {
                        AddCombination(EKeystroke.Forward, [EKeystroke.Forward]);
                    }
                    break;
                case EKeystroke.Back:
                    {
                        AddCombination(EKeystroke.Back, [EKeystroke.Back]);
                    }
                    break;
                case EKeystroke.Neutral:
                    {
                        AddCombination(EKeystroke.Neutral, [EKeystroke.Neutral]);
                    }
                    break;

                // right-thumbstick directions
                case EKeystroke.UpR:
                    {
                        AddCombination(EKeystroke.UpR, [EKeystroke.UpR]);
                    }
                    break;
                case EKeystroke.DownR:
                    {
                        AddCombination(EKeystroke.DownR, [EKeystroke.DownR]);
                    }
                    break;
                case EKeystroke.ForwardR:
                    {
                        AddCombination(EKeystroke.ForwardR, [EKeystroke.ForwardR]);
                    }
                    break;
                case EKeystroke.BackR:
                    {
                        AddCombination(EKeystroke.BackR, [EKeystroke.BackR]);
                    }
                    break;
                case EKeystroke.NeutralR:
                    {
                        AddCombination(EKeystroke.NeutralR, [EKeystroke.NeutralR]);
                    }
                    break;

                //button-down keystrokes
                case EKeystroke.A:
                    {
                        AddCombination(EKeystroke.A, [EKeystroke.A]);
                    }
                    break;
                case EKeystroke.B:
                    {
                        AddCombination(EKeystroke.B, [EKeystroke.B]);
                    }
                    break;
                case EKeystroke.X:
                    {
                        AddCombination(EKeystroke.X, [EKeystroke.X]);
                    }
                    break;
                case EKeystroke.Y:
                    {
                        AddCombination(EKeystroke.Y, [EKeystroke.Y]);
                    }
                    break;
                case EKeystroke.LShoulder:
                    {
                        AddCombination(EKeystroke.LShoulder, [EKeystroke.LShoulder]);
                    }
                    break;
                case EKeystroke.RShoulder:
                    {
                        AddCombination(EKeystroke.RShoulder, [EKeystroke.RShoulder]);
                    }
                    break;
                case EKeystroke.LTrigger:
                    {
                        AddCombination(EKeystroke.LTrigger, [EKeystroke.LTrigger]);
                    }
                    break;
                case EKeystroke.RTrigger:
                    {
                        AddCombination(EKeystroke.RTrigger, [EKeystroke.RTrigger]);
                    }
                    break;

                //Check some special stuff for the shoulder buttons...
                case EKeystroke.ForwardShoulder:
                    {
                        //shoulder in the direction the player is facing
                        AddCombination(EKeystroke.ForwardShoulder, [EKeystroke.ForwardShoulder]);
                    }
                    break;
                case EKeystroke.BackShoulder:
                    {
                        //shoulder in the other direction
                        AddCombination(EKeystroke.BackShoulder, [EKeystroke.BackShoulder]);
                    }
                    break;

                //button-up keystrokes
                case EKeystroke.ARelease:
                    {
                        AddCombination(EKeystroke.ARelease, [EKeystroke.ARelease]);
                    }
                    break;
                case EKeystroke.BRelease:
                    {
                        AddCombination(EKeystroke.BRelease, [EKeystroke.BRelease]);
                    }
                    break;
                case EKeystroke.XRelease:
                    {
                        AddCombination(EKeystroke.XRelease, [EKeystroke.XRelease]);
                    }
                    break;
                case EKeystroke.YRelease:
                    {
                        AddCombination(EKeystroke.YRelease, [EKeystroke.YRelease]);
                    }
                    break;
                case EKeystroke.LShoulderRelease:
                    {
                        AddCombination(EKeystroke.LShoulderRelease, [EKeystroke.LShoulderRelease]);
                    }
                    break;
                case EKeystroke.RShoulderRelease:
                    {
                        AddCombination(EKeystroke.RShoulderRelease, [EKeystroke.RShoulderRelease]);
                    }
                    break;
                case EKeystroke.LTriggerRelease:
                    {
                        AddCombination(EKeystroke.LTriggerRelease, [EKeystroke.LTriggerRelease]);
                    }
                    break;
                case EKeystroke.RTriggerRelease:
                    {
                        AddCombination(EKeystroke.RTriggerRelease, [EKeystroke.RTriggerRelease]);
                    }
                    break;

                //Chord the shoulder + trigger buttons
                case EKeystroke.LTriggerLShoulder:
                    {
                        AddCombination(EKeystroke.LTriggerLShoulder, [EKeystroke.LTrigger, EKeystroke.LShoulder]);
                        AddChord(EKeystroke.LTrigger, EKeystroke.LShoulder, EKeystroke.LTriggerLShoulder);
                    }
                    break;
                case EKeystroke.LTriggerRShoulder:
                    {
                        AddCombination(EKeystroke.LTriggerRShoulder, [EKeystroke.LTrigger, EKeystroke.RShoulder]);
                        AddChord(EKeystroke.LTrigger, EKeystroke.RShoulder, EKeystroke.LTriggerRShoulder);
                    }
                    break;
                case EKeystroke.RTriggerLShoulder:
                    {
                        AddCombination(EKeystroke.RTriggerLShoulder, [EKeystroke.RTrigger, EKeystroke.LShoulder]);
                        AddChord(EKeystroke.RTrigger, EKeystroke.LShoulder, EKeystroke.RTriggerLShoulder);
                    }
                    break;
                case EKeystroke.RTriggerRShoulder:
                    {
                        AddCombination(EKeystroke.RTriggerRShoulder, [EKeystroke.RTrigger, EKeystroke.RShoulder]);
                        AddChord(EKeystroke.RTrigger, EKeystroke.RShoulder, EKeystroke.RTriggerRShoulder);
                    }
                    break;

                //Both trigger chords
                case EKeystroke.LTriggerRTriggerLShoulder:
                    {
                        AddCombination(EKeystroke.LTriggerRTriggerLShoulder,
                        [
                            EKeystroke.LTrigger,
                            EKeystroke.RTrigger,
                            EKeystroke.LShoulder,
                            EKeystroke.LTriggerLShoulder,
                            EKeystroke.RTriggerLShoulder,
                        ]);
                        AddChord(EKeystroke.LTrigger, EKeystroke.RTriggerLShoulder, EKeystroke.LTriggerRTriggerLShoulder);
                        AddChord(EKeystroke.RTrigger, EKeystroke.LTriggerLShoulder, EKeystroke.LTriggerRTriggerLShoulder);
                    }
                    break;
                case EKeystroke.LTriggerRTriggerRShoulder:
                    {
                        AddCombination(EKeystroke.LTriggerRTriggerLShoulder,
                        [
                            EKeystroke.LTrigger,
                            EKeystroke.RTrigger,
                            EKeystroke.RShoulder,
                            EKeystroke.LTriggerRShoulder,
                            EKeystroke.RTriggerRShoulder,
                        ]);
                        AddChord(EKeystroke.LTrigger, EKeystroke.RTriggerRShoulder, EKeystroke.LTriggerRTriggerRShoulder);
                        AddChord(EKeystroke.RTrigger, EKeystroke.LTriggerRShoulder, EKeystroke.LTriggerRTriggerRShoulder);
                    }
                    break;

                //diagonals on right thumbstick
                case EKeystroke.UpForwardR:
                    {
                        AddCombination(EKeystroke.UpForwardR, [EKeystroke.UpR, EKeystroke.ForwardR]);
                        AddChord(EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR);
                    }
                    break;
                case EKeystroke.DownForwardR:
                    {
                        AddCombination(EKeystroke.DownForwardR, [EKeystroke.DownR, EKeystroke.ForwardR]);
                        AddChord(EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR);
                    }
                    break;
                case EKeystroke.UpBackR:
                    {
                        AddCombination(EKeystroke.UpBackR, [EKeystroke.UpR, EKeystroke.BackR]);
                        AddChord(EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR);
                    }
                    break;
                case EKeystroke.DownBackR:
                    {
                        AddCombination(EKeystroke.DownBackR, [EKeystroke.DownR, EKeystroke.BackR]);
                        AddChord(EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR);
                    }
                    break;

                //keystrokes + directions
                case EKeystroke.AUp:
                    {
                        AddCombination(EKeystroke.AUp, [EKeystroke.A, EKeystroke.Up]);
                        AddChord(EKeystroke.A, EKeystroke.Up, EKeystroke.AUp);
                    }
                    break;
                case EKeystroke.ADown:
                    {
                        AddCombination(EKeystroke.ADown, [EKeystroke.A, EKeystroke.Down]);
                        AddChord(EKeystroke.A, EKeystroke.Down, EKeystroke.ADown);
                    }
                    break;
                case EKeystroke.AForward:
                    {
                        AddCombination(EKeystroke.AForward, [EKeystroke.A, EKeystroke.Forward]);
                        AddChord(EKeystroke.A, EKeystroke.Forward, EKeystroke.AForward);
                    }
                    break;
                case EKeystroke.ABack:
                    {
                        AddCombination(EKeystroke.ABack, [EKeystroke.A, EKeystroke.Back]);
                        AddChord(EKeystroke.A, EKeystroke.Back, EKeystroke.ABack);
                    }
                    break;
                case EKeystroke.BUp:
                    {
                        AddCombination(EKeystroke.BUp, [EKeystroke.B, EKeystroke.Up]);
                        AddChord(EKeystroke.B, EKeystroke.Up, EKeystroke.BUp);
                    }
                    break;
                case EKeystroke.BDown:
                    {
                        AddCombination(EKeystroke.BDown, [EKeystroke.B, EKeystroke.Down]);
                        AddChord(EKeystroke.B, EKeystroke.Down, EKeystroke.BDown);
                    }
                    break;
                case EKeystroke.BForward:
                    {
                        AddCombination(EKeystroke.BForward, [EKeystroke.B, EKeystroke.Forward]);
                        AddChord(EKeystroke.B, EKeystroke.Forward, EKeystroke.BForward);
                    }
                    break;
                case EKeystroke.BBack:
                    {
                        AddCombination(EKeystroke.BBack, [EKeystroke.B, EKeystroke.Back]);
                        AddChord(EKeystroke.B, EKeystroke.Back, EKeystroke.BBack);
                    }
                    break;
                case EKeystroke.XUp:
                    {
                        AddCombination(EKeystroke.XUp, [EKeystroke.X, EKeystroke.Up]);
                        AddChord(EKeystroke.X, EKeystroke.Up, EKeystroke.XUp);
                    }
                    break;
                case EKeystroke.XDown:
                    {
                        AddCombination(EKeystroke.XDown, [EKeystroke.X, EKeystroke.Down]);
                        AddChord(EKeystroke.X, EKeystroke.Down, EKeystroke.XDown);
                    }
                    break;
                case EKeystroke.XForward:
                    {
                        AddCombination(EKeystroke.XForward, [EKeystroke.X, EKeystroke.Forward]);
                        AddChord(EKeystroke.X, EKeystroke.Forward, EKeystroke.XForward);
                    }
                    break;
                case EKeystroke.XBack:
                    {
                        AddCombination(EKeystroke.XBack, [EKeystroke.X, EKeystroke.Back]);
                        AddChord(EKeystroke.X, EKeystroke.Back, EKeystroke.XBack);
                    }
                    break;
                case EKeystroke.YUp:
                    {
                        AddCombination(EKeystroke.YUp, [EKeystroke.Y, EKeystroke.Up]);
                        AddChord(EKeystroke.Y, EKeystroke.Up, EKeystroke.YUp);
                    }
                    break;
                case EKeystroke.YDown:
                    {
                        AddCombination(EKeystroke.YDown, [EKeystroke.Y, EKeystroke.Down]);
                        AddChord(EKeystroke.Y, EKeystroke.Down, EKeystroke.YDown);
                    }
                    break;
                case EKeystroke.YForward:
                    {
                        AddCombination(EKeystroke.YForward, [EKeystroke.Y, EKeystroke.Forward]);
                        AddChord(EKeystroke.Y, EKeystroke.Forward, EKeystroke.YForward);
                    }
                    break;
                case EKeystroke.YBack:
                    {
                        AddCombination(EKeystroke.YBack, [EKeystroke.Y, EKeystroke.Back]);
                        AddChord(EKeystroke.Y, EKeystroke.Back, EKeystroke.YBack);
                    }
                    break;
                case EKeystroke.LShoulderUp:
                    {
                        AddCombination(EKeystroke.LShoulderUp, [EKeystroke.LShoulder, EKeystroke.Up]);
                        AddChord(EKeystroke.LShoulder, EKeystroke.Up, EKeystroke.LShoulderUp);
                    }
                    break;
                case EKeystroke.LShoulderDown:
                    {
                        AddCombination(EKeystroke.LShoulderDown, [EKeystroke.LShoulder, EKeystroke.Down]);
                        AddChord(EKeystroke.LShoulder, EKeystroke.Down, EKeystroke.LShoulderDown);
                    }
                    break;
                case EKeystroke.LShoulderForward:
                    {
                        AddCombination(EKeystroke.LShoulderForward, [EKeystroke.LShoulder, EKeystroke.Forward]);
                        AddChord(EKeystroke.LShoulder, EKeystroke.Forward, EKeystroke.LShoulderForward);
                    }
                    break;
                case EKeystroke.LShoulderBack:
                    {
                        AddCombination(EKeystroke.LShoulderBack, [EKeystroke.LShoulder, EKeystroke.Back]);
                        AddChord(EKeystroke.LShoulder, EKeystroke.Back, EKeystroke.LShoulderBack);
                    }
                    break;
                case EKeystroke.RShoulderUp:
                    {
                        AddCombination(EKeystroke.RShoulderUp, [EKeystroke.RShoulder, EKeystroke.Up]);
                        AddChord(EKeystroke.RShoulder, EKeystroke.Up, EKeystroke.RShoulderUp);
                    }
                    break;
                case EKeystroke.RShoulderDown:
                    {
                        AddCombination(EKeystroke.RShoulderDown, [EKeystroke.RShoulder, EKeystroke.Down]);
                        AddChord(EKeystroke.RShoulder, EKeystroke.Down, EKeystroke.RShoulderDown);
                    }
                    break;
                case EKeystroke.RShoulderForward:
                    {
                        AddCombination(EKeystroke.RShoulderForward, [EKeystroke.RShoulder, EKeystroke.Forward]);
                        AddChord(EKeystroke.RShoulder, EKeystroke.Forward, EKeystroke.RShoulderForward);
                    }
                    break;
                case EKeystroke.RShoulderBack:
                    {
                        AddCombination(EKeystroke.RShoulderBack, [EKeystroke.RShoulder, EKeystroke.Back]);
                        AddChord(EKeystroke.RShoulder, EKeystroke.Back, EKeystroke.RShoulderBack);
                    }
                    break;
                case EKeystroke.LTriggerUp:
                    {
                        AddCombination(EKeystroke.LTriggerUp, [EKeystroke.LTrigger, EKeystroke.Up]);
                        AddChord(EKeystroke.LTrigger, EKeystroke.Up, EKeystroke.LTriggerUp);
                    }
                    break;
                case EKeystroke.LTriggerDown:
                    {
                        AddCombination(EKeystroke.LTriggerDown, [EKeystroke.LTrigger, EKeystroke.Down]);
                        AddChord(EKeystroke.LTrigger, EKeystroke.Down, EKeystroke.LTriggerDown);
                    }
                    break;
                case EKeystroke.LTriggerForward:
                    {
                        AddCombination(EKeystroke.LTriggerForward, [EKeystroke.LTrigger, EKeystroke.Forward]);
                        AddChord(EKeystroke.LTrigger, EKeystroke.Forward, EKeystroke.LTriggerForward);
                    }
                    break;
                case EKeystroke.LTriggerBack:
                    {
                        AddCombination(EKeystroke.LTriggerBack, [EKeystroke.LTrigger, EKeystroke.Back]);
                        AddChord(EKeystroke.LTrigger, EKeystroke.Back, EKeystroke.LTriggerBack);
                    }
                    break;
                case EKeystroke.RTriggerUp:
                    {
                        AddCombination(EKeystroke.RTriggerUp, [EKeystroke.RTrigger, EKeystroke.Up]);
                        AddChord(EKeystroke.RTrigger, EKeystroke.Up, EKeystroke.RTriggerUp);
                    }
                    break;
                case EKeystroke.RTriggerDown:
                    {
                        AddCombination(EKeystroke.RTriggerDown, [EKeystroke.RTrigger, EKeystroke.Down]);
                        AddChord(EKeystroke.RTrigger, EKeystroke.Down, EKeystroke.RTriggerDown);
                    }
                    break;
                case EKeystroke.RTriggerForward:
                    {
                        AddCombination(EKeystroke.RTriggerForward, [EKeystroke.RTrigger, EKeystroke.Forward]);
                        AddChord(EKeystroke.RTrigger, EKeystroke.Forward, EKeystroke.RTriggerForward);
                    }
                    break;
                case EKeystroke.RTriggerBack:
                    {
                        AddCombination(EKeystroke.RTriggerBack, [EKeystroke.RTrigger, EKeystroke.Back]);
                        AddChord(EKeystroke.RTrigger, EKeystroke.Back, EKeystroke.RTriggerBack);
                    }
                    break;

                //left_right thumbstick combination directions
                case EKeystroke.UpUp:
                    {
                        AddCombination(EKeystroke.UpUp, [EKeystroke.Up, EKeystroke.UpR]);
                        AddChord(EKeystroke.Up, EKeystroke.UpR, EKeystroke.UpUp);
                    }
                    break;
                case EKeystroke.UpUpForward:
                    {
                        AddCombination(EKeystroke.UpUpForward, [EKeystroke.Up, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR]);
                        AddChord(EKeystroke.Up, EKeystroke.UpForwardR, EKeystroke.UpUpForward);
                        AddChord(EKeystroke.UpUp, EKeystroke.ForwardR, EKeystroke.UpUpForward);
                        AddChord(EKeystroke.UpForward, EKeystroke.UpR, EKeystroke.UpUpForward);
                    }
                    break;
                case EKeystroke.UpForward:
                    {
                        AddCombination(EKeystroke.UpForward, [EKeystroke.Up, EKeystroke.ForwardR]);
                        AddChord(EKeystroke.Up, EKeystroke.ForwardR, EKeystroke.UpForward);
                    }
                    break;
                case EKeystroke.UpDownForward:
                    {
                        AddCombination(EKeystroke.UpDownForward, [EKeystroke.Up, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR]);
                        AddChord(EKeystroke.Up, EKeystroke.DownForwardR, EKeystroke.UpDownForward);
                        AddChord(EKeystroke.UpDown, EKeystroke.ForwardR, EKeystroke.UpDownForward);
                        AddChord(EKeystroke.UpForward, EKeystroke.DownR, EKeystroke.UpDownForward);
                    }
                    break;
                case EKeystroke.UpDown:
                    {
                        AddCombination(EKeystroke.UpDown, [EKeystroke.Up, EKeystroke.DownR]);
                        AddChord(EKeystroke.Up, EKeystroke.DownR, EKeystroke.UpDown);
                    }
                    break;
                case EKeystroke.UpDownBack:
                    {
                        AddCombination(EKeystroke.UpDownBack, [EKeystroke.Up, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR]);
                        AddChord(EKeystroke.Up, EKeystroke.DownBackR, EKeystroke.UpDownBack);
                        AddChord(EKeystroke.UpDown, EKeystroke.BackR, EKeystroke.UpDownBack);
                        AddChord(EKeystroke.UpBack, EKeystroke.DownR, EKeystroke.UpDownBack);
                    }
                    break;
                case EKeystroke.UpBack:
                    {
                        AddCombination(EKeystroke.UpBack, [EKeystroke.Up, EKeystroke.BackR]);
                        AddChord(EKeystroke.Up, EKeystroke.BackR, EKeystroke.UpBack);
                    }
                    break;
                case EKeystroke.UpUpBack:
                    {
                        AddCombination(EKeystroke.UpUpBack, [EKeystroke.Up, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR]);
                        AddChord(EKeystroke.Up, EKeystroke.UpBackR, EKeystroke.UpUpBack);
                        AddChord(EKeystroke.UpBack, EKeystroke.UpR, EKeystroke.UpUpBack);
                        AddChord(EKeystroke.UpUp, EKeystroke.BackR, EKeystroke.UpUpBack);
                    }
                    break;
                case EKeystroke.UpNeutral:
                    {
                        AddCombination(EKeystroke.UpNeutral, [EKeystroke.Up, EKeystroke.NeutralR]);
                        AddChord(EKeystroke.Up, EKeystroke.NeutralR, EKeystroke.UpNeutral);
                    }
                    break;

                case EKeystroke.ForwardUp:
                    {
                        AddCombination(EKeystroke.ForwardUp, [EKeystroke.Forward, EKeystroke.UpR]);
                        AddChord(EKeystroke.Forward, EKeystroke.UpR, EKeystroke.ForwardUp);
                    }
                    break;
                case EKeystroke.ForwardUpForward:
                    {
                        AddCombination(EKeystroke.ForwardUpForward, [EKeystroke.Forward, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR]);
                        AddChord(EKeystroke.Forward, EKeystroke.UpForwardR, EKeystroke.ForwardUpForward);
                        AddChord(EKeystroke.ForwardForward, EKeystroke.UpR, EKeystroke.ForwardUpForward);
                        AddChord(EKeystroke.ForwardUp, EKeystroke.ForwardR, EKeystroke.ForwardUpForward);
                    }
                    break;
                case EKeystroke.ForwardForward:
                    {
                        AddCombination(EKeystroke.ForwardForward, [EKeystroke.Forward, EKeystroke.ForwardR]);
                        AddChord(EKeystroke.Forward, EKeystroke.ForwardR, EKeystroke.ForwardForward);
                    }
                    break;
                case EKeystroke.ForwardDownForward:
                    {
                        AddCombination(EKeystroke.ForwardDownForward, [EKeystroke.Forward, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR]);
                        AddChord(EKeystroke.Forward, EKeystroke.DownForwardR, EKeystroke.ForwardDownForward);
                        AddChord(EKeystroke.ForwardDown, EKeystroke.ForwardR, EKeystroke.ForwardDownForward);
                        AddChord(EKeystroke.ForwardForward, EKeystroke.DownR, EKeystroke.ForwardDownForward);
                    }
                    break;
                case EKeystroke.ForwardDown:
                    {
                        AddCombination(EKeystroke.ForwardDown, [EKeystroke.Forward, EKeystroke.DownR]);
                        AddChord(EKeystroke.Forward, EKeystroke.DownR, EKeystroke.ForwardDown);
                    }
                    break;
                case EKeystroke.ForwardDownBack:
                    {
                        AddCombination(EKeystroke.ForwardDownBack, [EKeystroke.Forward, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR]);
                        AddChord(EKeystroke.Forward, EKeystroke.DownBackR, EKeystroke.ForwardDownBack);
                        AddChord(EKeystroke.ForwardDown, EKeystroke.BackR, EKeystroke.ForwardDownBack);
                        AddChord(EKeystroke.ForwardBack, EKeystroke.DownR, EKeystroke.ForwardDownBack);
                    }
                    break;
                case EKeystroke.ForwardBack:
                    {
                        AddCombination(EKeystroke.ForwardBack, [EKeystroke.Forward, EKeystroke.BackR]);
                        AddChord(EKeystroke.Forward, EKeystroke.BackR, EKeystroke.ForwardBack);
                    }
                    break;
                case EKeystroke.ForwardUpBack:
                    {
                        AddCombination(EKeystroke.ForwardUpBack, [EKeystroke.Forward, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR]);
                        AddChord(EKeystroke.Forward, EKeystroke.UpBackR, EKeystroke.ForwardUpBack);
                        AddChord(EKeystroke.ForwardUp, EKeystroke.BackR, EKeystroke.ForwardUpBack);
                        AddChord(EKeystroke.ForwardBack, EKeystroke.UpR, EKeystroke.ForwardUpBack);
                    }
                    break;
                case EKeystroke.ForwardNeutral:
                    {
                        AddCombination(EKeystroke.ForwardNeutral, [EKeystroke.Forward, EKeystroke.NeutralR]);
                        AddChord(EKeystroke.Forward, EKeystroke.NeutralR, EKeystroke.ForwardNeutral);
                    }
                    break;

                case EKeystroke.DownUp:
                    {
                        AddCombination(EKeystroke.DownUp, [EKeystroke.Down, EKeystroke.UpR]);
                        AddChord(EKeystroke.Down, EKeystroke.UpR, EKeystroke.DownUp);
                    }
                    break;
                case EKeystroke.DownUpForward:
                    {
                        AddCombination(EKeystroke.DownUpForward, [EKeystroke.Down, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR]);
                        AddChord(EKeystroke.Down, EKeystroke.UpForwardR, EKeystroke.DownUpForward);
                        AddChord(EKeystroke.DownUp, EKeystroke.ForwardR, EKeystroke.DownUpForward);
                        AddChord(EKeystroke.DownForward, EKeystroke.UpR, EKeystroke.DownUpForward);
                    }
                    break;
                case EKeystroke.DownForward:
                    {
                        AddCombination(EKeystroke.DownForward, [EKeystroke.Down, EKeystroke.ForwardR]);
                        AddChord(EKeystroke.Down, EKeystroke.ForwardR, EKeystroke.DownForward);
                    }
                    break;
                case EKeystroke.DownDownForward:
                    {
                        AddCombination(EKeystroke.DownDownForward, [EKeystroke.Down, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR]);
                        AddChord(EKeystroke.Down, EKeystroke.DownForwardR, EKeystroke.DownDownForward);
                        AddChord(EKeystroke.DownDown, EKeystroke.ForwardR, EKeystroke.DownDownForward);
                        AddChord(EKeystroke.DownForward, EKeystroke.DownR, EKeystroke.DownDownForward);
                    }
                    break;
                case EKeystroke.DownDown:
                    {
                        AddCombination(EKeystroke.DownDown, [EKeystroke.Down, EKeystroke.DownR]);
                        AddChord(EKeystroke.Down, EKeystroke.DownR, EKeystroke.DownDown);
                    }
                    break;
                case EKeystroke.DownDownBack:
                    {
                        AddCombination(EKeystroke.DownDownBack, [EKeystroke.Down, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR]);
                        AddChord(EKeystroke.Down, EKeystroke.DownBackR, EKeystroke.DownDownBack);
                        AddChord(EKeystroke.DownDown, EKeystroke.BackR, EKeystroke.DownDownBack);
                        AddChord(EKeystroke.DownBack, EKeystroke.DownR, EKeystroke.DownDownBack);
                    }
                    break;
                case EKeystroke.DownBack:
                    {
                        AddCombination(EKeystroke.DownBack, [EKeystroke.Down, EKeystroke.BackR]);
                        AddChord(EKeystroke.Down, EKeystroke.BackR, EKeystroke.DownBack);
                    }
                    break;
                case EKeystroke.DownUpBack:
                    {
                        AddCombination(EKeystroke.DownUpBack, [EKeystroke.Down, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR]);
                        AddChord(EKeystroke.Down, EKeystroke.UpBackR, EKeystroke.DownUpBack);
                        AddChord(EKeystroke.DownUp, EKeystroke.BackR, EKeystroke.DownUpBack);
                        AddChord(EKeystroke.DownBack, EKeystroke.UpR, EKeystroke.DownUpBack);
                    }
                    break;
                case EKeystroke.DownNeutral:
                    {
                        AddCombination(EKeystroke.DownNeutral, [EKeystroke.Down, EKeystroke.NeutralR]);
                        AddChord(EKeystroke.Down, EKeystroke.NeutralR, EKeystroke.DownNeutral);
                    }
                    break;

                case EKeystroke.BackUp:
                    {
                        AddCombination(EKeystroke.BackUp, [EKeystroke.Back, EKeystroke.UpR]);
                        AddChord(EKeystroke.Back, EKeystroke.UpR, EKeystroke.BackUp);
                    }
                    break;
                case EKeystroke.BackUpForward:
                    {
                        AddCombination(EKeystroke.BackUpForward, [EKeystroke.Back, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR]);
                        AddChord(EKeystroke.Back, EKeystroke.UpForwardR, EKeystroke.BackUpForward);
                        AddChord(EKeystroke.BackUp, EKeystroke.ForwardR, EKeystroke.BackUpForward);
                        AddChord(EKeystroke.BackForward, EKeystroke.UpR, EKeystroke.BackUpForward);
                    }
                    break;
                case EKeystroke.BackForward:
                    {
                        AddCombination(EKeystroke.BackForward, [EKeystroke.Back, EKeystroke.ForwardR]);
                        AddChord(EKeystroke.Back, EKeystroke.ForwardR, EKeystroke.BackForward);
                    }
                    break;
                case EKeystroke.BackDownForward:
                    {
                        AddCombination(EKeystroke.BackDownForward, [EKeystroke.Back, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR]);
                        AddChord(EKeystroke.Back, EKeystroke.DownForwardR, EKeystroke.BackDownForward);
                        AddChord(EKeystroke.BackDown, EKeystroke.ForwardR, EKeystroke.BackDownForward);
                        AddChord(EKeystroke.BackForward, EKeystroke.DownR, EKeystroke.BackDownForward);
                    }
                    break;
                case EKeystroke.BackDown:
                    {
                        AddCombination(EKeystroke.BackDown, [EKeystroke.Back, EKeystroke.DownR]);
                        AddChord(EKeystroke.Back, EKeystroke.DownR, EKeystroke.BackDown);
                    }
                    break;
                case EKeystroke.BackDownBack:
                    {
                        AddCombination(EKeystroke.BackDownBack, [EKeystroke.Back, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR]);
                        AddChord(EKeystroke.Back, EKeystroke.DownBackR, EKeystroke.BackDownBack);
                        AddChord(EKeystroke.BackDown, EKeystroke.BackR, EKeystroke.BackDownBack);
                        AddChord(EKeystroke.BackBack, EKeystroke.DownR, EKeystroke.BackDownBack);
                    }
                    break;
                case EKeystroke.BackBack:
                    {
                        AddCombination(EKeystroke.BackBack, [EKeystroke.Back, EKeystroke.BackR]);
                        AddChord(EKeystroke.Back, EKeystroke.BackR, EKeystroke.BackBack);
                    }
                    break;
                case EKeystroke.BackUpBack:
                    {
                        AddCombination(EKeystroke.BackUpBack, [EKeystroke.Back, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR]);
                        AddChord(EKeystroke.Back, EKeystroke.UpBackR, EKeystroke.BackUpBack);
                        AddChord(EKeystroke.BackUp, EKeystroke.BackR, EKeystroke.BackUpBack);
                        AddChord(EKeystroke.BackBack, EKeystroke.UpR, EKeystroke.BackUpBack);
                    }
                    break;
                case EKeystroke.BackNeutral:
                    {
                        AddCombination(EKeystroke.BackNeutral, [EKeystroke.Back, EKeystroke.NeutralR]);
                        AddChord(EKeystroke.Back, EKeystroke.NeutralR, EKeystroke.BackNeutral);
                    }
                    break;

                case EKeystroke.NeutralUp:
                    {
                        AddCombination(EKeystroke.NeutralUp, [EKeystroke.Neutral, EKeystroke.UpR]);
                        AddChord(EKeystroke.Neutral, EKeystroke.UpR, EKeystroke.NeutralUp);
                    }
                    break;
                case EKeystroke.NeutralUpForward:
                    {
                        AddCombination(EKeystroke.NeutralUpForward, [EKeystroke.Neutral, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR]);
                        AddChord(EKeystroke.Neutral, EKeystroke.UpForwardR, EKeystroke.NeutralUpForward);
                        AddChord(EKeystroke.NeutralUp, EKeystroke.ForwardR, EKeystroke.NeutralUpForward);
                        AddChord(EKeystroke.NeutralForward, EKeystroke.UpR, EKeystroke.NeutralUpForward);
                    }
                    break;
                case EKeystroke.NeutralForward:
                    {
                        AddCombination(EKeystroke.NeutralForward, [EKeystroke.Neutral, EKeystroke.ForwardR]);
                        AddChord(EKeystroke.Neutral, EKeystroke.ForwardR, EKeystroke.NeutralForward);
                    }
                    break;
                case EKeystroke.NeutralDownForward:
                    {
                        AddCombination(EKeystroke.NeutralDownForward, [EKeystroke.Neutral, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR]);
                        AddChord(EKeystroke.Neutral, EKeystroke.DownForwardR, EKeystroke.NeutralDownForward);
                        AddChord(EKeystroke.NeutralDown, EKeystroke.ForwardR, EKeystroke.NeutralDownForward);
                        AddChord(EKeystroke.NeutralForward, EKeystroke.DownR, EKeystroke.NeutralDownForward);
                    }
                    break;
                case EKeystroke.NeutralDown:
                    {
                        AddCombination(EKeystroke.NeutralDown, [EKeystroke.Neutral, EKeystroke.DownR]);
                        AddChord(EKeystroke.Neutral, EKeystroke.DownR, EKeystroke.NeutralDown);
                    }
                    break;
                case EKeystroke.NeutralDownBack:
                    {
                        AddCombination(EKeystroke.NeutralDownBack, [EKeystroke.Neutral, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR]);
                        AddChord(EKeystroke.Neutral, EKeystroke.DownBackR, EKeystroke.NeutralDownBack);
                        AddChord(EKeystroke.NeutralDown, EKeystroke.BackR, EKeystroke.NeutralDownBack);
                        AddChord(EKeystroke.NeutralBack, EKeystroke.DownR, EKeystroke.NeutralDownBack);
                    }
                    break;
                case EKeystroke.NeutralBack:
                    {
                        AddCombination(EKeystroke.NeutralBack, [EKeystroke.Neutral, EKeystroke.BackR]);
                        AddChord(EKeystroke.Neutral, EKeystroke.BackR, EKeystroke.NeutralBack);
                    }
                    break;
                case EKeystroke.NeutralUpBack:
                    {
                        AddCombination(EKeystroke.NeutralUpBack, [EKeystroke.Neutral, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR]);
                        AddChord(EKeystroke.Neutral, EKeystroke.UpBackR, EKeystroke.NeutralUpBack);
                        AddChord(EKeystroke.NeutralUp, EKeystroke.BackR, EKeystroke.NeutralUpBack);
                        AddChord(EKeystroke.NeutralBack, EKeystroke.UpR, EKeystroke.NeutralUpBack);
                    }
                    break;
                case EKeystroke.NeutralNeutral:
                    {
                        AddCombination(EKeystroke.NeutralNeutral, [EKeystroke.Neutral, EKeystroke.NeutralR]);
                        AddChord(EKeystroke.Neutral, EKeystroke.NeutralR, EKeystroke.NeutralNeutral);
                    }
                    break;
            }
        }

        private void AddCombination(EKeystroke combo, params EKeystroke[] components)
        {
            Combinations[combo] = components;
        }

        private void AddChord(EKeystroke first, EKeystroke second, EKeystroke result)
        {
            Add(first);
            Add(second);

            //Add the chord as passed in
            if (!Chords.ContainsKey(first))
            {
                Chords[first] = new Dictionary<EKeystroke, EKeystroke>();
            }
            Chords[first][second] = result;

            //Add the reverse chord
            if (!Chords.ContainsKey(second))
            {
                Chords[second] = new Dictionary<EKeystroke, EKeystroke>();
            }
            Chords[second][first] = result;
        }

        /// <summary>
        /// Check if the next keystroke is contained in the buffered input
        /// </summary>
        /// <param name="buffered">A current keystroke in the input queue</param>
        /// <param name="next">a newly discovered input</param>
        /// <returns>Bool: true, the next keystroke can be discarded</returns>
        public bool IsRedundant(EKeystroke buffered, EKeystroke next)
        {
            if (Combinations.ContainsKey(buffered))
            {
                foreach (var key in Combinations[buffered])
                {
                    if (key == next)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryCombine(EKeystroke buffered, EKeystroke next, out EKeystroke combined)
        {
            if (Chords.ContainsKey(buffered))
            {
                if (Chords[buffered].ContainsKey(next))
                {
                    combined = Chords[buffered][next];
                    return true;
                }
            }

            //check the reverse as well
            if (Chords.ContainsKey(next))
            {
                if (Chords[next].ContainsKey(buffered))
                {
                    combined = Chords[next][buffered];
                    return true;
                }
            }

            combined = buffered;
            return false;
        }
    }
}