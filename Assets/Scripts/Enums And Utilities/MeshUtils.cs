using System.Collections.Generic;

public static class MeshUtils {

    public struct Triangle {

        public int i0;
        public int i1;
        public int i2;

        public Triangle (int i0, int i1, int i2) {
            this.i0 = i0;
            this.i1 = i1;
            this.i2 = i2;
        }

        public Triangle reversed => new Triangle(i2, i1, i0);
        
    }

    public static int[] MakeNormalTriangleIndexArray (IEnumerable<Triangle> tris) {
        var output = new List<int>();
        foreach(var tri in tris){
            output.Add(tri.i0);
            output.Add(tri.i1);
            output.Add(tri.i2);
        }
        return output.ToArray();
    }
	
}
