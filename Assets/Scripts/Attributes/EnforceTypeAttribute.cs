using UnityEngine;

public class EnforceTypeAttribute : PropertyAttribute {

    // TODO maybe this can be unneccessary
    // if i can make a serializable interface class
    // that has a reference to an object, its own custompropertydrawer that does stuff
    // and a public T getvalue method that just casts the serialized object as the T

    public readonly System.Type targetType;
    public readonly bool redIfNull;

    public EnforceTypeAttribute (System.Type type, bool redIfNull = false) {
        this.targetType = type;
        this.redIfNull = redIfNull;
    }
    
}
