using System;

public class NetworkModel {

    internal const float BIG_VARIATION_SIZE = 0.8f;
    internal const float SMALL_VARIATION_SIZE = 0.1f; //0.15f worked well
    internal float HALF_BIG_VARIATION_SIZE;
    internal float HALF_SMALL_VARIATION_SIZE;
    internal float[] mutationChance = new float[] { 0.0f, 1.0f, 0.95f };
    

    public float[][][] _Weights;


    private const float CONNECTION_INIT_VAL = 0f;

    public float[] Scores;
    public float MaxGameScore = 0;
    public float AvgGameScore = 0;


    public NetworkModel() {

        HALF_BIG_VARIATION_SIZE = BIG_VARIATION_SIZE / 2f;
        HALF_SMALL_VARIATION_SIZE = SMALL_VARIATION_SIZE / 2f;

        Scores = new float[Brain.NUM_OF_GAMES_PER_ITERATION];
        

        _Weights = new float[Brain.TOTAL_LAYERS_NUM][][];

        for (int l = 0; l < Brain.TOTAL_LAYERS_NUM; l++) {
            if (l == 0) {
                _Weights[l] = new float[Brain.INPUT_SIZE][];
            } else {
                _Weights[l] = new float[Brain.HIDDEN_LAYERS_SIZE][];
            }
            for (int i = 0; i < _Weights[l].Length; i++) {
                if (l == Brain.TOTAL_LAYERS_NUM - 1) {
                    _Weights[l][i] = new float[Brain.OUTPUT_SIZE];
                } else {
                    _Weights[l][i] = new float[Brain.HIDDEN_LAYERS_SIZE];

                }
                for (int j = 0; j < _Weights[l][i].Length; j++) {
                    _Weights[l][i][j] = CONNECTION_INIT_VAL;
                }
            }
        }


    }


    public void Randomize(NetworkModel[] parents, System.Random rnd, bool doBreed) {
       

        for (int i = 0; i < _Weights.Length; i++) {
            for (int j = 0; j < _Weights[i].Length; j++) {
                for (int k = 0; k < _Weights[i][j].Length; k++) {

                    int pNum = (doBreed) ? rnd.Next(Brain.PARENTS_NUM) : 0;

                    float mutationType = (float)rnd.NextDouble();
                    if (mutationType < mutationChance[0]) {
                        //nothing happanes
                    } else if (mutationType < mutationChance[1]) {
                        //big mutation
                        _Weights[i][j][k] = parents[pNum]._Weights[i][j][k] + ((float)rnd.NextDouble() * SMALL_VARIATION_SIZE) - HALF_SMALL_VARIATION_SIZE;
                    } else if (mutationType < mutationChance[2]) {

                        _Weights[i][j][k] = parents[pNum]._Weights[i][j][k] + ((float)rnd.NextDouble() * BIG_VARIATION_SIZE) - HALF_BIG_VARIATION_SIZE;
                    } else {
                        _Weights[i][j][k] = -parents[pNum]._Weights[i][j][k];
                    }

                        
                }
            }
        }


    }

    internal static void Duplicate(NetworkModel src, NetworkModel dest) {

        for (int i = 0; i < src._Weights.Length; i++) {
            for (int j = 0; j < src._Weights[i].Length; j++) {
                for (int k = 0; k < src._Weights[i][j].Length; k++) {
                    dest._Weights[i][j][k] = src._Weights[i][j][k];
                }
            }
        }

    }
}