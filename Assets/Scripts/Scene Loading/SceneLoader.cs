using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SceneLoading {

    public class SceneLoader : MonoBehaviour {

        public enum LoadMode {
            NO_LOADING_SCREEN,
            WITH_LOADING_SCREEN,
            WITH_LOADING_SCREEN_AND_MANUAL_CONTINUE
        }

        [SerializeField] SceneID initialScene = default;
        [SerializeField] Canvas loadingScreenCanvas = default;
        [SerializeField] Image loadingBar = default;
        [SerializeField] Text[] loadingTexts = default;

        private static SceneLoader instance;
        
        Coroutine loadCoroutine = null;

        void Start () {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(SceneLoader)} is not null!");
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            loadingScreenCanvas.sortingOrder = (int)(CanvasSortingOrder.LOADING_SCREEN);
            Load(initialScene, LoadMode.NO_LOADING_SCREEN);
        }

        public static void LoadScene (SceneID newScene, LoadMode loadMode) {
            instance.Load(newScene, loadMode);
        }

        public static void LoadNextScene (LoadMode loadMode) {
            instance.LoadNext(loadMode);
        }

        // important: the loads are only "done" when the awakes are also done. so i can do my inits without having to do anything here, i think...
        void Load (SceneID newScene, LoadMode loadMode) {
            if(loadCoroutine != null){
                Debug.LogWarning($"Currently loading another scene, call to load \"{newScene}\" will be ignored!");
                return;
            }
            List<AsyncOperation> loadOps = new List<AsyncOperation>();
            UnloadCurrentScenes();
            LoadNewScene();
            if(loadOps.Count > 0){
                loadCoroutine = StartCoroutine(LoadingVis());
            }
            
            void UnloadCurrentScenes () {
                for(int i=0; i<SceneManager.sceneCount; i++){
                    var scene = SceneManager.GetSceneAt(i);
                    if(scene == this.gameObject.scene){
                        continue;
                    }
                    loadOps.Add(SceneManager.UnloadSceneAsync(scene));
                }
            }

            void LoadNewScene () {
                if(newScene == SceneID.NONE){
                    return;
                }
                loadOps.Add(SceneManager.LoadSceneAsync((int)newScene, LoadSceneMode.Additive));
            }

            IEnumerator LoadingVis () {
                loadingScreenCanvas.enabled = true;
                bool doneLoading = false;
                while(!doneLoading){
                    float progressSum = 0f;
                    doneLoading = true;
                    foreach(var loadOp in loadOps){
                        progressSum += loadOp.progress;
                        doneLoading &= loadOp.isDone;
                    }
                    var progress =  progressSum / loadOps.Count;
                    loadingBar.fillAmount = progress;
                    var loadingTextText = $"{(100f * progress):F0}%";
                    foreach(var text in loadingTexts){
                        text.text = loadingTextText;
                    }
                    yield return null;
                }
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)newScene));
                loadingScreenCanvas.enabled = false;
                loadCoroutine = null;
            }
        }

        // probably use a scriptable object to know which is the "next" scene in line...
        void LoadNext (LoadMode loadMode) {

        }
        
    }
}