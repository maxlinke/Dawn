using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollisionFlagsExtensions {

    public static string ToStringImproved (this CollisionFlags flags) {
        if(flags == CollisionFlags.None){
            return CollisionFlags.None.ToString();
        }
        string output = string.Empty;
        DoComparison(CollisionFlags.Above);
        DoComparison(CollisionFlags.Sides);
        DoComparison(CollisionFlags.Below);
        if(output.Length > 0){
            output = output.Remove(output.Length-2);
        }
        return output;

        void DoComparison (CollisionFlags compare) {
            if((flags & compare) != 0){
                output += $"{compare}, ";
            }
        }
    }
	
}
