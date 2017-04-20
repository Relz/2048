using UnityEngine;
using UnityEngine.UI;

namespace Game2048
{
	public class ScorePanel : MonoBehaviour
	{
		public GameObject scoreLabel;
		public GameObject scoreValue;
		private Text _scoreLabelText;
		private Text _scoreValueText;
		private int _score;

		void Start()
		{
			_scoreLabelText = scoreLabel.GetComponent<Text>();
			_scoreValueText = scoreValue.GetComponent<Text>();
			_scoreLabelText.text = Constant.SCORE_PANEL.LABEL;
			Reset();
		}

		public void Reset()
		{
			_score = Constant.SCORE_PANEL.INITIAL_VALUE;
			_scoreValueText.text = _score.ToString();
		}

		public void IncreaseScore(int value)
		{
			_score += value;
			_scoreValueText.text = _score.ToString();
		}
	}
}
