using UnityEngine;
using System.Collections.Generic;

namespace Game2048
{
	public class Constant
	{
		public readonly static string RESTART_BUTTON_LABEL = "Restart";
		public class SCORE_PANEL
		{
			public readonly static string LABEL = "Score:";
			public readonly static int INITIAL_VALUE = 0;
		}
		public class FIELD
		{
			public readonly static int INITIAL_CUBE_COUNT = 2;
			public readonly static int INITIAL_CUBE_VALUE = 2;
			public readonly static int NEXT_STEP_CUBE_COUNT = 1;
			public static class OFFSET
			{
				public readonly static int LEFT = 5;
				public readonly static int TOP = 5;
			}
			public readonly static int GRID_WIDTH = 4;
			public readonly static int CELL_SIZE = 88;

			public class CUBE
			{
				public readonly static int COLOR_CHANGE = 20;
			}
		}
		public class KEYBINDING
		{
			public readonly static string UP = "W";
			public readonly static string RIGHT = "D";
			public readonly static string DOWN = "S";
			public readonly static string LEFT = "A";
		}
	}
}