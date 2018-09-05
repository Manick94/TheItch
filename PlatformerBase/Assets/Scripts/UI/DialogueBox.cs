﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIAnchor))]
public class DialogueBox : MonoBehaviour {

    [SerializeField] private GameObject charPrefab; //gameobject prefab for individual letters
    [SerializeField] private Sprite[] letters; //array of all possible letter sprites

    private SpriteRenderer charachterImage; //ref to spriteRenderer of the image location 
    private List<SpriteRenderer> text; //refs to the spriteRenderers of the letter locations

    private const int numLines = 3; //number of lines in the dialoge box
    private const int charsPerLine = 16; //number of charachters in each line
    private int dialogueChunk; //which chunk of dialogue is currently being displayed

    private List<string> chunks; //chunks of dialogue that are displayed one at a time

    private void Awake()
    {
        gameObject.SetActive(false);
        dialogueChunk = -1; //default no dialogue 
        chunks = new List<string>(); 

        text = new List<SpriteRenderer>();
        GameObject newLetter;
        //Create all the placeHolders for charachter positions
        for(int line = 0; line< numLines; line++)
        {
            for(int cha = 0; cha<charsPerLine; cha++)
            {
                newLetter = Instantiate(charPrefab, transform);
                newLetter.name = "char" + line + "." + cha; 
                newLetter.transform.position = transform.position + 
                    new Vector3((cha * 1.125f) - 6.375f, (line * -1.375f) + 1.375f, 0); //TODO: make variables for adjustable 'font size'
                text.Add(newLetter.GetComponent<SpriteRenderer>());
            }
        }
        //Create the placeholder for the face image
        GameObject charachterFace = Instantiate(charPrefab, transform);
        charachterFace.name = "faceImage";
        charachterFace.transform.position = transform.position + new Vector3(-9.0f, 0.0f, 0.0f);
        charachterImage = charachterFace.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// display the next chunk of dialogue and an image in the dialogue box
    /// </summary>
    /// <param name="message">full message to display</param>
    /// <param name="face">image to display</param>
    public void OnTriggerKeyPressed(string message, Sprite face)
    {
        OnTriggerKeyPressed(message);
        charachterImage.sprite = face;
    }

    /// <summary>
    /// display the next chunk of dialogue and an image in the dialogue box
    /// </summary>
    /// <param name="message">full message to display</param>
    public void OnTriggerKeyPressed(string message)
    {
        gameObject.SetActive(true);
        //check if there new message
        if (dialogueChunk == -1)
        {
            CreateChunks(message);
        }
        //make sure there are chunks of dialogue to display
        if(chunks.Count != 0)
        {
            //advance dialogue to next or first chunk
            dialogueChunk++;
            //exit dialogue if there are no more chunks
            if (dialogueChunk >= chunks.Count)
            {
                Reset();
                return; //don't display the dialogue becuase there is none
            }
            DisplayChunk(chunks[dialogueChunk]);
        }
       
    }

    /// <summary>
    /// exit out of the dialogue and reset it back to the beginning 
    /// </summary>
    public void Reset()
    {
        dialogueChunk = -1; //reset dialogue
        gameObject.SetActive(false); //close dialogue box
    }

    /// <summary>
    /// divide a full message into chunks containing lines that will be displayed in sequence
    /// </summary>
    /// <param name="message">full message to divide</param>
    public void CreateChunks(string message)
    {
        //make sure list of chunks has only one element
        chunks.Clear(); 
        chunks.Add("");
        gameObject.SetActive(true);
        //parse throught each letter in the message
        string line = "";
        string word = "";
        for (int i = 0; i < message.Length; i++)
        {
            word += message[i];
            if (message[i] == 32 || i == message.Length - 1) //ASCII: (' ' = 32)
            {
                word = message[i] == 32 ? word.Substring(0, word.Length - 1) : word;//remove space from end of word
                //check if word extends past end of line
                if (line.Length + word.Length > charsPerLine)
                {
                    line += new string(' ', charsPerLine - line.Length); //fill in the rest of the line with spaces
                    chunks[chunks.Count - 1] += line; //add line to chunk
                    if(chunks[chunks.Count - 1].Length >= (numLines * charsPerLine)) { chunks.Add(""); } //create a new chunk when current one is full
                    line = ""; //start a new line
                }
                line += word + (line.Length + word.Length == charsPerLine ? "" : " "); //add word to line and space if not end of line
                //always add in the last line
                if (i == message.Length - 1)
                {
                    chunks[chunks.Count - 1] += line;
                }
                word = ""; //start a new word
            }
        }
        chunks[chunks.Count - 1] += new string(' ', (charsPerLine * numLines) - chunks[chunks.Count - 1].Length); //fill in the rest of the final chunk
    }

    /// <summary>
    /// sets the images for each of the individual charachters in the dialogue box
    /// </summary>
    /// <param name="message">charachters to display</param>
    private void DisplayChunk(string message)
    {
        //make sure message is valid
        if(message.Length <= (numLines * charsPerLine))
        {
            message = message.ToLower(); //make all letters lowercase
            for(int i = 0; i < message.Length; i++)
            { 
                char letter = message[i]; 
                int spriteNum = 12; //default to blank space in case of unassign char
                //is letter an number 
                if (letter >= 48 && letter <= 57) //ASCII: ('0' = 48) ('9' = 57)
                {
                    spriteNum = letter - 48; //Sprites: ('0' = 0) ('9' = 9)
                }
                //is letter part of the alpabet
                else if(letter >= 97 && letter <= 122) //ASCII: ('a' = 97) ('z' = 122)
                {
                    spriteNum = letter - 84; //Sprites: ('a' = 13) ('z' = 38)
                }
                //other charachters
                else
                {
                    switch ((int)letter)
                    {
                        case 46: //ASCII: ('.' = 46)
                            spriteNum = 10; //Sprites: ('.' = 10)
                            break;
                        case 44: //ASCII: (',' = 44)
                            spriteNum = 11; //Sprites: (',' = 11)
                            break;
                    }
                }
                text[i].sprite = letters[spriteNum];
            }
        }
        //Something has gone wrong in Display Message, string passed in was too long 
        else
        {
            Debug.Log("message has more than" + (numLines * charsPerLine) + "charachters");
        }
    }
}