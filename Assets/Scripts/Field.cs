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
	}

	public class Field : MonoBehaviour
	{
		public Transform cube;
		private static Transform _cube;
		private static Transform _fieldObjectTransform;
		private static Matrix4x4 _field;
		private static int _freeCellCount;

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
			List<MatrixCell> initialCubeCoordinates = GetRandomCubeCoordinates();
			DrawCubes(initialCubeCoordinates);
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
				result.Add(new MatrixCell(randomRow, randomColumn));
				--_freeCellCount;
				if (_freeCellCount == 0)
				{
					break;
				}
			}
			return result;
		}

		private static void DrawCubes(List<MatrixCell> initialCubeCoordinates)
		{
			for (int i = 0; i < initialCubeCoordinates.Count; ++i)
			{
				_field[initialCubeCoordinates[i].row, initialCubeCoordinates[i].column] = Constant.FIELD.INITIAL_CUBE_VALUE;
				Transform cubeObjectTransform = Instantiate(_cube, Vector3.zero, Quaternion.identity);
				cubeObjectTransform.gameObject.GetComponentInChildren<Text>().text = Constant.FIELD.INITIAL_CUBE_VALUE.ToString();
				cubeObjectTransform.SetParent(_fieldObjectTransform);
				Debug.Log(initialCubeCoordinates[i].column + " " + initialCubeCoordinates[i].row);
				cubeObjectTransform.gameObject.GetComponent<RectTransform>().anchoredPosition = 
					new Vector3(
						initialCubeCoordinates[i].column * (Constant.FIELD.CELL_SIZE + Constant.FIELD.GRID_WIDTH) + Constant.FIELD.OFFSET.LEFT,
						-(initialCubeCoordinates[i].row * (Constant.FIELD.CELL_SIZE + Constant.FIELD.GRID_WIDTH) + Constant.FIELD.OFFSET.TOP),
						0
					);
			}
		}
		void OnGUI()
		{
			if (Event.current.Equals(Event.KeyboardEvent(Constant.KEYBINDING.UP)))
			{
				Debug.Log("UP");
			}
			else if (Event.current.Equals(Event.KeyboardEvent(Constant.KEYBINDING.RIGHT)))
			{
				Debug.Log("RIGHT");
			}
			else if (Event.current.Equals(Event.KeyboardEvent(Constant.KEYBINDING.DOWN)))
			{
				Debug.Log("DOWN");
			}
			else if (Event.current.Equals(Event.KeyboardEvent(Constant.KEYBINDING.LEFT)))
			{
				Debug.Log("LEFT");
			}
		}
	}
}