using UnityEngine;
using UnityEngine.UI;

namespace Game2048
{
	public class Restart : MonoBehaviour
	{
		public Field field;
		public ScorePanel scorePanel;
		private Text _restartButtonText;
		void Start()
		{
			_restartButtonText = GetComponentInChildren<Text>();
			_restartButtonText.text = Constant.RESTART_BUTTON_LABEL;
			GetComponent<Button>().onClick.AddListener(RestartGame);
		}

		private void RestartGame()
		{
			field.Reset();
			scorePanel.Reset();
		}
	}
}