

using System;
using UnityEngine;

public class Game2048 : GameModel {

    public enum Direction { Up, Down, Left, Right };

    public VisualTile[] InputTiles;
    private LogicTile[][] _tiles;

    private float _score;

    internal const int SIDE_LEN = 4;
    internal const int STARTING_PICIES = 2;
    private float[] SPAWNING_CHANCE = new float[] { 0.85f, 0.15f };

    private int _randomSeed = 0;
    private System.Random _rnd;

    private int ScanInitialX;
    private int ScanInitialY;
    private int SlowScanDirectionX;
    private int SlowScanDirectionY;
    private int FastScanDirectionX;
    private int FastScanDirectionY;
    private int ShiftDirectionX;
    private int ShiftDirectionY;

    private int ScannerX;
    private int ScannerY;
    private int ShiftScannerX;
    private int ShiftScannerY;


    override public void InitGame() {
        _tiles = new LogicTile[SIDE_LEN][];

        for (int i = 0; i < SIDE_LEN * SIDE_LEN; i++) {
            int coll = i % SIDE_LEN;
            int row = Mathf.FloorToInt(i / SIDE_LEN);

            if (coll == 0) {
                _tiles[row] = new LogicTile[SIDE_LEN];
            }

            _tiles[row][coll] = new LogicTile();

            InputTiles[i].Src = _tiles[row][coll];
        }
    }

    override public void ResetGame(bool useRandomSeed) {
        if (useRandomSeed) {
            _rnd = new System.Random(_randomSeed);
        }

        for (int i = 0; i < SIDE_LEN; i++) {
            for (int j = 0; j < SIDE_LEN; j++) {
                _tiles[i][j].PossibleNumValue = 0;
                _tiles[i][j].PossibleAction = LogicTile.ActionType.None;
                _tiles[i][j].Finlize();

            }
        }

        for (int p = 0; p < STARTING_PICIES; p++) {
            SpawnTile(1);
        }

        _score = 0;
        
    }

    override public void GenerateRandomSeed() {
        _randomSeed = (int)DateTime.Now.Ticks;
    }

    private void SpawnTile(int tileValue) {
        if (tileValue == -1) {
            double spawnChance = _rnd.NextDouble();

            float currentBar = 0f;
            bool chanceMet = false;
            for (int s = 0; s < SPAWNING_CHANCE.Length; s++) {
                currentBar += SPAWNING_CHANCE[s];

                if (spawnChance <= currentBar) {
                    tileValue = s + 1;
                    chanceMet = true;
                    break;
                }
            }
            if (!chanceMet) {
                tileValue = SPAWNING_CHANCE.Length;
            }
        }

        int emptyPlaces = 0;
        for (int i = 0; i < SIDE_LEN; i++) {
            for (int j = 0; j < SIDE_LEN; j++) {
                if (_tiles[i][j].NumValue == 0) {
                    emptyPlaces++;
                }
            }
        }


        int wantedPosition = _rnd.Next(emptyPlaces);

        int index = 0;
        for (int i = 0; i < SIDE_LEN; i++) {
            for (int j = 0; j < SIDE_LEN; j++) {

                if (_tiles[i][j].NumValue == 0) {

                    if (index == wantedPosition) {

                        _tiles[i][j].PossibleNumValue = tileValue; ;
                        _tiles[i][j].PossibleAction = LogicTile.ActionType.New;
                        _tiles[i][j].Finlize();

                        i = SIDE_LEN;
                        j = SIDE_LEN;


                    } else {
                        index++;

                    }
                }


            }
        }
    }


    //return if this is the game ending move
    override public bool MakeMove(int input) {
        switch (input) {
            case 0: return MakeMove(Direction.Up);
            case 1: return MakeMove(Direction.Down);
            case 2: return MakeMove(Direction.Left);
            default: return MakeMove(Direction.Right);
        }
    }

    public bool MakeMove(Direction direction) {

        switch (direction) {
            case Direction.Up:
                ScanInitialX = 0;
                ScanInitialY = 0;
                SlowScanDirectionX = 0;
                SlowScanDirectionY = 1;
                FastScanDirectionX = 1;
                FastScanDirectionY = 0;
                break;
            case Direction.Down:
                ScanInitialX = 0;
                ScanInitialY = SIDE_LEN - 1;
                SlowScanDirectionX = 0;
                SlowScanDirectionY = -1;
                FastScanDirectionX = 1;
                FastScanDirectionY = 0;

                break;
            case Direction.Left:
                ScanInitialX = 0;
                ScanInitialY = 0;
                SlowScanDirectionX = 1;
                SlowScanDirectionY = 0;
                FastScanDirectionX = 0;
                FastScanDirectionY = 1;
                break;
            default:
                ScanInitialX = SIDE_LEN - 1;
                ScanInitialY = 0;
                SlowScanDirectionX = -1;
                SlowScanDirectionY = 0;
                FastScanDirectionX = 0;
                FastScanDirectionY = 1;
                break;
        }

        ShiftDirectionX = SlowScanDirectionX * -1;
        ShiftDirectionY = SlowScanDirectionY * -1;

        ScannerX = ScanInitialX;
        ScannerY = ScanInitialY;

        bool moveMade = false;
        int mergeScore = 0;

        for (int i = 0; i < SIDE_LEN * SIDE_LEN; i++) {

            //shift tile scanner.x scanner .y
            if (_tiles[ScannerX][ScannerY].PossibleNumValue > 0) {
                ShiftScannerX = ScannerX;
                ShiftScannerY = ScannerY;

                bool inBounds = true;
                bool isTargetEmpty = true;

                while (inBounds && isTargetEmpty) {

                    bool merge = false;

                    ShiftScannerX += ShiftDirectionX;
                    ShiftScannerY += ShiftDirectionY;

                    inBounds = ShiftScannerX >= 0 && ShiftScannerX < SIDE_LEN && ShiftScannerY >= 0 && ShiftScannerY < SIDE_LEN;

                    if (inBounds) {
                        isTargetEmpty = (_tiles[ShiftScannerX][ShiftScannerY].PossibleNumValue == 0);
                    }

                    if (inBounds) {
                        merge = !isTargetEmpty && _tiles[ScannerX][ScannerY].PossibleNumValue == _tiles[ShiftScannerX][ShiftScannerY].PossibleNumValue;
                    }

                    if (merge) {
                        mergeScore += _tiles[ShiftScannerX][ShiftScannerY].PossibleNumValue;
                    }

                    moveMade |= ((inBounds && isTargetEmpty) || merge);


                    if (!inBounds || !isTargetEmpty) {

                        if (merge) {
                            _tiles[ShiftScannerX][ShiftScannerY].PossibleAction = LogicTile.ActionType.Grow;
                        } else {
                            ShiftScannerX -= ShiftDirectionX;
                            ShiftScannerY -= ShiftDirectionY;
                        }
                        int numValue = _tiles[ScannerX][ScannerY].PossibleNumValue + ((merge) ? 1 : 0);

                        _tiles[ScannerX][ScannerY].PossibleNumValue = 0;
                        _tiles[ShiftScannerX][ShiftScannerY].PossibleNumValue = numValue;

                    }

                }

            }

            //progress scanner
            bool isBoreder = (i % SIDE_LEN) == SIDE_LEN - 1;
            ScannerX = ScannerX + ((isBoreder) ? (SlowScanDirectionX + (FastScanDirectionX * -(SIDE_LEN - 1))) : (FastScanDirectionX));
            ScannerY = ScannerY + ((isBoreder) ? (SlowScanDirectionY + (FastScanDirectionY * -(SIDE_LEN - 1))) : (FastScanDirectionY));
        }

        for (int i = 0; i < SIDE_LEN; i++) {
            for (int j = 0; j < SIDE_LEN; j++) {
                _tiles[i][j].Finlize();
            }
        }

        if (moveMade) {
            SpawnTile(-1);
        }

        _score += ((mergeScore <= 0) ? -1 : mergeScore * 2);

        return !moveMade;

    }

    override public void GameStateToInputs(float[] inputsToFill) {
        //fill input
        for (int i = 0; i < _tiles.Length; i++) {
            for (int j = 0; j < _tiles[i].Length; j++) {
                inputsToFill[i * SIDE_LEN + j] = Mathf.Pow(2, _tiles[i][j].NumValue);
            }
        }
    }

    override public float GetScore() {
        return _score;
    }

    override public void Render() {
        for (int i = 0; i < InputTiles.Length; i++) {
            InputTiles[i].Render();
        }
    }

    override public void UseButtons() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            MakeMove(Direction.Up);
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            MakeMove(Direction.Down);
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            MakeMove(Direction.Left);
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            MakeMove(Direction.Right);
        }
    }
}
