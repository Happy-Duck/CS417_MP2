using System;
using UnityEngine;

namespace Sharyq
{
    public interface IAchievementUnlockable
    {
        public void Unlock(string achievementName, string achievementDescription);
    }
}