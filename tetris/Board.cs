﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tetris
{
    public class Board
    {
        private BoardContent BoardContent;

        // QUIZ: What does `auto-implemented property` mean?
        // LEARN MORE at https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/auto-implemented-properties
        public int CurrentRow { get; private set; } = 0;
        public int CurrentCol { get; private set; } = 0;
        public Piece CurrentPiece { get; private set; } = null;
        public Piece NextPiece { get; private set; } = null;

        // QUIZ: What does the `= null` assignment mean in the parameter list?
        // LEARN MORE at https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments#optional-arguments
        public Board(BoardContent content)
        {
            BoardContent = content;
        }

        public void NewPiece()
        {
            // QUIZ: What does `??` mean in the next line of code?
            // LEARN MORE at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-conditional-operator
            CurrentPiece = NextPiece ?? Pieces.GetRandomPiece();
            NextPiece = Pieces.GetRandomPiece();

            // Set initial position
            CurrentRow = 0;
            CurrentCol = (BoardContent.Width - CurrentPiece.Width) / 2;

            if (!CanMergePatternIntoBoardContent(CurrentRow, CurrentCol, CurrentPiece.Pattern))
            {
                // Initial position already occupied -> GAME OVER
                throw new BoardException();
            }
        }

        public void DropPiece()
        {
            // Start in current row
            var row = CurrentRow;

            // Find first row which is already occupied
            for (; row <= BoardContent.Height - CurrentPiece.Height; row++)
            {
                if (!CanMergePatternIntoBoardContent(row, CurrentCol, CurrentPiece.Pattern))
                {
                    // Occupied row found -> piece has to land one row above
                    CurrentRow = row - 1;
                    return;
                }
            }

            // No row occupied -> place piece in lowest row
            CurrentRow = row - 1;
        }

        // QUIZ: How is the C# concept called that enables queries like `Where` and `Any`?
        // LEARN MORE at https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/quantifier-operations
        public bool CanMergePatternIntoBoardContent(int targetRow, int targetCol, bool[,] pattern) =>
            // Check whether any target pixel is already occupied
            !pattern.ToEnumerable()
                .Where(item => item.val)
                .Any(item => item.val && BoardContent[targetRow + item.row, targetCol + item.col]);

        public bool IsMovePossible(Direction direction)
        {
            switch (direction)
            {
                case Direction.Down:
                    // QUIZ: Will `CanMergePatternIntoBoardContent` be called if bottom has already been reached?
                    // LEARN MORE at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-or-operator
                    return !((CurrentRow + CurrentPiece.Height) == BoardContent.Height ||
                        !CanMergePatternIntoBoardContent(CurrentRow + 1, CurrentCol, CurrentPiece.Pattern));
                case Direction.Left:
                    return !(CurrentCol == 0 || !CanMergePatternIntoBoardContent(CurrentRow, CurrentCol - 1, CurrentPiece.Pattern));
                case Direction.Right:
                    return !((CurrentCol + CurrentPiece.Width) == BoardContent.Width ||
                        !CanMergePatternIntoBoardContent(CurrentRow, CurrentCol + 1, CurrentPiece.Pattern));
                default:
                    // QUIZ: Does this make sense? `direction` cannot be anything but down, left or right, can it?
                    // LEARN MORE at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/enum#robust-programming
                    throw new InvalidOperationException();
            }
        }

        public bool TryRotatePiece(RotationDirection direction)
        {
            var rotatedPiece = CurrentPiece.GetRotatedPiece(direction);
            if ((CurrentRow + rotatedPiece.Height) > BoardContent.Height
                || (CurrentCol + CurrentPiece.Width) > BoardContent.Width)
            {
                return false;
            }

            var newCol = CurrentCol + (CurrentPiece.Width - rotatedPiece.Width) / 2;

            if (CanMergePatternIntoBoardContent(CurrentRow, newCol, rotatedPiece.Pattern))
            {
                CurrentPiece = rotatedPiece;
                CurrentCol = newCol;
                return true;
            }

            return false;
        }

        public bool TryMove(Direction direction)
        {
            if (IsMovePossible(direction))
            {
                if (direction == Direction.Down)
                {
                    CurrentRow++;
                }
                else
                {
                    // Note type case for enum in the next line of code
                    CurrentCol += (int)direction;
                }

                return true;
            }

            return false;
        }

        // QUIZ: Why is the following method called `Try...`?
        // LEARN MORE at https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/exceptions-and-performance#try-parse-pattern
        public bool TryMergingPatternIntoBoardContent(int targetRow, int targetCol, bool[,] pattern)
        {
            if (!CanMergePatternIntoBoardContent(targetRow, targetCol, pattern))
            {
                return false; // Indicate error
            }

            // Set target pixcels
            foreach (var item in pattern.ToEnumerable().Where(item => item.val))
            {
                BoardContent[targetRow + item.row, targetCol + item.col] = item.val;
            }

            return true; // Indicate success
        }

        public void MergePatternIntoBoardContent(int targetRow, int targetCol, bool[,] pattern)
        {
            if (!TryMergingPatternIntoBoardContent(targetRow, targetCol, pattern))
            {
                throw new BoardException();
            }
        }

        public void MergeCurrentPieceIntoBoardContent() => 
            MergePatternIntoBoardContent(CurrentRow, CurrentCol, CurrentPiece.Pattern);
    }
}
