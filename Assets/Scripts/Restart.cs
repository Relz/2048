using UnityEngine;
using UnityEngine.UI;

namespace Game2048
{
	public class Restart : MonoBehaviour
	{
		private static Text _restartButtonText;
		void Start()
		{
			_restartButtonText = GetComponentInChildren<Text>();
			_restartButtonText.text = Constant.RESTART_BUTTON_LABEL;
			GetComponent<Button>().onClick.AddListener(RestartGame);
		}

		private static void RestartGame()
		{
			Field.Reset();
			ScorePanel.Reset();
			Audio.Reset();
		}
	}
}