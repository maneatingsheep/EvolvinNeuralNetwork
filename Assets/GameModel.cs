
using System;
using UnityEngine;

public abstract class GameModel : MonoBehaviour {

    abstract public void InitGame();

    abstract public void GenerateRandomSeed();

    abstract public void ResetGame(bool useRandomSeed);

    //return move score
    abstract public bool MakeMove(int input);

    abstract public void GameStateToInputs(float[] inputsToFill);

    abstract public float GetScore();

    abstract public void Render();

    abstract public void UseButtons();
     
}
