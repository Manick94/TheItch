﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : Button
{
    [SerializeField] private string levelName;
    [SerializeField] private LoadSceneMode mode;

    [SerializeField] private bool credits = false;

    protected override void OnActive()
    {
        render.sprite = inactive;
    }

    protected override void OnClick()
    {
        if (!credits)
        {
            GameSaver.FolderNumber = GetComponentInParent<SaveDisplay>().saveNumber;
            GameSaver.gameData = null;
            Inventory.ClearItemStates();
            BackgroundAudioPlayer.menu = false;
        }
        SceneManager.LoadScene(levelName, mode);
    }

    protected override void OnEnter()
    {
        render.sprite = active;
    }
}
