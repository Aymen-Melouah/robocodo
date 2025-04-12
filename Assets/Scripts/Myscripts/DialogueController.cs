using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;  // Added for Image component

public class DialogueController : MonoBehaviour
{
    [Header("Dialogue Content")]
    public TextMeshProUGUI DialogueText;
    public string[] Sentences;
    public AudioClip[] SentenceAudios;     // Array of audio clips for each sentence
    public Sprite[] AllSprites;            // All available sprites that can be used
    public int[] SpriteIndices;            // Which sprite to show for each sentence (index into AllSprites)
    
    [Header("UI References")]
    public Image SpriteDisplay;            // UI Image component to display sprites
    public Canvas canvass;
    public Animator DialogueAnimator;
    
    [Header("Dialogue Settings")]
    public int Index = 0;
    public float DialogueSpeed = 0.05f;
    public bool StartDialogue = true;
    public float AutoStartDelay = 1f;
    public bool AutoStartDialogue = true;
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    // State tracking properties
    public bool IsDialogueRunning { get; private set; }
    public bool IsTyping { get; private set; }

    private Coroutine typingCoroutine;

    void Start()
    {
        // Initialize audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        IsDialogueRunning = false;
        IsTyping = false;
        
        // Auto-start dialogue with delay
        if (AutoStartDialogue)
        {
            StartCoroutine(AutoStartWithDelay());
        }
    }
    
    private IEnumerator AutoStartWithDelay()
    {
        // Wait for specified delay
        yield return new WaitForSeconds(AutoStartDelay);
        
        // Start dialogue automatically
        StartDialogueSequence();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(IsDialogueRunning)
            {
                ProgressDialogue();
            }
            else 
            {
                // If no sentences are available, reset the dialogue
                EndDialogue();
            }
            
        }
    }

    // Public method to start dialogue from external scripts
    public void StartDialogueSequence()
    {
        DialogueAnimator.SetTrigger("Enter");
        StartDialogue = false;
        IsDialogueRunning = true;
        NextSentence();
    }

    // Public method to progress dialogue from external scripts
    public void ProgressDialogue()
    {
        // If currently typing, complete the sentence instantly
        if (IsTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            IsTyping = false;
            DialogueText.text = Sentences[Index];
            Index++;
        }
        else
        {
            // Stop current audio if it's playing
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            NextSentence();
        }
    }

    // Public method to set all dialogue content including sprite indices
    public void SetDialogueContent(string[] newSentences, AudioClip[] newAudioClips, Sprite[] newSprites, int[] newSpriteIndices)
    {
        Sentences = newSentences;
        SentenceAudios = newAudioClips;
        AllSprites = newSprites;
        SpriteIndices = newSpriteIndices;
        Index = 0;
    }

    // Method to assign a specific sprite to a specific sentence
    public void AssignSpriteToSentence(int sentenceIndex, int spriteIndex)
    {
        if (sentenceIndex >= 0 && sentenceIndex < Sentences.Length)
        {
            // Make sure SpriteIndices array exists and is large enough
            if (SpriteIndices == null)
            {
                SpriteIndices = new int[Sentences.Length];
                // Fill with -1 to indicate no sprite
                for (int i = 0; i < SpriteIndices.Length; i++)
                {
                    SpriteIndices[i] = -1;
                }
            }
            else if (SpriteIndices.Length < Sentences.Length)
            {
                int[] newIndices = new int[Sentences.Length];
                // Copy existing values
                for (int i = 0; i < SpriteIndices.Length; i++)
                {
                    newIndices[i] = SpriteIndices[i];
                }
                // Fill remaining with -1
                for (int i = SpriteIndices.Length; i < newIndices.Length; i++)
                {
                    newIndices[i] = -1;
                }
                SpriteIndices = newIndices;
            }
            
            // Assign the sprite index
            SpriteIndices[sentenceIndex] = spriteIndex;
        }
    }

    // Public method to skip to a specific sentence
    public void JumpToSentence(int sentenceIndex)
    {
        if (sentenceIndex >= 0 && sentenceIndex < Sentences.Length)
        {
            Index = sentenceIndex;
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            if (IsTyping && typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                IsTyping = false;
            }
            NextSentence();
        }
    }

    // Public method to end dialogue immediately
    public void EndDialogue()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        if (IsTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            IsTyping = false;
        }
        DialogueText.text = "";
        // Clear the sprite display
        if (SpriteDisplay != null)
        {
            SpriteDisplay.sprite = null;
            SpriteDisplay.enabled = false;
        }
        DialogueAnimator.SetTrigger("Exit");
        Index = 0;
        StartDialogue = true;
        IsDialogueRunning = false;
    }

    void NextSentence()
    {
        if (Index <= Sentences.Length - 1)
        {
            canvass.gameObject.SetActive(true); // Enable the Canvas
            DialogueText.text = "";
            
            // Play audio for current sentence if available
            if (SentenceAudios != null && SentenceAudios.Length > Index && SentenceAudios[Index] != null)
            {
                audioSource.clip = SentenceAudios[Index];
                audioSource.Play();
            }
            
            // Display sprite for current sentence based on SpriteIndices
            if (SpriteDisplay != null && AllSprites != null && SpriteIndices != null &&
                Index < SpriteIndices.Length && SpriteIndices[Index] >= 0 && 
                SpriteIndices[Index] < AllSprites.Length)
            {
                SpriteDisplay.sprite = AllSprites[SpriteIndices[Index]];
                SpriteDisplay.enabled = true;
            }
            else if (SpriteDisplay != null)
            {
                // No sprite for this sentence, hide the image component
                SpriteDisplay.enabled = false;
            }
            
            // Start typing effect
            typingCoroutine = StartCoroutine(WriteSentence());
        }
        else
        {   
            EndDialogue();
            canvass.gameObject.SetActive(false); // Disable the Canvas
        }
    }

    IEnumerator WriteSentence()
    {
        IsTyping = true;
        foreach(char Character in Sentences[Index].ToCharArray())
        {
            DialogueText.text += Character;
            yield return new WaitForSeconds(DialogueSpeed);
        }
        IsTyping = false;
        Index++;
    }
}