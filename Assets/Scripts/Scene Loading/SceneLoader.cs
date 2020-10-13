using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SceneLoading {

    public class SceneLoader : MonoBehaviour {

        public enum LoadMode {
            NoLoadingScreen,
            WithLoadingScreen,
            WithLoadingScreenAndManualContinue
        }

        [SerializeField] Canvas loadingScreenCanvas = default;
        [SerializeField] Image loadingBar = default;
        [SerializeField] Text[] loadingTexts = default;
        [SerializeField] Text continueText = default;

        private static SceneLoader instance;

        public static bool LoadingScreenVisible { 
            get {
                if(instance == null){
                    return false;
                }
                return instance.loadingScreenCanvas.enabled;
            }
        }
        
        Coroutine loadCoroutine = null;

        void Start () {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(SceneLoader)} is not null!");
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            loadingScreenCanvas.sortingOrder = (int)(CanvasSortingOrder.LOADING_SCREEN);
            loadingScreenCanvas.enabled = false;
        }

        public static void LoadScene (SceneID newScene, LoadMode loadMode) {
            instance.Load(newScene, loadMode);
        }

        void Load (SceneID newScene, LoadMode loadMode) {
            if(loadCoroutine != null){
                Debug.LogWarning($"Currently loading another scene, call to load \"{newScene}\" will be ignored!");
                return;
            }
            var loadOp = SceneManager.LoadSceneAsync((int)newScene, LoadSceneMode.Single);
            loadOp.allowSceneActivation = true;
            loadOp.completed += (op) => SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)newScene));
            if(loadMode != LoadMode.NoLoadingScreen){
                loadCoroutine = StartCoroutine(LoadingVis(loadOp, newScene, loadMode == LoadMode.WithLoadingScreenAndManualContinue));
            }
        }

        IEnumerator LoadingVis (AsyncOperation loadOp, SceneID newScene, bool manualContinue) {
            loadingScreenCanvas.enabled = true;
            continueText.gameObject.SetActive(false);
            while(!loadOp.isDone){
                float progress = loadOp.progress;
                loadingBar.fillAmount = progress;
                var loadingTextText = $"{(100f * progress):F0}%";
                foreach(var text in loadingTexts){
                    text.text = loadingTextText;
                }
                yield return null;   
            }
            if(manualContinue){
                continueText.gameObject.SetActive(true);
                while(!Input.anyKeyDown){
                    yield return null;
                }
            }
            loadingScreenCanvas.enabled = false;
            loadCoroutine = null;
        }
        
    }
}