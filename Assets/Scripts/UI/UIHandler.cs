using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.UI
{
    using Controller;

    public class UIHandler : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        private Transform mainProc;

        [SerializeField]
        private Transform[] procs;

        [SerializeField]
        private GameObject buttonRun;

        [SerializeField]
        private GameObject buttonReset;

        [SerializeField]
        private Color colorProcActive;

        [SerializeField]
        private Color colorProcDeactive;

        [SerializeField]
        private GameObject panelFinish;
        
        [SerializeField]
        public AudioSource winSound; // Added for win sound

        public static UIHandler Instance;
        public Transform activeProc;
        #endregion

        #region Methods
        private void Awake()
        {
            if (Instance != null)
                Destroy(this.gameObject);
            Instance = this;
            activeProc = mainProc;
        }

        internal void AddOperation(UIOperation uiOperation)
        {
            UIOperation instance = Instantiate(uiOperation, Vector3.zero, Quaternion.identity, activeProc);
            instance.IsAdded = true;

            if (activeProc == mainProc)
                GameManager.Instance.AddOperation(instance.Operation);
            else
                GameManager.Instance.AddOperationInSubProcedure(instance.Operation, GetIndex(activeProc));
        }

        internal void RemoveOperation(UIOperation uiOperation)
        {
            Destroy(uiOperation.gameObject);

            if (activeProc == mainProc)
                GameManager.Instance.RemoveOperation(uiOperation.Operation);
            else
                GameManager.Instance.RemoveOperationFromSubProcedure(uiOperation.Operation, GetIndex(activeProc));
        }

        public void Run()
        {
            GameManager.Instance.RunCode();
            ShowHideRunButton(false);
        }

        public void Reset()
        {
            GameManager.Instance.ResetCode();
            ShowHideRunButton(true);
        }

        // Changed from private to public so BotController can access it
        public void ShowHideRunButton(bool isShow)
        {
            buttonRun.SetActive(isShow);
            buttonReset.SetActive(!isShow);
        }

        public void OnProc(Transform caller)
        {
            DeactiveAllProcs();
            caller.parent.parent.parent.GetComponent<Image>().color = colorProcActive;
            activeProc = caller;
        }

        private void DeactiveAllProcs()
        {
            mainProc.parent.parent.parent.GetComponent<Image>().color = colorProcDeactive;

            foreach (Transform proc in procs)
            {
                proc.parent.parent.parent.GetComponent<Image>().color = colorProcDeactive;
            }
        }

        private int GetIndex(Transform proc)
        {
            for (int i = 0; i < procs.Length; i++)
            {
                if (proc == procs[i])
                    return i;
            }

            return -1;
        }

        public void ShowFinish()
        {
            panelFinish.SetActive(true);
            
            if (winSound != null)
            {
                winSound.Play(); // Play the win sound when the panel is displayed
            }
        }

        public void NextLevel()
        {
            Debug.Log("NextLevel button clicked");
            
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            int nextScene = currentScene + 1;
            
            Debug.Log("Current scene: " + currentScene + ", Next scene: " + nextScene);
            
            if (nextScene < SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log("Loading next scene directly: " + nextScene);
                GameManager.LoadLevel(nextScene);
            }
            else
            {
                Debug.Log("No more levels available, returning to main menu");
                GameManager.LoadLevel(0);
            }
        }

        public void BackToMain()
        {
            GameManager.LoadLevel(0);
        }
        #endregion
    }
}