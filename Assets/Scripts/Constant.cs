using UnityEngine;

namespace Game2048
{
	public static class Constant
	{
		public static string RESTART_BUTTON_LABEL = "Restart";
		public static class SCORE_PANEL
		{
			public static string LABEL = "Score:";
			public static int INITIAL_VALUE = 0;
		}
		public static class FIELD
		{
			public static int INITIAL_CUBE_COUNT = 2;
			public static int INITIAL_CUBE_VALUE = 2;
			public static class OFFSET
			{
				public static int LEFT = 5;
				public static int TOP = 5;
			}
			public static int GRID_WIDTH = 4;
			public static int CELL_SIZE = 88;
		}
		public static class KEYBINDING
		{
			public static string UP = "W";
			public static string RIGHT = "D";
			public static string DOWN = "S";
			public static string LEFT = "A";
		}
	}
}