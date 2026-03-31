using UnityEngine;
using TWD.Core;
using TWD.Utilities;

namespace TWD.Environment
{
    public class ExitZone : MonoBehaviour
    {
        [SerializeField] private string _nextSceneName;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Constants.Tags.PLAYER))
            {
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadScene(_nextSceneName);
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(_nextSceneName);
            }
        }
    }
}
