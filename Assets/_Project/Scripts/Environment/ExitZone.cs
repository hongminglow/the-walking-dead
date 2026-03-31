using UnityEngine;
using TWD.Core;
using TWD.Utilities;

namespace TWD.Environment
{
    public class ExitZone : MonoBehaviour
    {
        [SerializeField] private string _nextSceneName;

        private bool _triggered;

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            if (other.CompareTag(Constants.Tags.PLAYER))
            {
                _triggered = true;
                GameManager.Instance.CompleteLevel(_nextSceneName);
            }
        }
    }
}
