﻿using System.Collections;

namespace Game.Controller.Operation
{
    using Model;

    public class WalkOperation : BotOperation
    {
        #region Variables
        private Position nextPosition;
        #endregion

        #region Methods
        public override bool IsValid()
        {
            nextPosition = botController.currentPosition;

            switch (botController.currentDirection)
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
                default:
                    break;
            }

            return BoardManager.Instance.PlatformIsExists(nextPosition);
        }

        public override IEnumerator Run()
        {
            if (IsValid())
            {
                Platform currentPlatform = BoardManager.Instance.GetPlatform(botController.currentPosition);
                Platform nextPlatform = BoardManager.Instance.GetPlatform(nextPosition);

                if (currentPlatform.Height == nextPlatform.Height)
                {
                    // Always pass false for isLastMove because ExecuteOperations manages animation now
                    yield return botController.Walk(nextPosition, nextPlatform.lastBlock.transform.position, false);
                }
            }
        }
        #endregion
    }
}