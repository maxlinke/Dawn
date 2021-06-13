using UnityEngine;
using UnityEngine.Rendering;

public static class MeshExtensions {

    public static void CopyDataFrom (this Mesh mesh, Mesh other) {
        mesh.Clear(false);
        mesh.indexFormat = other.indexFormat;
        mesh.vertices = other.vertices;
        mesh.name = other.name;

        mesh.bindposes = other.bindposes;
        mesh.boneWeights = other.boneWeights;
        mesh.colors = other.colors;
        mesh.hideFlags = other.hideFlags;
        mesh.normals = other.normals;
        mesh.subMeshCount = other.subMeshCount;
        mesh.tangents = other.tangents;
        mesh.uv = other.uv;
        mesh.uv2 = other.uv2;
        mesh.uv3 = other.uv3;
        mesh.uv4 = other.uv4;
        mesh.uv5 = other.uv5;
        mesh.uv6 = other.uv6;
        mesh.uv7 = other.uv7;
        mesh.uv8 = other.uv8;

        mesh.triangles = other.triangles;
        mesh.bounds = other.bounds;

        if(other.subMeshCount > 1){
            mesh.subMeshCount = other.subMeshCount;
            for(int i=0; i<other.subMeshCount; i++){
                var sm = other.GetSubMesh(i);
                var smd = new SubMeshDescriptor(
                    indexStart: sm.indexStart,
                    indexCount: sm.indexCount,
                    topology: sm.topology
                );
                mesh.SetSubMesh(i, smd, MeshUpdateFlags.Default);
            }
        }
    }
	
}
