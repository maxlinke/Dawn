using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneLoading {

    public class SceneLoader : MonoBehaviour {

        public enum LoadMode {
            NO_LOADING_SCREEN,
            WITH_LOADING_SCREEN,
            WITH_LOADING_SCREEN_AND_MANUAL_CONTINUE
        }

        [SerializeField] SceneID initialScene = default;
        [SerializeField] LoadingScreen loadingScreen = default;

        private static SceneLoader instance;

        SceneID currentScene = SceneID.NONE;
        Coroutine loadVis = null;

        void Start () {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(SceneLoader)} is not null!");
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            Load(initialScene, LoadMode.NO_LOADING_SCREEN);     // get load mode from the same scriptable object that says what the next level is
            // maybe even make a separate struct for "level" or something that can contain multiple scenes? like, level, navigation, background?
        }

        public static void LoadScene (SceneID newScene, LoadMode loadMode) {
            instance.Load(newScene, loadMode);
        }

        public static void LoadNextScene (LoadMode loadMode) {
            instance.LoadNext(loadMode);
        }

        void Load (SceneID newScene, LoadMode loadMode) {
            List<AsyncOperation> loadOps = new List<AsyncOperation>();
            UnloadCurrentScene();
            LoadNewScene();
            if(loadOps.Count > 0){
                loadVis = StartCoroutine(LoadingVis());
            }
            // set new as active, preferrably as soon as it's loaded fully
            
            void UnloadCurrentScene () {
                // SceneManager.GetAllScenes    // < unload all of those except this one? how can i tell THIS one?
                if(currentScene == SceneID.NONE){
                    return;
                }
                loadOps.Add(SceneManager.UnloadSceneAsync((int)currentScene));
            }

            void LoadNewScene () {
                if(newScene == SceneID.NONE){
                    return;
                }
                loadOps.Add(SceneManager.LoadSceneAsync((int)newScene, LoadSceneMode.Additive));
            }

            IEnumerator LoadingVis () {
                loadingScreen.Show();
                var progress = 0f;
                while(progress < 1f){
                    float sum = 0f;
                    foreach(var loadOp in loadOps){
                        sum += loadOp.progress;
                    }
                    progress =  sum / loadOps.Count;    // TODO < i don't trust floating point numbers, use the booleans...
                    yield return null;
                }
                loadingScreen.Hide();

                // TODO maybe merge the loading screen into this?
                // TODO some way of letting the scene initialize itself before removing the loadingscreen
                // ^ static class with some kind of thingy. idk. 
            }
        }

        // probably use a scriptable object to know which is the "next" scene in line...
        void LoadNext (LoadMode loadMode) {

        }
        
    }
}