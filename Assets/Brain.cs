using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Brain : MonoBehaviour {

    public bool DoBreed;

    internal const int TOTAL_LAYERS_NUM = 5;
    internal const int HIDDEN_LAYERS_SIZE = 64;

    internal const int INPUT_SIZE = 16;
    internal const int OUTPUT_SIZE = 4;

    internal const int NUM_OF_GAMES_PER_ITERATION = 5;

    private const int VARIATIONS_NUM = 500;
    internal const int PARENTS_NUM = 4;


    private NetworkModel[] VariationModels;
    private NetworkModel[] OriginalModels;

    internal int CurrentGen = -1;
    internal int CurrentGameIteration = 0;

    internal int CurrentVariant = 0;

    private System.Random _rnd;

    private List<int> _genScoresFrequency = new List<int>();

    
    public float[][] _layers;

    public float LastGenMaxScore = float.MinValue;
    public float LastGenAvgScore = 0;
    public float AllTimeMaxScore = 0;

    public Image ModelImage;

    public float MaxGameScore {
        get {
            return VariationModels[CurrentVariant].MaxGameScore;
        }
    }

    public float AvgGameScore {
        get {
            return VariationModels[CurrentVariant].AvgGameScore;
        }
    }

    public float CurrentGameScore {
        get {
            return VariationModels[CurrentVariant].Scores[CurrentGameIteration];
        }
        set {
            VariationModels[CurrentVariant].Scores[CurrentGameIteration] = value;
        }
    }

    public float[] InputLayer {
        get {
            return _layers[0];
        }
    }

    public void Init() {

        _rnd = new System.Random();
        OriginalModels = new NetworkModel[PARENTS_NUM];
        for (int i = 0; i < PARENTS_NUM; i++) {
            OriginalModels[i] = new NetworkModel();
        }

        VariationModels = new NetworkModel[VARIATIONS_NUM];
        for (int i = 0; i < VARIATIONS_NUM; i++) {
            VariationModels[i] = new NetworkModel();
        }

        //activators

        _layers = new float[TOTAL_LAYERS_NUM][];

        for (int i = 0; i < TOTAL_LAYERS_NUM; i++) {
            if (i == 0) {
                _layers[i] = new float[INPUT_SIZE];
            } else if (i == TOTAL_LAYERS_NUM - 1) {
                _layers[i] = new float[OUTPUT_SIZE];
            } else {
                _layers[i] = new float[HIDDEN_LAYERS_SIZE];
            }
            
        }
    }

    internal void DoNextGenIteration() {


        //finilize ending iteration
        bool isLastGameIteration = CurrentGameIteration == NUM_OF_GAMES_PER_ITERATION - 1;

        //every last game iteration
        if (CurrentGen > -1 && isLastGameIteration) {
            //finilize score

            VariationModels[CurrentVariant].AvgGameScore = 0;

            //average games
            for (int j = 0; j < VariationModels[CurrentVariant].Scores.Length; j++) {
                VariationModels[CurrentVariant].MaxGameScore = Mathf.Max(VariationModels[CurrentVariant].MaxGameScore, VariationModels[CurrentVariant].Scores[j]);
                VariationModels[CurrentVariant].AvgGameScore += VariationModels[CurrentVariant].Scores[j];

            }

            VariationModels[CurrentVariant].AvgGameScore = VariationModels[CurrentVariant].AvgGameScore / NUM_OF_GAMES_PER_ITERATION;

        }


        //init starting iteration
        if ((CurrentVariant == VARIATIONS_NUM - 1 && isLastGameIteration) || CurrentGen == -1) { //next gen
           
            CreateNextGen();
            CurrentVariant = 0;
            CurrentGameIteration = 0;


            _genScoresFrequency.Clear();

        } else {
            if (isLastGameIteration) { //next sibling
                CurrentVariant++;
                CurrentGameIteration = 0;
            } else {
                CurrentGameIteration++; //next game
            }

            
        }

        VariationModels[CurrentVariant].Scores[CurrentGameIteration] = 0;
    }

    public void CreateNextGen() {
        
        CurrentGen++;

        if (CurrentGen > 0) {

            LastGenMaxScore = int.MinValue;
            LastGenAvgScore = 0;

            for (int i = 0; i < VariationModels.Length; i++) {
                LastGenAvgScore += VariationModels[i].AvgGameScore;
            }

            LastGenAvgScore /= VARIATIONS_NUM;

            //find best variant

            Array.Sort(VariationModels, delegate (NetworkModel a, NetworkModel b) {
                return b.AvgGameScore.CompareTo(a.AvgGameScore);
            });

            
            LastGenMaxScore = Mathf.Max(LastGenMaxScore, VariationModels[0].AvgGameScore);
            AllTimeMaxScore = Mathf.Max(AllTimeMaxScore, VariationModels[0].AvgGameScore);


            PrintLastGenReport();

            //reset scores
            for (int i = 0; i < VariationModels.Length; i++) {
                for (int j = 0; j < VariationModels[i].Scores.Length; j++) {
                    VariationModels[i].Scores[j] = 0;
                }
            }



            //set new original model
            
            for (int i = 0; i < PARENTS_NUM; i++) {
                NetworkModel.Duplicate(VariationModels[i], OriginalModels[i]);
            }


        }

        //DrawOriginalNetwok();

        for (int i = 0; i < VariationModels.Length; i++) {
            VariationModels[i].Randomize(OriginalModels, _rnd, DoBreed);
        }

    }

    private void PrintLastGenReport() {
        print(CurrentGen + " - avg: " + LastGenAvgScore + " max: " + LastGenMaxScore);
    }

    internal int CalculateMove(bool showDebugData) {

        //fill layers
        for (int l = 0; l < TOTAL_LAYERS_NUM - 1; l++) {
            if (l > 0) {
                //NormlizeLayer(_layers[l]);
            }

            for (int i = 0; i < _layers[l].Length; i++) {
                for (int j = 0; j < _layers[l + 1].Length; j++) {
                    if (i == 0) {
                        _layers[l + 1][j] = 0;
                    }
                    //_layers[l + 1][j] += _layers[l][i] * VariationModels[CurrentVariant]._Weights[l][i][j];
                    _layers[l + 1][j] += Activation(_layers[l][i], VariationModels[CurrentVariant]._Weights[l][i][j]);
                }
            }
        }

        int bestIndex = 0;
        float higestOutput = float.MinValue;

        string debugStr = "";

        for (int i = 0; i < _layers[_layers.Length - 1].Length; i++) {
            if (showDebugData) {
                debugStr += _layers[_layers.Length - 1][i] + " ";
            }
            if (_layers[_layers.Length - 1][i] > higestOutput) {
                higestOutput = _layers[_layers.Length - 1][i];
                bestIndex = i;
            }
        }

        if (showDebugData) {
            debugStr += bestIndex;
            print(debugStr);
        }

        return bestIndex;
    }

    private float Activation(float input, float weight) {
        return (float)Math.Tanh(input) * weight;
    }

    private void NormlizeLayer(float[] layer) {
        
        float minVal = float.MaxValue;
        float maxVal = float.MinValue;
        for (int i = 0; i < layer.Length; i++) {
            if (layer[i] < minVal) {
                minVal = layer[i];
            }
            if (layer[i] > maxVal) {
                maxVal = layer[i];
            }
        }

        float scale = (maxVal - minVal);

        for (int i = 0; i < layer.Length; i++) {

            layer[i] = (layer[i] - minVal) / scale;
            //layer[i] = (layer[i] - minVal);

        }
    }

    private void DrawOriginalNetwok() {
        int posX = 0;
        int posY = 0;

        for (int i = 0; i < OriginalModels[0]._Weights.Length; i++) {
            for (int j = 0; j < OriginalModels[0]._Weights[i].Length; j++) {
                for (int k = 0; k < OriginalModels[0]._Weights[i][j].Length; k++) {
                    ModelImage.sprite.texture.SetPixel(posX, posY, GetColorFromValue(OriginalModels[0]._Weights[i][j][k]));
                    posX++;
                    if (posX == ModelImage.sprite.texture.width) {
                        posX = 0;
                        posY++;
                    }
                }
            }
        }


        ModelImage.sprite.texture.Apply();
    }

    private Color GetColorFromValue(float val) {
        float colorToneVal = val + 0.5f;
        return new Color(colorToneVal, colorToneVal, colorToneVal);
    }
}


