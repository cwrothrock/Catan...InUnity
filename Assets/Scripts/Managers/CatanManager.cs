using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class CatanManager : MonoBehaviour
{
    public static CatanManager Instance { get; private set; }

    public event EventHandler OnStateChanged;

    private enum State {
        GameSetup,
        GamePlaying,
        GameOver,
    }

    private State state = State.GameSetup;
    private float gamePlayingTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {

    }

    void Update()
    {
        switch (state) {
            case State.GameSetup:
                gamePlayingTimer += Time.deltaTime;
                break;
            case State.GamePlaying:
                gamePlayingTimer += Time.deltaTime;
                break;
            case State.GameOver:
                // Trigger Game Over UI
                break;
        }
    }

    public bool IsSetup() => state == State.GameSetup;
    public bool IsPlaying() => state == State.GamePlaying;
    public bool IsGameOver() => state == State.GameOver;
}
