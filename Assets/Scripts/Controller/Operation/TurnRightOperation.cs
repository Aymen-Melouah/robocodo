using System.Collections;
using UnityEngine;

namespace Game.Controller.Operation
{
    using Model;
    
    public class TurnRightOperation : BotOperation
    {
        #region Variables
        private bool isValidTurn = true;
        private bool shouldPlayWarningSound = false;
        #endregion

        #region Methods
        public override bool IsValid()
        {
            // The turn itself is always valid
            isValidTurn = true;
            
            // Check if there will be a block in front after turning
            BotController.Direction newDirection = (BotController.Direction)(((int)botController.currentDirection + 1) % 4);
            Position currentPosition = botController.currentPosition;
            Position nextPosition = currentPosition;
            
            // Calculate the position in front after the turn
            switch (newDirection)
            {
                case BotController.Direction.FORWARD:
                    nextPosition += new Position(0, 1);
                    break;
                case BotController.Direction.BACKWARD:
                    nextPosition += new Position(0, -1);
                    break;
                case BotController.Direction.LEFT:
                    nextPosition += new Position(-1, 0);
                    break;
                case BotController.Direction.RIGHT:
                    nextPosition += new Position(1, 0);
                    break;
            }
            
            // Check if there is a platform at the next position
            bool platformExists = BoardManager.Instance.PlatformIsExists(nextPosition);
            
            // If no platform exists, set flag to play warning sound during Run
            shouldPlayWarningSound = !platformExists;
            
            // Debug logging to help troubleshoot
            Debug.Log("TurnRight - Platform exists in new direction: " + platformExists);
            if (!platformExists) {
                Debug.Log("TurnRight - Will play warning sound");
            }
            
            return isValidTurn;
        }

        public override IEnumerator Run()
        {
            // Check if we need to play the warning sound
            if (shouldPlayWarningSound)
            {
                Debug.Log("TurnRight - Playing warning sound now");
                botController.PlayWarningSound(BotController.WarningType.Turn);
            }
            
            yield return botController.TurnRight();
        }
        #endregion
    }
}