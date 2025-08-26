namespace QuantumMobileInputTools
{
	using UnityEngine;
	public class QuantumMobileButtonsManager : MonoBehaviour
	{
		public bool ShowOnEditor = false;

		void Start()
		{
#pragma warning disable CS0162
#if UNITY_STANDALONE || UNITY_WEBGL
#if UNITY_EDITOR
			if (ShowOnEditor == false)
			{
				gameObject.SetActive(false);
			}
			return;
#endif
      gameObject.SetActive(false);
#endif
		}
#pragma warning restore CS0162
	}
}