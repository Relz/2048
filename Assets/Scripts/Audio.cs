using UnityEngine;

namespace Game2048
{
	public class Audio : MonoBehaviour
	{
		private static AudioSource _sourse;
		void Start()
		{
			_sourse = GetComponent<AudioSource>();
		}
		public static void Reset()
		{

		}

		public static void PlayOneShot(AudioClip clip)
		{
			_sourse.PlayOneShot(clip);
		}

	}
}
