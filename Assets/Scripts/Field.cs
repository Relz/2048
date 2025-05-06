using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

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

	public class Field : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		public Transform cube;
		public ScorePanel scorePanel;
		private Transform _cube;
		private Transform _fieldObjectTransform;
		private Matrix4x4 _field;
		private int _freeCellCount;
		private List<MatrixCell> _cubesCoordinates;
		private List<GameObject> _cubes = new List<GameObject>();
		private Dictionary<MatrixCell, GameObject> _cubeObjects = new Dictionary<MatrixCell, GameObject>();
		private bool _isAnimating = false;
		private int _activeAnimations = 0;

		void Start()
		{
			_fieldObjectTransform = gameObject.transform;
			_cube = cube;
			Reset();
		}

		public void Reset()
		{
			Clear();
			_field = new Matrix4x4();
			_freeCellCount = 4 * 4;
			_cubesCoordinates = GetRandomCubeCoordinates(Constant.FIELD.INITIAL_CUBE_COUNT);
			DrawCubes(_cubesCoordinates, Constant.FIELD.INITIAL_CUBE_VALUE);
			scorePanel.Reset();
			_isAnimating = false;
			_activeAnimations = 0;
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
			_cubes.Clear();
			_cubeObjects.Clear();
			_isAnimating = false;
			_activeAnimations = 0;
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
				DrawCube(cubesCoordinates[i], value, false);
			}
		}

		private void DrawCube(MatrixCell cubeCoordinates, int value, bool animateSpawn = true)
		{
			Vector2 fieldSize = _fieldObjectTransform.GetComponent<RectTransform>().sizeDelta;
			Vector2 cellSize = new(fieldSize.x * 0.2241143617f, fieldSize.y * 0.2241143617f);
			float separatorThickness = (fieldSize.x - cellSize.x * 4) / 5;
			Transform cubeObjectTransform = Instantiate(_cube, Vector3.zero, Quaternion.identity);
			GameObject cubeGO = cubeObjectTransform.gameObject;
			cubeObjectTransform.GetComponent<RectTransform>().sizeDelta = cellSize;
			_cubeObjects[cubeCoordinates] = cubeGO;
			cubeGO.GetComponentInChildren<Text>().text = value.ToString();
			cubeObjectTransform.SetParent(_fieldObjectTransform);
			Vector3 targetPosition = new Vector3(
				cubeCoordinates.column * (cellSize.x + separatorThickness) + separatorThickness,
				-(cubeCoordinates.row * (cellSize.y + separatorThickness) + separatorThickness),
				0
			);
			cubeObjectTransform.gameObject.GetComponent<RectTransform>().anchoredPosition = targetPosition;

			Color newColor = cubeGO.GetComponent<Image>().color;
			float newColorValue = (float)(Math.Log(value, 2) - 1) * Constant.FIELD.CUBE.COLOR_CHANGE;
			newColor.r -= newColorValue / 255;
			newColor.b -= newColorValue / 255;
			cubeGO.GetComponent<Image>().color = newColor;

			if (animateSpawn)
			{
				cubeObjectTransform.GetComponent<RectTransform>().localScale = Vector3.zero;
				StartCoroutine(AnimateSpawn(cubeGO.transform));
			}
			else
			{
				cubeObjectTransform.GetComponent<RectTransform>().localScale = Vector3.one;
			}
			_field[cubeCoordinates.row, cubeCoordinates.column] = value;
		}

		private IEnumerator AnimateSpawn(Transform cubeTransform)
		{
			_activeAnimations++;
			float timer = 0f;
			Vector3 startScale = Vector3.zero;
			Vector3 endScale = Vector3.one;
			while (timer < Constant.FIELD.ANIMATION_DURATION / 2)
			{
				cubeTransform.localScale = Vector3.Lerp(startScale, endScale, timer / (Constant.FIELD.ANIMATION_DURATION / 2));
				timer += Time.deltaTime;
				yield return null;
			}
			cubeTransform.localScale = endScale;
			_activeAnimations--;
		}

		private IEnumerator AnimateCubeMove(GameObject cubeToAnimate, Vector3 targetScreenPosition, 
		                                   bool isMovingCubeInMerge, // True if this is the cube that moves and then is destroyed
		                                   GameObject stationaryCubeInMergeIfAny, // The cube that is merged into (null if not a merge)
		                                   int finalValueForStationaryCube) // Value for the stationary cube after merge
		{
			_activeAnimations++;
			RectTransform rectTransform = cubeToAnimate.GetComponent<RectTransform>();
			Vector3 startPosition = rectTransform.anchoredPosition;
			float timer = 0f;

			while (timer < Constant.FIELD.ANIMATION_DURATION)
			{
				rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetScreenPosition, timer / Constant.FIELD.ANIMATION_DURATION);
				timer += Time.deltaTime;
				yield return null;
			}
			rectTransform.anchoredPosition = targetScreenPosition;

			if (isMovingCubeInMerge)
			{
				Destroy(cubeToAnimate); // Moving cube is destroyed
				if (stationaryCubeInMergeIfAny != null)
				{
					// Update the stationary cube that was merged into
					stationaryCubeInMergeIfAny.GetComponentInChildren<Text>().text = finalValueForStationaryCube.ToString();
					UpdateCubeColor(stationaryCubeInMergeIfAny, finalValueForStationaryCube);
					scorePanel.IncreaseScore(finalValueForStationaryCube); // Score for the combined value
				}
			}
			// If it just moved to an empty spot (isMovingCubeInMerge = false), its appearance doesn't change here.
			// Its position in _cubeObjects and _field is already updated before this coroutine was called.

			_activeAnimations--;
		}

		private void UpdateCubeColor(GameObject cubeGO, int value)
		{
			Image image = cubeGO.GetComponent<Image>();
			// Assuming the Prefab's Image component has a base color (e.g., light grey or white)
			// that is then tinted. For this example, let's assume Color.white is the starting point for tinting calculations.
			// If your prefab has a different base color, you might need to adjust this.
			Color baseColor = Color.white; // Or load from prefab if necessary

			if (value == 0) // Should not happen if cubeGO exists and has a value
			{
				// Handle case of empty cell if needed, though typically we destroy cube GameObjects
				image.color = new Color(0,0,0,0); // Transparent
				return;
			}

			// Color changes based on log2(value). Example: 2=1, 4=2, 8=3, etc.
			// We subtract 1 because initial value 2 (log2(2)=1) might be the base look.
			float colorFactor = (float)(Math.Log(value, 2) -1);
			if (colorFactor < 0) colorFactor = 0; // for value 2, factor is 0.

			// This is a simplified coloring scheme. You'll likely want something more sophisticated.
			// The original code modified R and B. Let's try to map value to a gradient.
			// Example: Higher values get more reddish/less blueish.
			// This specific color logic from the original might need to be preserved or improved.
			float R = baseColor.r;
			float G = baseColor.g;
			float B = baseColor.b;

			// Original logic: R and B decrease with higher values based on CUBE.COLOR_CHANGE
			float change = colorFactor * Constant.FIELD.CUBE.COLOR_CHANGE / 255f;
			R -= change;
			B -= change;
			// G was unchanged in original snippet for DrawCube

			image.color = new Color(Mathf.Clamp01(R), Mathf.Clamp01(G), Mathf.Clamp01(B), baseColor.a);
		}

		private Vector3 GetScreenPosition(MatrixCell cell)
		{
			Vector2 fieldSize = _fieldObjectTransform.GetComponent<RectTransform>().sizeDelta;
			Vector2 cellSize = new(fieldSize.x * 0.2241143617f, fieldSize.y * 0.2241143617f);
			float separatorThickness = (fieldSize.x - cellSize.x * 4) / 5;
			return new Vector3(
				cell.column * (cellSize.x + separatorThickness) + separatorThickness,
				-(cell.row * (cellSize.y + separatorThickness) + separatorThickness),
				0
			);
		}

		void OnGUI()
		{
			if (_isAnimating || _activeAnimations > 0) return;

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
			if (_isAnimating || _activeAnimations > 0) return;
			_isAnimating = true;

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
				StartCoroutine(DelayedSpawnNewCubes());
			}
			else
			{
				_isAnimating = false;
			}
		}

		private IEnumerator DelayedSpawnNewCubes()
		{
			while (_activeAnimations > 0)
			{
				yield return null;
			}

			List<MatrixCell> newCubesCoordinates = GetRandomCubeCoordinates(Constant.FIELD.NEXT_STEP_CUBE_COUNT);
			DrawCubes(newCubesCoordinates, Constant.FIELD.INITIAL_CUBE_VALUE);

			yield return null;
			while (_activeAnimations > 0)
			{
				yield return null;
			}

			if (_freeCellCount == 0 && !CanMakeAnyMove())
			{
				Debug.Log("Game Over!");
			}
			_isAnimating = false;
		}

		private bool CanMakeAnyMove()
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (_field[i, j] == _field[i, j + 1])
					{
						return true;
					}
				}
			}

			for (int j = 0; j < 4; j++)
			{
				for (int i = 0; i < 3; i++)
				{
					if (_field[i, j] == _field[i + 1, j])
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool VerticalPush(MatrixCell matrixCell, int rowDirection)
		{
			bool hasMoved = false;
			int initialRow = matrixCell.row;
			int column = matrixCell.column;
			int currentValue = (int)_field[initialRow, column];

			if (currentValue == 0) return false; // Should not happen if called on a cell with a cube

			GameObject movingCube = _cubeObjects[matrixCell];
			MatrixCell oldCell = matrixCell;

			// Find the furthest possible position or merge target
			int targetRow = initialRow;
			for (int r = initialRow + rowDirection; r >= 0 && r < 4; r += rowDirection)
			{
				if (_field[r, column] == 0) // Empty cell
				{
					targetRow = r;
				}
				else if (_field[r, column] == currentValue) // Mergeable cell
				{
					targetRow = r;
					break; 
				}
				else // Blocked by another cube
				{
					break;
				}
			}

			if (targetRow != initialRow)
			{
				hasMoved = true;
				MatrixCell newCell = new MatrixCell(column, targetRow);
				Vector3 targetScreenPos = GetScreenPosition(newCell);

				_field[initialRow, column] = 0; // Vacate old cell
				_cubeObjects.Remove(oldCell);

				if (_field[targetRow, column] == 0) // Moving to an empty cell
				{
					_field[targetRow, column] = currentValue;
					_cubeObjects[newCell] = movingCube;
					StartCoroutine(AnimateCubeMove(movingCube, targetScreenPos, false, null, 0));
				}
				else // Merging with another cube (_field[targetRow, column] == currentValue)
				{
					int mergedValue = currentValue * 2;
					_field[targetRow, column] = mergedValue;
					_freeCellCount++; // The cell the moving cube came from is now free

					GameObject stationaryCube = _cubeObjects[newCell]; // The cube being merged into
					StartCoroutine(AnimateCubeMove(movingCube, targetScreenPos, true, stationaryCube, mergedValue));
				}
			}
			return hasMoved;
		}

		private bool HorizontalPush(MatrixCell matrixCell, int columnDirection)
		{
			bool hasMoved = false;
			int row = matrixCell.row;
			int initialColumn = matrixCell.column;
			int currentValue = (int)_field[row, initialColumn];

			if (currentValue == 0) return false;

			GameObject movingCube = _cubeObjects[matrixCell];
			MatrixCell oldCell = matrixCell;

			// Find the furthest possible position or merge target
			int targetColumn = initialColumn;
			for (int c = initialColumn + columnDirection; c >= 0 && c < 4; c += columnDirection)
			{
				if (_field[row, c] == 0) // Empty cell
				{
					targetColumn = c;
				}
				else if (_field[row, c] == currentValue) // Mergeable cell
				{
					targetColumn = c;
					break;
				}
				else // Blocked
				{
					break;
				}
			}

			if (targetColumn != initialColumn)
			{
				hasMoved = true;
				MatrixCell newCell = new MatrixCell(targetColumn, row);
				Vector3 targetScreenPos = GetScreenPosition(newCell);

				_field[row, initialColumn] = 0; // Vacate old cell
				_cubeObjects.Remove(oldCell);

				if (_field[row, targetColumn] == 0) // Moving to an empty cell
				{
					_field[row, targetColumn] = currentValue;
					_cubeObjects[newCell] = movingCube;
					StartCoroutine(AnimateCubeMove(movingCube, targetScreenPos, false, null, 0));
				}
				else // Merging with another cube (_field[row, targetColumn] == currentValue)
				{
					int mergedValue = currentValue * 2;
					_field[row, targetColumn] = mergedValue;
					_freeCellCount++;

					GameObject stationaryCube = _cubeObjects[newCell]; // The cube being merged into
					StartCoroutine(AnimateCubeMove(movingCube, targetScreenPos, true, stationaryCube, mergedValue));
				}
			}
			return hasMoved;
		}

		private void DestroyCube(MatrixCell matrixCell)
		{
			if (_cubeObjects.TryGetValue(matrixCell, out GameObject cubeToDestroy))
			{
				Destroy(cubeToDestroy);
				_cubeObjects.Remove(matrixCell);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			Vector2 delta = eventData.position - eventData.pressPosition;

			if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
			{
				if (delta.x < 0)
				{
					MakeStep(Direction.LEFT);
				}
				else
				{
					MakeStep(Direction.RIGHT);
				}
			}
			else
			{
				if (delta.y < 0)
				{
					MakeStep(Direction.DOWN);
				}
				else
				{
					MakeStep(Direction.UP);
				}
			}
		}
	}
}