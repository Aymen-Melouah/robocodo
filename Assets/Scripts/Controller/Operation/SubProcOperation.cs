using UnityEngine;
using System.Collections;

namespace Game.Controller.Operation
{
    public class SubProcOperation : BotOperation
    {
        #region Variables
            [SerializeField]
            private int index;
        #endregion

        #region Methods
            public override bool IsValid()
            {
                return true;
            }

            public override IEnumerator Run()
            {
                // Get the botController reference
                BotController botController = GameObject.FindObjectOfType<BotController>();
                
                // Use the ExecuteOperations method instead of running operations individually
                if (GameManager.Instance.SubProcedures[index].Operations.Count > 0)
                {
                    yield return botController.ExecuteOperations(
                        GameManager.Instance.SubProcedures[index].Operations);
                }
            }
        #endregion
    }
}