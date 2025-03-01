using System.Collections;
using UnityEngine;

namespace Hypnagogia.Utils
{
    public class StaticCoroutine
    {
        static StaticCoroutineRunner runner;

        public static Coroutine Start(IEnumerator coroutine)
        {
            EnsureRunner();
            return runner.StartCoroutine(coroutine);
        }

        static void EnsureRunner()
        {
            if (runner == null)
            {
                runner = new GameObject("[Static Coroutine Runner]").AddComponent<StaticCoroutineRunner>();
                Object.DontDestroyOnLoad(runner.gameObject);
            }
        }

        class StaticCoroutineRunner : MonoBehaviour { }
    }
}