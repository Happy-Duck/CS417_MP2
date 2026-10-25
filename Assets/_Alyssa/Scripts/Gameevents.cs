using System;

namespace Alyssa
{
    // Tiny event bus so Milestones can count things without touching every script.
    public static class GameEvents
    {
        public static event Action<string> Harvested;   // resource name of the fruit (e.g. "Strawberry")
        public static event Action Fertilized;

        public static void RaiseHarvested(string resource) { if (Harvested != null) Harvested(resource); }
        public static void RaiseFertilized() { if (Fertilized != null) Fertilized(); }
    }
}