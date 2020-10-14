using UnityEngine;

namespace SceneLoading {

    public static class SceneBuildIndices {

        // works because build indices start at 0 and increase one by one
        // also, the line number - 10 is equal to the build index...
        private static SceneID[] ids = new SceneID[]{
            SceneID.MovementTesting,
            SceneID.Test1,
            SceneID.TerrainTest
        };

        public static int GetBuildIndex (SceneID id) {
            for(int i=0; i<ids.Length; i++){
                if(ids[i] == id){
                    return i;
                }
            }
            Debug.LogError($"Couldn't find build index for {nameof(SceneID)} \"{id}\"!");
            return -1;
        }

        public static SceneID GetID (int index) {
            try{
                return ids[index];
            }catch(System.Exception e){
                Debug.LogError(e.Message);
                return (SceneID)(-1);
            }
        }
        
    }

}