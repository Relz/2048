using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game2048
{
	public struct MatrixCell
	{
		public int column;
		public int row;

		public MatrixCell(int column, int row)
		{
			this.column = column;
			this.row = row;
		}

		public static bool operator ==(MatrixCell lhs, MatrixCell rhs)
		{
			return lhs.column == rhs.column && lhs.row == rhs.row;
		}

		public static bool operator !=(MatrixCell lhs, MatrixCell rhs)
		{
			return lhs.column != rhs.column || lhs.row != rhs.row;
		}
	}

	public enum Direction
	{
		NONE, UP, RIGHT, DOWN, LEFT
	}

	public class Field : MonoBehaviour
	{
		public Transform cube;
		public ScorePanel scorePanel;
		private Transform _cube;
		private Transform _fieldObjectTransform;
		private Matrix4x4 _field;
		private int _freeCellCount;
		private List<MatrixCell> _cubesCoordinates;
		private List<GameObject> _cubes = new List<GameObject>();

		void Start()
		{
			_fieldObjectTransform = gameObject.transform;
			_cube = cube;
			Reset();
		}

		public void Reset()
		{
			Clear();
			_freeCellCount = 4 * 4;
			_cubesCoordinates = GetRandomCubeCoordinates(Constant.FIELD.INITIAL_CUBE_COUNT);
			_cubes.Clear();
			DrawCubes(_cubesCoordinates, Constant.FIELD.INITIAL_CUBE_VALUE);
		}

		private void Clear()
		{
			for (int col = 0; col < 4; ++col)
			{
				for (int row = 0; row < 4; ++row)
				{
					_field[row, col] = 0;
				}
			}
			foreach (Transform child in _fieldObjectTransform)
			{
				Destroy(child.gameObject);
			}
		}

		private List<MatrixCell> GetRandomCubeCoordinates(int cubeCount)
		{
			List<MatrixCell> result = new List<MatrixCell>();
			if (_freeCellCount == 0)
			{
				return result;
			}
			System.Random random = new System.Random();
			for (int i = 0; i < cubeCount; ++i)
			{
				int randomRow = -1;
				int randomColumn = -1;
				while (randomRow == -1 || randomColumn == -1 || _field[randomRow, randomColumn] != 0)
				{
					randomRow = random.Next(0, 4);
					randomColumn = random.Next(0, 4);
				}
				result.Add(new MatrixCell(randomColumn, randomRow));
				_field[randomRow, randomColumn] = Constant.FIELD.INITIAL_CUBE_VALUE;
				--_freeCellCount;
				if (_freeCellCount == 0)
				{
					break;
				}
			}
			return result;
		}

		private void DrawCubes(List<MatrixCell> cubesCoordinates, int value)
		{
			for (int i = 0; i < cubesCoordinates.Count; ++i)
			{
				DrawCube(cubesCoordinates[i], value);
			}
		}

		private void DrawCube(MatrixCell cubeCoordinates, int value)
		{
			Transform cubeObjectTransform = Instantiate(_cube, Vector3.zero, Quaternion.identity);
			_cubes.Add(cubeObjectTransform.gameObject);
			cubeObjectTransform.gameObject.GetComponentInChildren<Text>().text = value.ToString();
			cubeObjectTransform.SetParent(_fieldObjectTransform);
			cubeObjectTransform.gameObject.GetComponent<RectTransform>().anchoredPosition =
				new Vector3(
					cubeCoordinates.column * (Constant.FIELD.CELL_SIZE + Constant.FIELD.GRID_WIDTH) + Constant.FIELD.OFFSET.LEFT,
					-(cubeCoordinates.row * (Constant.FIELD.CELL_SIZE + Constant.FIELD.GRID_WIDTH) + Constant.FIELD.OFFSET.TOP),
					0
				);
			Color newColor = cubeObjectTransform.gameObject.GetComponent<Image>().color;
			float newColorValue = (float)(Math.Log(value, 2) - 1) * Constant.FIELD.CUBE.COLOR_CHANGE;
			newColor.r -= newColorValue / 255;
			newColor.b -= newColorValue / 255;
			cubeObjectTransform.gameObject.GetComponent<Image>().color = newColor;
		}

		void OnGUI()
		{
			Direction direction = Direction.NONE;
			if (Event.current.Equals(Event.KeyboardEvent(Constant.KEYBINDING.UP)))
			{
				direction = Direction.UP;
			}
			else if (Event.current.Equals(Event.KeyboardEvent(Constant.KEYBINDING.RIGHT)))
			{
				direction = Direction.RIGHT;
			}
			else if (Event.current.Equals(Event.KeyboardEvent(Constant.KEYBINDING.DOWN)))
			{
				direction = Direction.DOWN;
			}
			else if (Event.current.Equals(Event.KeyboardEvent(Constant.KEYBINDING.LEFT)))
			{
				direction = Direction.LEFT;
			}
			if (direction != Direction.NONE)
			{
				MakeStep(direction);
			}
		}

		private void MakeStep(Direction direction)
		{
			int startColumn = 0;
			int startRow = 0;
			int columnDirection = 1;
			int rowDirection = 1;
			bool doesProcessColumnFirst = true;

			if (direction == Direction.RIGHT)
			{
				startColumn = 3;
				startRow = 0;
				columnDirection = -1;
				rowDirection = 1;
				doesProcessColumnFirst = false;
			}
			else if (direction == Direction.DOWN)
			{
				startColumn = 0;
				startRow = 3;
				columnDirection = 1;
				rowDirection = -1;
			}
			else if (direction == Direction.LEFT)
			{
				startColumn = 0;
				startRow = 0;
				columnDirection = 1;
				rowDirection = 1;
				doesProcessColumnFirst = false;
			}

			bool doesStepSucceded = false;
			if (doesProcessColumnFirst)
			{
				for (int row = startRow; row < 4 && row > -1; row += rowDirection)
				{
					for (int column = startColumn; column < 4 && column > -1; column += columnDirection)
					{
						if (_field[row, column] != 0)
						{
							doesStepSucceded = VerticalPush(new MatrixCell(column, row), -rowDirection) || (doesStepSucceded);
						}
					}
				}
			}
			else
			{
				for (int column = startColumn; column < 4 && column > -1; column += columnDirection)
				{
					for (int row = startRow; row < 4 && row > -1; row += rowDirection)
					{
						if (_field[row, column] != 0)
						{
							doesStepSucceded = HorizontalPush(new MatrixCell(column, row), -columnDirection) || (doesStepSucceded);
						}
					}
				}
			}
			if (doesStepSucceded)
			{
				List<MatrixCell> newCubes = GetRandomCubeCoordinates(Constant.FIELD.NEXT_STEP_CUBE_COUNT);
				_cubesCoordinates.AddRange(newCubes);
				DrawCubes(newCubes, Constant.FIELD.INITIAL_CUBE_VALUE);
			}
		}

		private bool VerticalPush(MatrixCell matrixCell, int rowDirection)
		{
			int row = matrixCell.row;
			if (row + rowDirection < 4 && row + rowDirection > -1)
			{
				row += rowDirection;
				for (; row < 4 && row > -1; row += rowDirection)
				{
					if (_field[row, matrixCell.column] != 0)
					{
						if (_field[row, matrixCell.column] != _field[matrixCell.row, matrixCell.column])
						{
							row -= rowDirection;
							break;
						}
						DestroyCube(new MatrixCell(matrixCell.column, row));
						_field[row, matrixCell.column] *= 2;
						scorePanel.IncreaseScore((int)_field[row, matrixCell.column]);
						_field[matrixCell.row, matrixCell.column] = 0;
						DestroyCube(matrixCell);
						_cubesCoordinates.Add(new MatrixCell(matrixCell.column, row));
						DrawCube(_cubesCoordinates[_cubesCoordinates.Count - 1], (int)_field[row, matrixCell.column]);
						++_freeCellCount;
						return true;
					}
				}
			}
			if (row == 4 || row == -1)
			{
				row -= rowDirection;
			}
			if (row == matrixCell.row)
			{
				return false;
			}
			_field[row, matrixCell.column] = _field[matrixCell.row, matrixCell.column];
			_field[matrixCell.row, matrixCell.column] = 0;
			DestroyCube(matrixCell);
			_cubesCoordinates.Add(new MatrixCell(matrixCell.column, row));
			DrawCube(_cubesCoordinates[_cubesCoordinates.Count - 1], (int)_field[row, matrixCell.column]);
			return true;
		}

		private bool HorizontalPush(MatrixCell matrixCell, int columnDirection)
		{
			int column = matrixCell.column;
			if (column + columnDirection < 4 && column + columnDirection > -1)
			{
				column += columnDirection;
				for (; column < 4 && column > -1; column += columnDirection)
				{
					if (_field[matrixCell.row, column] != 0)
					{
						if (_field[matrixCell.row, column] != _field[matrixCell.row, matrixCell.column])
						{
							column -= columnDirection;
							break;
						}
						DestroyCube(new MatrixCell(column, matrixCell.row));
						_field[matrixCell.row, column] *= 2;
						scorePanel.IncreaseScore((int)_field[matrixCell.row, column]);
						_field[matrixCell.row, matrixCell.column] = 0;
						DestroyCube(matrixCell);
						_cubesCoordinates.Add(new MatrixCell(column, matrixCell.row));
						DrawCube(_cubesCoordinates[_cubesCoordinates.Count - 1], (int)_field[matrixCell.row, column]);
						++_freeCellCount;
						return true;
					}
				}
			}
			if (column == 4 || column == -1)
			{
				column -= columnDirection;
			}
			if (column == matrixCell.column)
			{
				return false;
			}
			_field[matrixCell.row, column] = _field[matrixCell.row, matrixCell.column];
			_field[matrixCell.row, matrixCell.column] = 0;
			DestroyCube(matrixCell);
			_cubesCoordinates.Add(new MatrixCell(column, matrixCell.row));
			DrawCube(_cubesCoordinates[_cubesCoordinates.Count - 1], (int)_field[matrixCell.row, column]);
			return true;
		}

		private void DestroyCube(MatrixCell matrixCell)
		{
			for (int i = 0; i < _cubesCoordinates.Count; ++i)
			{
				if (_cubesCoordinates[i] == matrixCell)
				{
					Destroy(_cubes[i]);
				}
			}
		}
	}
}