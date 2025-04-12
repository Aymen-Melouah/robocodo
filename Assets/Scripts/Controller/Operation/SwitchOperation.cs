
using System.Collections;

namespace Game.Controller.Operation
{
    using Model;

    public class SwitchOperation : BotOperation
    {
        #region Variables
            TargetPlatform targetPlatform;
        #endregion

        #region Methods
            public override bool IsValid ()
            {
                targetPlatform = BoardManager.Instance.GetTargetPlatform(botController.currentPosition);
                return targetPlatform != null;
            }

            public override IEnumerator Run ()
            {
                if (!IsValid())
                {
                    // Play warning sound when no target platform exists
                    botController.PlayWarningSound(BotController.WarningType.Switch);
                    yield break;
                }

                yield return botController.Switch(targetPlatform);
            }
        #endregion
    }
}