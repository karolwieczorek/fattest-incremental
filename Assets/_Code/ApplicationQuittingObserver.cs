using System;
using UnityEngine;

namespace FattestInc {
    public static class ApplicationQuittingObserver {
        public static event Action GameQuitEvent;

        static void Quit()
        {
            Debug.Log("Quitting the Player");
            GameQuitEvent?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.quitting += Quit;
        }
    }
}