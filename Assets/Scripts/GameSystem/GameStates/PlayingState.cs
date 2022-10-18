﻿using BoardSystem;
using ChessSystem;
using CommandSystem;
using Cysharp.Threading.Tasks;
using GameSystem.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem.GameStates
{
    internal class PlayingState : GameState
    {
        public const string Name = "Playing";
        
        private readonly CommandQueue _commandQueue;
        private readonly Board<PieceView> _board;
        private readonly Engine<PieceView> _engine;

        private BoardView _boardView;
        private Position? _fromPosition;

        public PlayingState()
        {
            _commandQueue = new CommandQueue();
            _board = new Board<PieceView>(PositionHelper.Rows, PositionHelper.Columns);
            _engine = new Engine<PieceView>(_board, _commandQueue);

            _board.PieceMoved += (s, e) => e.Piece.MoveTo(PositionHelper.WorldPosition(e.ToPosition));

        }


        public override async UniTask OnEnter()
        {
            await SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);

            _boardView = GameObject.FindObjectOfType<BoardView>();
            var pieces = GameObject.FindObjectsOfType<PieceView>();
            foreach (var piece in pieces)
                _board.Place(piece, PositionHelper.GridPosition(piece.WorldPosition));
        }

        public override async UniTask OnExit()
        {
            await base.OnExit();
            await SceneManager.UnloadSceneAsync(2);
        }

        public override async UniTask OnResume()
        {
            await base.OnResume();
            
            if(_boardView != null)
                _boardView.PositionSelected += SelectTile;
        }

        public override async UniTask OnSuspend()
        {
            await base.OnResume();

            if (_boardView != null)
                _boardView.PositionSelected -= SelectTile;
        }


        private void SelectTile(object source, PositionEventArgs eventArgs)
        {
            
            var position = eventArgs.Position;
            if (_engine.TryGetPiece(position, out var piece))
            {
                _fromPosition = position;
                _boardView.SetActivePositions(_engine.MoveSets[piece.Type].Positions(position));
            }
            else if (_fromPosition != null && _engine.Move((Position)_fromPosition, position))
            {
                _boardView.SetActivePositions(new List<Position>());
            }
                
        }
    }
}
