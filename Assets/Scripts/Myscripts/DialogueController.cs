using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video; // Add this

public class DialogueController : MonoBehaviour
{
    [Header("Dialogue Content")]
    public TextMeshProUGUI DialogueText;
    public string[] Sentences;
    public AudioClip[] SentenceAudios;
    public Sprite[] AllSprites;
    public int[] SpriteIndices;

    [Header("Video Content")]
    public VideoClip[] AllVideos;         // All available video clips
    public int[] VideoIndices;            // Which video to show for each sentence (index into AllVideos)
    public int VideoTriggerSentenceIndex = -1; // Sentence index after which videos should start appearing (-1 means videos can appear from the beginning)

    [Header("UI References")]
    public Image SpriteDisplay;
    public RawImage VideoDisplay;         // UI RawImage for video
    public VideoPlayer videoPlayer;       // VideoPlayer component
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

    public bool IsDialogueRunning { get; private set; }
    public bool IsTyping { get; private set; }

    private Coroutine typingCoroutine;
    private bool canShowVideos = false; // Flag to control when videos can be shown

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Setup VideoPlayer
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.Stop();
        if (VideoDisplay != null)
        {
            if (videoPlayer.targetTexture == null)
                videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
            VideoDisplay.texture = videoPlayer.targetTexture;
            VideoDisplay.enabled = false;
            VideoDisplay.gameObject.SetActive(false); // Disable the video object at start
        }

        IsDialogueRunning = false;
        IsTyping = false;

        // Initialize video visibility flag
        canShowVideos = (VideoTriggerSentenceIndex < 0);

        if (AutoStartDialogue)
            StartCoroutine(AutoStartWithDelay());
    }

    private IEnumerator AutoStartWithDelay()
    {
        yield return new WaitForSeconds(AutoStartDelay);
        StartDialogueSequence();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(IsDialogueRunning)
                ProgressDialogue();
            else
                EndDialogue();
        }
    }

    public void StartDialogueSequence()
    {
        DialogueAnimator.SetTrigger("Enter");
        StartDialogue = false;
        IsDialogueRunning = true;
        canShowVideos = (VideoTriggerSentenceIndex < 0); // Reset video visibility flag
        NextSentence();
    }

    public void ProgressDialogue()
    {
        if (IsTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            IsTyping = false;
            DialogueText.text = Sentences[Index];
            Index++;
            
            // Check if we've passed the trigger sentence for videos
            if (!canShowVideos && VideoTriggerSentenceIndex >= 0 && Index > VideoTriggerSentenceIndex)
            {
                canShowVideos = true;
            }
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            if (videoPlayer.isPlaying)
                videoPlayer.Stop();
            NextSentence();
        }
    }

    public void SetDialogueContent(string[] newSentences, AudioClip[] newAudioClips, Sprite[] newSprites, int[] newSpriteIndices, VideoClip[] newVideos, int[] newVideoIndices, int newVideoTriggerSentenceIndex = -1)
    {
        Sentences = newSentences;
        SentenceAudios = newAudioClips;
        AllSprites = newSprites;
        SpriteIndices = newSpriteIndices;
        AllVideos = newVideos;
        VideoIndices = newVideoIndices;
        VideoTriggerSentenceIndex = newVideoTriggerSentenceIndex;
        Index = 0;
        canShowVideos = (VideoTriggerSentenceIndex < 0);
    }

    public void SetVideoTriggerSentence(int triggerSentenceIndex)
    {
        VideoTriggerSentenceIndex = triggerSentenceIndex;
        canShowVideos = (VideoTriggerSentenceIndex < 0) || (Index > VideoTriggerSentenceIndex);
    }

    public void AssignSpriteToSentence(int sentenceIndex, int spriteIndex)
    {
        // ... (unchanged)
    }

    public void AssignVideoToSentence(int sentenceIndex, int videoIndex)
    {
        if (sentenceIndex >= 0 && sentenceIndex < Sentences.Length)
        {
            if (VideoIndices == null)
            {
                VideoIndices = new int[Sentences.Length];
                for (int i = 0; i < VideoIndices.Length; i++)
                    VideoIndices[i] = -1;
            }
            else if (VideoIndices.Length < Sentences.Length)
            {
                int[] newIndices = new int[Sentences.Length];
                for (int i = 0; i < VideoIndices.Length; i++)
                    newIndices[i] = VideoIndices[i];
                for (int i = VideoIndices.Length; i < newIndices.Length; i++)
                    newIndices[i] = -1;
                VideoIndices = newIndices;
            }
            VideoIndices[sentenceIndex] = videoIndex;
        }
    }

    public void JumpToSentence(int sentenceIndex)
    {
        // ... (unchanged)
    }

    public void EndDialogue()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
        if (videoPlayer.isPlaying)
            videoPlayer.Stop();
        if (IsTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            IsTyping = false;
        }
        DialogueText.text = "";
        if (SpriteDisplay != null)
        {
            SpriteDisplay.sprite = null;
            SpriteDisplay.enabled = false;
        }
        if (VideoDisplay != null)
        {
            VideoDisplay.texture = videoPlayer.targetTexture;
            VideoDisplay.enabled = false;
            VideoDisplay.gameObject.SetActive(false); // Hide video object at end
        }
        DialogueAnimator.SetTrigger("Exit");
        Index = 0;
        StartDialogue = true;
        IsDialogueRunning = false;
        canShowVideos = (VideoTriggerSentenceIndex < 0); // Reset video visibility flag
    }

    void NextSentence()
    {
        if (Index <= Sentences.Length - 1)
        {
            canvass.gameObject.SetActive(true);
            DialogueText.text = "";

            // Check if we've passed the trigger sentence for videos
            if (!canShowVideos && VideoTriggerSentenceIndex >= 0 && Index > VideoTriggerSentenceIndex)
            {
                canShowVideos = true;
            }

            // Play audio
            if (SentenceAudios != null && SentenceAudios.Length > Index && SentenceAudios[Index] != null)
            {
                audioSource.clip = SentenceAudios[Index];
                audioSource.Play();
            }

            // Display sprite
            if (SpriteDisplay != null && AllSprites != null && SpriteIndices != null &&
                Index < SpriteIndices.Length && SpriteIndices[Index] >= 0 &&
                SpriteIndices[Index] < AllSprites.Length)
            {
                SpriteDisplay.sprite = AllSprites[SpriteIndices[Index]];
                SpriteDisplay.enabled = true;
            }
            else if (SpriteDisplay != null)
            {
                SpriteDisplay.enabled = false;
            }

            // Play video only if assigned for this sentence AND videos are allowed to show
            if (canShowVideos && VideoDisplay != null && AllVideos != null && VideoIndices != null &&
                Index < VideoIndices.Length && VideoIndices[Index] >= 0 &&
                VideoIndices[Index] < AllVideos.Length)
            {
                videoPlayer.clip = AllVideos[VideoIndices[Index]];
                if (videoPlayer.targetTexture == null)
                    videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
                VideoDisplay.texture = videoPlayer.targetTexture;
                VideoDisplay.enabled = true;
                VideoDisplay.gameObject.SetActive(true); // Enable video object only when needed
                videoPlayer.Stop();
                videoPlayer.Play();
            }
            else if (VideoDisplay != null)
            {
                VideoDisplay.enabled = false;
                VideoDisplay.gameObject.SetActive(false); // Disable video object when not needed
                if (videoPlayer.isPlaying)
                    videoPlayer.Stop();
            }

            typingCoroutine = StartCoroutine(WriteSentence());
        }
        else
        {
            EndDialogue();
            canvass.gameObject.SetActive(false);
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
        
        // Check if we've passed the trigger sentence for videos after typing is complete
        if (!canShowVideos && VideoTriggerSentenceIndex >= 0 && Index > VideoTriggerSentenceIndex)
        {
            canShowVideos = true;
        }
    }
}