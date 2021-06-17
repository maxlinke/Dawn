using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SceneLoading {

    public class SceneLoader : MonoBehaviour {

        // TODO take the loading screen out of this
        // and i'm not sure what THIS will be useful for in the end
        // obviously showing and hiding the loading screen i guess
        // and the scene enum can go as well
        // because you can only load scenes that are in the build order
        // and i can iterate over those and get the names and such
        // so... no need for an enum that i need to maintain
        // for "next scene" things i can just make a scriptable object ..
        // .. with an #if UNITY_EDITOR array scenes and a duplicate ..
        // .. background array of either the names of build indices ..
        // .. that get synced via the thing's custom editor. just make ..
        // .. sure that when a build is made, some postprocessor call ..
        // .. or something exists to update that background array again

        [SerializeField, RedIfEmpty] Canvas m_canvas = default;
        [SerializeField, RedIfEmpty] Image  m_loadingBar = default;
        [SerializeField, RedIfEmpty] Image  m_spinner = default;
        [SerializeField, RedIfEmpty] Text[] m_loadingTexts = default;
        [SerializeField, RedIfEmpty] Text   m_continueText = default;

        private static SceneLoader instance;

        public static bool LoadingScreenVisible { 
            get {
                if(instance == null){
                    return false;
                }
                return instance.m_canvas.enabled;
            }
        }

        public static SceneID CurrentScene {
            get {
                if(instance == null){
                    return (SceneID)(-1);
                }
                return SceneBuildIndices.GetID(SceneManager.GetActiveScene().buildIndex);
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
            m_canvas.sortingOrder = CanvasSortingOrder.LoadingScreen;
            m_canvas.enabled = false;
        }

        public static void LoadScene (SceneID newScene, LoadMode loadMode) {
            instance.Load(newScene, loadMode);
        }

        void Load (SceneID newScene, LoadMode loadMode) {
            if(loadCoroutine != null){
                Debug.LogWarning($"Currently loading another scene, call to load \"{newScene}\" will be ignored!");
                return;
            }
            int sceneIndex = SceneBuildIndices.GetBuildIndex(newScene);
            var loadOp = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
            loadOp.allowSceneActivation = true;
            loadOp.completed += (op) => SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIndex));
            if(loadMode != LoadMode.NoLoadingScreen){
                loadCoroutine = StartCoroutine(LoadingVis(loadOp, loadMode == LoadMode.WithLoadingScreenAndManualContinue));
            }
        }

        IEnumerator LoadingVis (AsyncOperation loadOp, bool manualContinue) {
            m_canvas.enabled = true;
            m_continueText.gameObject.SetActive(false);
            m_spinner.gameObject.SetActive(true);
            var spinnerT = 0f;
            while(!loadOp.isDone){
                float progress = loadOp.progress;
                m_loadingBar.fillAmount = progress;
                SetLoadingTextProgress(progress);
                SetSpinnerT(spinnerT);
                spinnerT += Time.unscaledDeltaTime;
                yield return null;   
            }
            if(manualContinue){
                // TODO pause game (time only)
                m_loadingBar.fillAmount = 1;
                SetLoadingTextProgress(1);
                m_continueText.gameObject.SetActive(true);
                m_spinner.gameObject.SetActive(false);
                while(!Input.anyKeyDown){
                    yield return null;
                }
                // TODO unpause game
            }
            m_canvas.enabled = false;
            loadCoroutine = null;
        }

        void SetLoadingTextProgress (float progress) {
            var loadingTextText = $"{(100f * progress):F0}%";
            foreach(var text in m_loadingTexts){
                text.text = loadingTextText;
            }
        }

        void SetSpinnerT (float t) {
            m_spinner.fillAmount = Mathf.PingPong(t, 1f);
            m_spinner.transform.localScale = new Vector3(
                x: ((Mathf.Repeat(0.5f * t, 1) < 0.5f) ? 1f : -1f), 
                y: 1f, 
                z: 1f
            );
        }
        
    }
}