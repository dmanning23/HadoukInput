using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Text;

namespace HadoukInput
{
    /// <summary>
    /// A delegate to get the current time.
    /// </summary>
    /// <returns>float: the current time in seconds.</returns>
    public delegate float CurrentTime();

    /// <summary>
    /// This is the guts of the input lib... 
    /// This class queues up input from the player, condenses input into the correct keystrokes, and parses queued input to look for patterns.
    /// </summary>
    public class InputWrapper
    {
        #region Properties

        public const float DefaultBufferedInputExpire = 0.05f;
        public const float DefaultQueuedInputExpire = 0.5f;

        /// <summary>
        /// Callback to a method to get the current time, used to time the input
        /// </summary>
        private readonly CurrentTime GetCurrentTime;

        /// <summary>
        /// The move tree, which acutally holds the wholle move list
        /// </summary>
        private MoveList Moves { get; set; }

        /// <summary>
        /// This is the buffer for input before it is put in the listInput
        /// this allows for simultaneous button presses
        /// Input is held in here for a split second, condensed into keystrokes as they come in, then put in the queue.
        /// </summary>
        private readonly List<InputItem> BufferedInput;

        /// <summary>
        /// list of queued input
        /// This is used to look for patterns as loaded from the move list.
        /// Input is held in here for a little bit while it parses for moves.
        /// </summary>
        private readonly List<InputItem> QueuedInput;

        public IControllerWrapper Controller { get; private set; }

        /// <summary>
        /// Length of time input items are held in the buffer before being put in the queue
        /// </summary>
        public float BufferedInputExpire { get; set; }

        /// <summary>
        /// Length of time input items are held in the queue before they are discarded.
        /// </summary>
        public float QueuedInputExpire { get; set; }

        public KeystrokeCombiner KeystrokeCombiner { get; private set; }

        #endregion //Properties

        #region Methods

        #region Initialization 

        /// <summary>
        /// Setup the state machine for combining input
        /// </summary>
        static InputWrapper()
        {
        }

        /// <summary>
        /// contructor
        /// </summary>
        /// <param name="controller">index of the controller this player will use</param>
        /// <param name="clock">The external clock that will be used to time this dude.  This guy doesn't update his own timer!</param>
        public InputWrapper(IControllerWrapper controller, CurrentTime clock)
        {
            BufferedInputExpire = DefaultBufferedInputExpire;
            QueuedInputExpire = DefaultQueuedInputExpire;
            BufferedInput = new List<InputItem>();
            QueuedInput = new List<InputItem>();
            Controller = controller;
            GetCurrentTime = clock;
            Moves = new MoveList();
            KeystrokeCombiner = new KeystrokeCombiner();
        }

        #endregion //Initialization 

        /// <summary>
        /// clear out all the stored input
        /// </summary>
        public void Clear()
        {
            BufferedInput.Clear();
            QueuedInput.Clear();
        }

        /// <summary>
        /// Parse the move lists and queues.  This method is called every frame, unless the game is paused.
        /// This update function will read input from the controller and then parse the move lists and queues.
        /// Use this update if the character is your game only faces left or right.
        /// </summary>
        /// <param name="inputState">the current input state of the game.
        /// If an input state is passed in, the controller wrapper will be updated with the data in there.
        /// If the controller wrapper has gotten it's input from somewhere else (ie the network), pass in null</param>
        /// <param name="flipped">whether or not the character that this input wrapper controls is facing right(false) or left(true).</param>
        public void Update(IInputState inputState, bool flipped)
        {
            UpdateController(inputState);

            //create a fake direction for left or right
            UpdateMoveQueue(flipped, (flipped ? -Vector2.UnitX : Vector2.UnitX));
        }

        /// <summary>
        /// Parse the move lists and queues.  This method is called every frame, unless the game is paused.
        /// This update function will read input from the controller and then parse the move lists and queues.
        /// </summary>
        /// <param name="inputState">the current input state of the game.
        /// If an input state is passed in, the controller wrapper will be updated with the data in there.
        /// If the controller wrapper has gotten it's input from somewhere else (ie the network), pass in null</param>
        /// <param name="flipped">whether or not the character that this input wrapper controls is facing right(false) or left(true).</param>
        /// <param name="direction">the direction the character is facing</param>
        public void Update(IInputState inputState, bool flipped, Vector2 direction)
        {
            UpdateController(inputState);
            UpdateMoveQueue(flipped, direction);
        }

        /// <summary>
        /// update the controller
        /// </summary>
        /// <param name="inputState"></param>
        private void UpdateController(IInputState inputState)
        {
            //first update the controller if an input state was passed in.
            if ((null != inputState) && (null != Controller))
            {
                Controller.Update(inputState);
            }
        }

        /// <summary>
        /// update all the queues that hold the move data
        /// </summary>
        /// <param name="flipped">whether or not the character that this input wrapper controls is facing right(false) or left(true).</param>
        /// <param name="direction">the direction the character is facing</param>
        protected void UpdateMoveQueue(bool flipped, Vector2 direction)
        {
            //first, remove any old input from the system
            var currentTime = GetCurrentTime();
            var minInputItemTime = currentTime - QueuedInputExpire;
            while (QueuedInput.Count > 0)
            {
                if (QueuedInput[0].Time <= minInputItemTime)
                {
                    QueuedInput.RemoveAt(0);
                }
                else
                {
                    //got out all the old input
                    break;
                }
            }

            //loop through and check directions and single actions
            if (null != Controller)
            {
                //start at the top so they get combined in the correct order
                for (var i = EKeystroke.RTriggerRelease; i >= 0; i--)
                {
                    //get the result of checking that input button
                    if (Controller.CheckKeystroke(i, flipped, direction))
                    {
                        BufferKeyStroke(i, currentTime);
                    }
                }
            }

            //check if any buffered input keys are expired
            minInputItemTime = currentTime - BufferedInputExpire;
            while (BufferedInput.Count > 0)
            {
                if (BufferedInput[0].Time <= minInputItemTime)
                {
                    //Check if this message is already queued
                    var found = false;
                    for (var i = 0; i < QueuedInput.Count; i++)
                    {
                        if (QueuedInput[i] == BufferedInput[0])
                        {
                            found = true;
                            break;
                        }
                    }

                    //if message not already queued, add the input message to the input list
                    if (!found)
                    {
                        QueuedInput.Add(BufferedInput[0]);
                    }

                    //remove the message from the buffered input
                    BufferedInput.RemoveAt(0);
                }
                else
                {
                    //got out all the old buffered input
                    break;
                }
            }
        }

        private void BufferKeyStroke(EKeystroke foundKey, float currentTime)
        {
            //ok, found a keystroke... check if we even need it
            var needIt = true;
            for (var i = 0; i < BufferedInput.Count; i++)
            {
                //is it a dupe keystroke?
                if (BufferedInput[i].Keystroke == foundKey)
                {
                    //yeah we got plenty of that shit already
                    needIt = false;
                    break;
                }

                //Is this new keystroke part of a combination that is already buffered?
                if (KeystrokeCombiner.IsRedundant(BufferedInput[i].Keystroke, foundKey))
                {
                    needIt = false;
                    break;
                }

                //Can we combine these keystrokes?
                var combined = BufferedInput[i].Keystroke;
                if (KeystrokeCombiner.TryCombine(BufferedInput[i].Keystroke, foundKey, out combined))
                {
                    //Ok, these two keystrokes can be combined... 

                    //remove the old keystroke
                    BufferedInput.RemoveAt(i);

                    //buffer the new keystroke
                    BufferKeyStroke(combined, currentTime);
                    needIt = false;
                    break;
                }
            }

            if (needIt)
            {
                //add to the buffered input for checking later
                BufferedInput.Add(new InputItem(currentTime, foundKey));
            }
        }

        /// <summary>
        /// Get the next Move out of the queue
        /// This clears the move out of the queue, so you can only get it once!
        /// </summary>
        /// <returns>int: the id of the Move (as message index in statemachine). -1 for no Move</returns>
        public string GetNextMove()
        {
            return Moves.GetNextMove(QueuedInput);
        }

        public override string ToString()
        {
            var inputTextmyText = new StringBuilder();
            for (var i = 0; i < QueuedInput.Count; i++)
            {
                inputTextmyText.AppendFormat("{0}, ", QueuedInput[i].Keystroke.ToString());
            }

            return inputTextmyText.ToString();
        }

        /// <summary>
        /// A method used for test harness to output to the screen
        /// </summary>
        /// <returns></returns>
        public string GetBufferedInput()
        {
            var inputText = new StringBuilder();
            for (var i = 0; i < BufferedInput.Count; i++)
            {
                inputText.AppendFormat("{0}, ", BufferedInput[i].Keystroke.ToString());
            }

            return inputText.ToString();
        }

        /// <summary>
        /// get the number of connected gamepads
        /// </summary>
        /// <returns>The num of gamepads.</returns>
        public static int NumGamepads()
        {
            var total = 0;
            for (var i = 0; i < GamePad.MaximumGamePadCount; i++)
            {
                if (GamePad.GetState(i).IsConnected)
                {
                    total++;
                }
            }

            return total;
        }

        #endregion //Methods

        #region File IO

        public void AddMove(string moveName, params EKeystroke[] keystrokes)
        {
            foreach (var keystroke in keystrokes)
            {
                KeystrokeCombiner.Add(keystroke);
            }

            Moves.AddMove(moveName, keystrokes);
        }

        public void AddMoves(MoveListModel moveListModel)
        {
            foreach (var move in moveListModel.Moves)
            {
                AddMove(move.Name, move.Keystrokes.ToArray());
            }
        }

        /// <summary>
        /// read input from a xna resource
        /// </summary>
        /// <param name="xmlFilename">name of the resource to load</param>
        /// <param name="messageIds">delegate method for resolving message names</param>
        /// <returns>bool: whether or not it was able to load the input list</returns>
        public void ReadXmlFile(Filename xmlFilename, ContentManager xmlContent = null)
        {
            using (var moveListModel = new MoveListModel(xmlFilename))
            {
                moveListModel.ReadXmlFile(xmlContent);
                AddMoves(moveListModel);
            }
        }

        #endregion //File IO
    }
}