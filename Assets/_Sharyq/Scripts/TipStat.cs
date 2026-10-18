using System;
using Shared;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Sharyq
{
    public class TipStat : BaseResourcePickup
    {
        [SerializeField]
        private int tipAmount;

        public float TipAmount
        {
            get => tipAmount;
            set
            {
                if (value <= 0) tipAmount = 1;
                else tipAmount = (int) value;
            } 
        }

        protected override void Collect(SelectEnterEventArgs args)
        {
            if (args.interactorObject is not NearFarInteractor)
                return;
            
            ResourceManager.Instance.Add(resource, tipAmount);
            onResourcePickup.Invoke();
            Destroy(gameObject);
        }
    }
}

