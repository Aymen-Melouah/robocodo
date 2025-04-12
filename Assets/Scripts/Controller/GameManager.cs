using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Game.Controller
{
    using Model;
    using Operation;
    using UI;
    
    public class GameManager : MonoBehaviour
    {
        #region Variables
            [SerializeField]
            private int subProcCount = 0;
            
            private Procedure mainProcedure = new Procedure();
            private Procedure[] subProcedures;
            public static GameManager Instance;
            private static int level = 0;
                        
            private BotController botController;
        #endregion
        
        #region SetterGetters
            internal Procedure[] SubProcedures
            {
                get { return subProcedures; }
            }
        #endregion
        
        #region Methods
            private void Awake()
            {
                if (Instance != null)
                    Destroy(this.gameObject);
                Instance = this;
                
                Initialize();
            }
            
            private void Start()
            {
                // Set level to current scene index when scene loads
                level = SceneManager.GetActiveScene().buildIndex;
                Debug.Log("Current level set to: " + level);
            }
            
            private void Initialize()
            {
                subProcedures = new Procedure[subProcCount];
                for (int i = 0; i < subProcedures.Length; i++)
                {
                    subProcedures[i] = new Procedure();
                }
                                
                botController = GameObject.FindObjectOfType<BotController>();
            }
            
            public void Finish()
            {
                UIHandler.Instance.ShowFinish();
                StopAllCoroutines();
            }
            
            public void LoadNextLevel()
            {
                Debug.Log("Current level: " + level);
                Debug.Log("Scene count: " + SceneManager.sceneCountInBuildSettings);
                
                if (level < SceneManager.sceneCountInBuildSettings - 1)
                {
                    Debug.Log("Loading next level: " + (level + 1));
                    LoadLevel(level + 1);
                }
                else
                {
                    Debug.Log("No more levels to load, returning to main menu");
                    LoadLevel(0);
                }
            }
            
            public static void LoadLevel(int index)
            {
                Debug.Log("LoadLevel called with index: " + index);
                if (index >= 0 && index < SceneManager.sceneCountInBuildSettings)
                {
                    level = index;
                    SceneManager.LoadScene(index);
                    Debug.Log("Scene loaded: " + index);
                }
                else
                {
                    Debug.LogError("Invalid scene index: " + index);
                }
            }
            
            public void AddOperation(BotOperation operation)
            {
                mainProcedure.Add(operation);
            }
            
            public void RemoveOperation(BotOperation operation)
            {
                mainProcedure.Remove(operation);
            }
            
            public void AddOperationInSubProcedure(BotOperation operation, int subProcIndex)
            {
                subProcedures[subProcIndex].Add(operation);
            }
            
            public void RemoveOperationFromSubProcedure(BotOperation operation, int subProcIndex)
            {
                subProcedures[subProcIndex].Remove(operation);
            }
            
            public void ResetCode()
            {
                StartCoroutine(DoResetCode());
            }
            
            public void RunCode()
            {
                StartCoroutine(DoRunCode());
            }
            
            private System.Collections.IEnumerator DoResetCode()
            {
                ResetOperation resetOperation = new ResetOperation();
                resetOperation.Initialize();
                yield return resetOperation.Run();
                StopAllCoroutines();
            }
            
            private System.Collections.IEnumerator DoRunCode()
            {
                // Use botController.ExecuteOperations instead of running operations one by one
                yield return StartCoroutine(botController.ExecuteOperations(mainProcedure.Operations));
                yield return new WaitForFixedUpdate();
            }
        #endregion
    }
}