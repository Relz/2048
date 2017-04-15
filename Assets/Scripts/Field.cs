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
		private static Transform _cube;
		private static Transform _fieldObjectTransform;
		private static Matrix4x4 _field;
		private static int _freeCellCount;
		private static List<MatrixCell> _cubesCoordinates;
		private static List<GameObject> _cubes = new List<GameObject>();

		void Start()
		{
			_fieldObjectTransform = gameObject.transform;
			_cube = cube;
			_freeCellCount = 4 * 4;
			Reset();
		}

		public static void Reset()
		{
			Clear();
			_cubesCoordinates = GetRandomCubeCoordinates();
			_cubes.Clear();
			DrawCubes(_cubesCoordinates);
		}

		private static void Clear()
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

		private static List<MatrixCell> GetRandomCubeCoordinates()
		{
			List<MatrixCell> result = new List<MatrixCell>();
			System.Random random = new System.Random();
			for (int i = 0; i < Constant.FIELD.INITIAL_CUBE_COUNT; ++i)
			{
				int randomRow = -1;
				int randomColumn = -1;
				while (randomRow == -1 || randomColumn == -1 || _field[randomRow, randomColumn] != 0)
				{
					randomRow = random.Next(0, 4);
					randomColumn = random.Next(0, 4);
				}
				result.Add(new MatrixCell(randomColumn, randomRow));
				--_freeCellCount;
				if (_freeCellCount == 0)
				{
					break;
				}
				_field[randomRow, randomColumn] = Constant.FIELD.INITIAL_CUBE_VALUE;
			}
			return result;
		}

		private static void DrawCubes(List<MatrixCell> cubesCoordinates)
		{
			for (int i = 0; i < cubesCoordinates.Count; ++i)
			{
				DrawCube(cubesCoordinates[i]);
			}
		}

		private static void DrawCube(MatrixCell cubeCoordinates)
		{
			Transform cubeObjectTransform = Instantiate(_cube, Vector3.zero, Quaternion.identity);
			_cubes.Add(cubeObjectTransform.gameObject);
			cubeObjectTransform.gameObject.GetComponentInChildren<Text>().text = Constant.FIELD.INITIAL_CUBE_VALUE.ToString();
			cubeObjectTransform.SetParent(_fieldObjectTransform);
			cubeObjectTransform.gameObject.GetComponent<RectTransform>().anchoredPosition =
				new Vector3(
					cubeCoordinates.column * (Constant.FIELD.CELL_SIZE + Constant.FIELD.GRID_WIDTH) + Constant.FIELD.OFFSET.LEFT,
					-(cubeCoordinates.row * (Constant.FIELD.CELL_SIZE + Constant.FIELD.GRID_WIDTH) + Constant.FIELD.OFFSET.TOP),
					0
				);
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

		private static void MakeStep(Direction direction)
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

			if (doesProcessColumnFirst)
			{
				for (int row = startRow; row < 4 && row > -1; row += rowDirection)
				{
					for (int column = startColumn; column < 4 && column > -1; column += columnDirection)
					{
						if (_field[row, column] != 0)
						{
							VerticalPush(new MatrixCell(column, row), -rowDirection);
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
							HorizontalPush(new MatrixCell(column, row), -columnDirection);
						}
					}
				}
			}
		}

		private static void VerticalPush(MatrixCell matrixCell, int rowDirection)
		{
			int row = matrixCell.row;
			if (row + rowDirection < 4 && row + rowDirection > -1)
			{
				row += rowDirection;
				for (; row < 4 && row > -1; row += rowDirection)
				{
					if (_field[row, matrixCell.column] != 0)
					{
						break;
					}
				}
				row -= rowDirection;
			}
			if (row == matrixCell.row)
			{
				return;
			}
			_field[row, matrixCell.column] = _field[matrixCell.row, matrixCell.column];
			_field[matrixCell.row, matrixCell.column] = 0;
			_cubesCoordinates.Add(new MatrixCell(matrixCell.column, row));
			DrawCube(_cubesCoordinates[_cubesCoordinates.Count - 1]);
			DestroyCube(matrixCell);
		}

		private static void HorizontalPush(MatrixCell matrixCell, int columnDirection)
		{
			int column = matrixCell.column;
			if (column + columnDirection < 4 && column + columnDirection > -1)
			{
				column += columnDirection;
				for (; column < 4 && column > -1; column += columnDirection)
				{
					if (_field[matrixCell.row, column] != 0)
					{
						break;
					}
				}
				column -= columnDirection;
			}
			if (column == matrixCell.column)
			{
				return;
			}
			_field[matrixCell.row, column] = _field[matrixCell.row, matrixCell.column];
			_field[matrixCell.row, matrixCell.column] = 0;
			_cubesCoordinates.Add(new MatrixCell(column, matrixCell.row));
			DrawCube(_cubesCoordinates[_cubesCoordinates.Count - 1]);
			DestroyCube(matrixCell);
		}

		private static void DestroyCube(MatrixCell matrixCell)
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