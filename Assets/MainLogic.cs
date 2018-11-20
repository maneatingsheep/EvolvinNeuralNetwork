using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainLogic : MonoBehaviour {

    public Brain BrainRef;

    public GameModel GameRef;

    
    public enum PlayRates { Fastest, Slow, OneMove, Manual};


    public PlayRates PlayRate = PlayRates.Manual;

    public Image GenImage;


    public Text ScoreText;
    public Text IterText;
    public Text GenText;

    private bool _isPlaying;
    private float _lastTime;

    private const float WAIT_BETWEEN_SLOW_MOVES = 1f;
    private const double TIME_BETWEEN_RENDER_MS = 1000;
    private const bool RENDER_ONLY_ON_GEN = true;


    //public int MAX_GEN;

    
    

    // Use this for initialization
    void Start () {
        

        GameRef.InitGame();
       

        for (int i = 0; i < GenImage.sprite.texture.width; i++) {
            for (int j = 0; j < GenImage.sprite.texture.height; j++) {
                GenImage.sprite.texture.SetPixel(i, j, Color.blue);
            }
        }
        GenImage.sprite.texture.Apply();

        BrainRef.Init();
        DoNextModelIteration();
    }

   
    // Update is called once per frame
    void Update () {
        switch (PlayRate) {
            case PlayRates.Fastest:
                DoMoves(false);
                break;
            case PlayRates.Slow:
                if (Time.time > _lastTime + WAIT_BETWEEN_SLOW_MOVES) {
                    DoMoves(true);
                }
                break;
            case PlayRates.OneMove:
                if (Input.GetKeyDown(KeyCode.Space)) {
                    DoMoves(true);
                }
                break;
            case PlayRates.Manual:
                if (PlayRate == PlayRates.Manual) {
                    GameRef.UseButtons();
                }
                break;
        }
    }


    private void DoNextModelIteration() {
        
        if (BrainRef.CurrentGen > -1) {
            BrainRef.CurrentGameScore = GameRef.GetScore();
            PlotGenScore(BrainRef.LastGenAvgScore, BrainRef.LastGenMaxScore);
        }

        BrainRef.DoNextGenIteration();

        //generic training
        if (BrainRef.CurrentVariant == 0) {
        //single game training
        //if (BrainRef.CurrentVariant == 0 && BrainRef.CurrentGen == 0) {
            GameRef.GenerateRandomSeed();
            
        }
        
        GameRef.ResetGame(BrainRef.CurrentGameIteration == 0);


        /*if (BrainRef.CurrentGen == MAX_GEN) {
            PlayRate = PlayRates.Manual;
        }*/

        
    }

    private void PlotGenScore(float lastGenAvgScore, float lastGenMaxScore) {
        for (int i = 0; i < 100; i++) {
            Color col;
            if (i < BrainRef.LastGenAvgScore * 3) {
                col = Color.red;
            } else if (i < BrainRef.LastGenMaxScore / 2) {
                col = Color.green;
            } else {
                col = Color.blue;
            }
            GenImage.sprite.texture.SetPixel((BrainRef.CurrentGen - 1) % 200, i, col);
            
        }
    }

    public void DoMoves(bool isSingle) {
        if (isSingle) { 
            if (PlayAIMove(true)) {
                DoNextModelIteration();
            }
            
        } else {
            bool timesUp = false;
            DateTime startTime = DateTime.Now;
            bool keepGoing = RENDER_ONLY_ON_GEN;

            while (!timesUp || keepGoing) {
                bool canPlayAnotherMove = true;
                while (canPlayAnotherMove && (!timesUp || keepGoing)) {
                    canPlayAnotherMove = !PlayAIMove(false);
                    timesUp = (DateTime.Now - startTime).TotalMilliseconds > TIME_BETWEEN_RENDER_MS;
                }
                if (!canPlayAnotherMove) {
                    DoNextModelIteration();
                }
                keepGoing = BrainRef.CurrentVariant != 0;
            }

        }
        
        RenderBoard();
    }

    

    private bool PlayAIMove(bool showDebugData) {
        _lastTime = Time.time;

        GameRef.GameStateToInputs(BrainRef.InputLayer);

        int AIDescision = BrainRef.CalculateMove(showDebugData);
        
        return GameRef.MakeMove(AIDescision);
    }

    private void RenderBoard() {
        GameRef.Render();


        IterText.text = "Sample: " + BrainRef.CurrentVariant;
        GenText.text = "Gen: " + BrainRef.CurrentGen;
        ScoreText.text = "Score: " + BrainRef.AvgGameScore + "/" + BrainRef.AllTimeMaxScore;

        GenImage.sprite.texture.Apply();
    }

}
