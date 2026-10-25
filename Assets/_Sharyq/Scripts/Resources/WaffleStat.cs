using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Sharyq
{
    public class WaffleStat : BaseResourcePickup
    {
        [SerializeField]
        private GameObject burntWaffleInst;

        private ResourceProducingStat _stat;

        public StoveCooking stove { set; get; }

        protected override void Awake()
        {
            base.Awake();
            _stat = GetComponentInChildren<ResourceProducingStat>();
        }

        private void Start()
        {
            _stat.StartProduction();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _stat.finishedCooking.AddListener(BurnWaffle);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _stat.finishedCooking.RemoveListener(BurnWaffle);
        }

        protected override void Collect(SelectEnterEventArgs args)
        {
            if (args.interactorObject is not NearFarInteractor)
                return;
            
            stove.ActivateStoveMovement();
            base.Collect(args);
        }

        private void BurnWaffle()
        {
            var obj= Instantiate(burntWaffleInst, transform.position, Quaternion.identity);
            obj.GetComponent<BaseResourcePickup>().onResourcePickup.AddListener(stove.ActivateStoveMovement);
            Destroy(gameObject);
        }
    }
}