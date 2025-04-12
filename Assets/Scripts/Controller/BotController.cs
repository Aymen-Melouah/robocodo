using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Controller
{
    using Model;
    using Operation;
    using UI;

    public class BotController : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        internal Position currentPosition;

        [SerializeField]
        internal Direction currentDirection = Direction.FORWARD;

        internal enum Direction { FORWARD, RIGHT, BACKWARD, LEFT };

        private float positionThreshold = 0.05f;
        private float rotationThreshold = 10f;
        
        // Reduced deltaTime for slower movement to match animation
        private float deltaTime = 0.075f; // Changed from 0.125f to 0.075f for slower movement
        
        private Position initialPlatformPosition;
        private Direction initialDirection;
        private Vector3 initialWorldPosition;
        private Vector3 initialWorldRotation;

        private Animator animator;
        
        // Track if we're currently executing operations
        private bool isExecutingOperations = false;
        
        // Track if game is stopped due to warning
        private bool isGameStopped = false;

        // Add warning sound variables
        [SerializeField] 
        private AudioClip jumpWarningSound;
        
        [SerializeField] 
        private AudioClip switchWarningSound;

        // Add turn warning sound
        [SerializeField] 
        private AudioClip turnWarningSound;

        private AudioSource audioSource;
        #endregion

        #region Methods
        private void Awake()
        {
            initialPlatformPosition = currentPosition;
            initialDirection = currentDirection;
            initialWorldPosition = this.transform.position;
            initialWorldRotation = this.transform.eulerAngles;

            animator = GetComponent<Animator>();

            // Ensure AudioSource component exists
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // New method to play warning sounds and stop game
        public void PlayWarningSound(WarningType type)
        {
            AudioClip clipToPlay = null;
            
            switch (type)
            {
                case WarningType.Jump:
                    clipToPlay = jumpWarningSound;
                    break;
                case WarningType.Switch:
                    clipToPlay = switchWarningSound;
                    break;
                case WarningType.Turn:
                    clipToPlay = turnWarningSound;
                    break;
            }

            if (clipToPlay != null && audioSource != null)
            {
                Debug.Log("Playing sound: " + type.ToString());
                audioSource.PlayOneShot(clipToPlay);
                
                // Stop the game and trigger UI reset
                StopGame();
            }
            else
            {
                Debug.LogWarning("Cannot play sound: " + (clipToPlay == null ? "No clip" : "No audio source"));
            }
        }
        
        // New method to stop the game and use UIHandler to show the reset button
        private void StopGame()
        {
            isGameStopped = true;
            
            // Stop all coroutines to freeze current execution
            StopAllCoroutines();
            
            // Show reset button using the existing UIHandler
            if (UIHandler.Instance != null)
            {
                // This will show the reset button and hide the run button
                UIHandler.Instance.ShowHideRunButton(false);
                
                Debug.Log("Game stopped due to warning - Reset button shown");
            }
            else
            {
                Debug.LogWarning("UIHandler instance not found!");
            }
        }

        // Helper method to check if there's a platform in front
        private bool IsPlatformInFront(Direction directionToCheck)
        {
            Position positionToCheck = currentPosition;
            
            switch (directionToCheck)
            {
                case Direction.FORWARD:
                    positionToCheck += new Position(0, 1);
                    break;
                case Direction.BACKWARD:
                    positionToCheck += new Position(0, -1);
                    break;
                case Direction.LEFT:
                    positionToCheck += new Position(-1, 0);
                    break;
                case Direction.RIGHT:
                    positionToCheck += new Position(1, 0);
                    break;
            }
            
            bool exists = BoardManager.Instance.PlatformIsExists(positionToCheck);
            Debug.Log("Checking direction " + directionToCheck + " at position " + positionToCheck + ": " + (exists ? "Platform exists" : "No platform"));
            return exists;
        }

        // Enum for warning types
        public enum WarningType
        {
            Jump,
            Switch,
            Turn
        }

        internal void Reset()
        {
            this.transform.position = initialWorldPosition;
            this.transform.eulerAngles = initialWorldRotation;
            currentPosition = initialPlatformPosition;
            currentDirection = initialDirection;

            animator.SetBool("isWalking", false);
            animator.SetBool("isJumping", false);
            isExecutingOperations = false;
            isGameStopped = false;
        }

        public IEnumerator ExecuteOperations(List<BotOperation> operations)
        {
            if (operations == null || operations.Count == 0) yield break;
            
            isExecutingOperations = true;
            isGameStopped = false;
            
            // Analyze operations to find sequences of similar operations
            List<List<BotOperation>> operationGroups = new List<List<BotOperation>>();
            List<BotOperation> currentGroup = new List<BotOperation>();
            
            for (int i = 0; i < operations.Count; i++)
            {
                BotOperation op = operations[i];
                
                if (currentGroup.Count == 0)
                {
                    currentGroup.Add(op);
                    continue;
                }
                
                if ((op is WalkOperation && currentGroup[currentGroup.Count - 1] is WalkOperation) ||
                    (op is JumpOperation && currentGroup[currentGroup.Count - 1] is JumpOperation) ||
                    (op is TurnLeftOperation && currentGroup[currentGroup.Count - 1] is TurnLeftOperation) ||
                    (op is TurnRightOperation && currentGroup[currentGroup.Count - 1] is TurnRightOperation))
                {
                    currentGroup.Add(op);
                }
                else
                {
                    operationGroups.Add(new List<BotOperation>(currentGroup));
                    currentGroup.Clear();
                    currentGroup.Add(op);
                }
            }
            
            if (currentGroup.Count > 0)
            {
                operationGroups.Add(currentGroup);
            }
            
            // Execute each group
            foreach (var group in operationGroups)
            {
                // Check if game is stopped before continuing
                if (isGameStopped)
                {
                    isExecutingOperations = false;
                    yield break;
                }
                
                BotOperation firstOp = group[0];
                
                if (firstOp is WalkOperation)
                {
                    animator.SetBool("isWalking", true);
                    animator.SetBool("isJumping", false);
                    
                    // Add a small delay to ensure walking animation starts before movement
                    yield return new WaitForSeconds(0.1f);
                }
                else if (firstOp is JumpOperation)
                {
                    animator.SetBool("isJumping", true);
                    animator.SetBool("isWalking", false);
                }
                
                foreach (var op in group)
                {
                    // Check if game is stopped before each operation
                    if (isGameStopped)
                    {
                        if (firstOp is WalkOperation)
                        {
                            animator.SetBool("isWalking", false);
                        }
                        else if (firstOp is JumpOperation)
                        {
                            animator.SetBool("isJumping", false);
                        }
                        
                        isExecutingOperations = false;
                        yield break;
                    }
                    
                    yield return op.Run();
                    
                    // Add a cooldown between operations
                    yield return new WaitForSeconds(0.3f);
                }
                
                if (firstOp is WalkOperation)
                {
                    animator.SetBool("isWalking", false);
                }
                else if (firstOp is JumpOperation)
                {
                    animator.SetBool("isJumping", false);
                }
            }
            
            isExecutingOperations = false;
        }

        public IEnumerator Walk(Position nextPosition, Vector3 platformWorldPosition, bool isLastMove)
        {
            // Minimum time to complete walking to match animation
            float minWalkTime = 0.5f;
            float startTime = Time.time;
            Vector3 startPosition = this.transform.position;
            float journeyLength = Vector3.Distance(startPosition, platformWorldPosition);
            float distanceCovered = 0f;
            
            while ((distanceCovered / journeyLength < 1.0f || 
                   Time.time - startTime < minWalkTime) && 
                   !isGameStopped)
            {
                // Calculate journey fraction but ensure we don't exceed minimum time
                float timeElapsed = Time.time - startTime;
                distanceCovered = Vector3.Distance(startPosition, this.transform.position);
                
                // Use smoothed lerp for more natural movement that matches animation
                float t = Mathf.SmoothStep(0, 1, timeElapsed / minWalkTime);
                this.transform.position = Vector3.Lerp(startPosition, platformWorldPosition, t);
                
                yield return new WaitForFixedUpdate();
            }
            
            if (!isGameStopped)
            {
                this.transform.position = platformWorldPosition;
                currentPosition = nextPosition;
            }
            yield return null;
        }

        public IEnumerator TurnRight()
        {
            // Check if there will be a platform in front after turning
            Direction newDirection = (Direction)(((int)currentDirection + 1) % 4);
            bool platformExists = IsPlatformInFront(newDirection);
            
            // If no platform exists after turn, play warning sound
            if (!platformExists)
            {
                Debug.Log("TurnRight: No platform in direction " + newDirection + " - Playing warning sound");
                PlayWarningSound(WarningType.Turn);
                // Note: Game will be stopped by PlayWarningSound
                yield break;
            }
            
            Vector3 targetAngles = this.transform.eulerAngles + new Vector3(0, 90, 0);
            Vector3 currentAngles = this.transform.eulerAngles;
            
            while ((currentAngles - targetAngles).magnitude > rotationThreshold && !isGameStopped)
            {
                currentAngles = Vector3.Lerp(currentAngles, targetAngles, deltaTime);
                this.transform.rotation = Quaternion.Euler(currentAngles);
                yield return new WaitForFixedUpdate();
            }
            
            if (!isGameStopped)
            {
                this.transform.eulerAngles = targetAngles;
                currentDirection = (Direction)((int)(currentDirection + 1) % 4);
            }
            yield return new WaitForFixedUpdate();
        }

        public IEnumerator TurnLeft()
        {
            // Check if there will be a platform in front after turning
            Direction newDirection = (Direction)(((int)currentDirection + 3) % 4);
            bool platformExists = IsPlatformInFront(newDirection);
            
            // If no platform exists after turn, play warning sound
            if (!platformExists)
            {
                Debug.Log("TurnLeft: No platform in direction " + newDirection + " - Playing warning sound");
                PlayWarningSound(WarningType.Turn);
                // Note: Game will be stopped by PlayWarningSound
                yield break;
            }
            
            Vector3 targetAngles = this.transform.eulerAngles - new Vector3(0, 90, 0);
            Vector3 currentAngles = this.transform.eulerAngles;
            
            if (currentAngles.y > 180)
                currentAngles = this.transform.eulerAngles - Vector3.up * 360;
            
            while ((currentAngles - targetAngles).magnitude > rotationThreshold && !isGameStopped)
            {
                currentAngles = Vector3.Lerp(currentAngles, targetAngles, deltaTime);
                this.transform.rotation = Quaternion.Euler(currentAngles);
                yield return new WaitForFixedUpdate();
            }
            
            if (!isGameStopped)
            {
                this.transform.eulerAngles = targetAngles;
                currentDirection = (Direction)((int)(currentDirection + 3) % 4);
            }
            yield return new WaitForFixedUpdate();
        }

        public IEnumerator Switch(TargetPlatform targetPlatform)
        {
            animator.SetTrigger("switch");
            
            // Wait for animation to start
            yield return new WaitForSeconds(0.2f);
            
            if (!isGameStopped)
            {
                targetPlatform.Switch();
            }
            
            // Shorter cooldown but still long enough for animation to be visible
            yield return new WaitForSeconds(0.7f);
            
            yield return new WaitForFixedUpdate();
        }

       public IEnumerator Jump(Position nextPosition, Vector3 lastPlatformBlockPosition)
{
    Vector3 startPos = this.transform.position;
    Vector3 endPos = lastPlatformBlockPosition;
    float jumpHeight = 1.0f;
    float journeyLength = Vector3.Distance(startPos, endPos);
    float startTime = Time.time;
    float distanceCovered = 0;
    
    while (distanceCovered < journeyLength && !isGameStopped)
    {
        float journeyFraction = distanceCovered / journeyLength;
        float height = Mathf.Sin(journeyFraction * Mathf.PI) * jumpHeight;
        
        Vector3 newPos = Vector3.Lerp(startPos, endPos, journeyFraction);
        newPos.y = startPos.y + height;
        this.transform.position = newPos;
        
        distanceCovered = (Time.time - startTime) / deltaTime * journeyLength * 0.5f;
        yield return new WaitForFixedUpdate();
    }
    
    if (!isGameStopped)
    {
        this.transform.position = lastPlatformBlockPosition;
        currentPosition = nextPosition;
    }
    
    // Add a small cooldown after jump animation completes
    yield return new WaitForSeconds(0.3f);
    
    yield return new WaitForFixedUpdate();
}
        #endregion
    }
}