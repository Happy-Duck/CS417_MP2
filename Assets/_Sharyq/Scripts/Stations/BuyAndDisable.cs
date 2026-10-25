using UnityEngine;

namespace Sharyq
{
    public class BuyAndDisable : MonoBehaviour
    {
        [SerializeField] private GameObject _gameObject;

        public void DisableOnBuy()
        {
            gameObject.SetActive(false);
        }
    }
}