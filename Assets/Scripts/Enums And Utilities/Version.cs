using System;
using System.Collections.Generic;
using UnityEngine;

public class Version : IComparable {

    public static bool operator <(Version a, Version b) => a.CompareTo(b) < 0;
    public static bool operator >(Version a, Version b) => a.CompareTo(b) > 0;
    public static bool operator ==(Version a, Version b) => a.CompareTo(b) == 0;
    public static bool operator !=(Version a, Version b) => a.CompareTo(b) != 0;

    public static implicit operator Version (string s) => new Version(s);

    private List<int> numbers;

    public Version (string version) {
        if(string.IsNullOrEmpty(version)){
            throw new System.Exception("Null or empty version strings aren't valid!");
        }
        if(!IsNumeral(version[0])){
            throw new System.Exception("Version string must start with a number!");
        }
        this.numbers = new List<int>();
        int numStart = 0;
        for(int i=1; i<=version.Length; i++){
            bool endReached = (i == version.Length);
            char c = endReached ? '.' : version[i];
            if(!IsNumeral(c)){
                if(c == '.'){
                    var numString = version.Substring(numStart, i-numStart);
                    ParseAndAdd(numString);
                    numStart = i+1;
                }else{
                    throw new System.Exception($"Illegal character \'{c}\' ({(int)c}) detected! Only numbers and \'.\' allowed!");
                }
            }
        }

        bool IsNumeral (char inputChar) {
            return (inputChar >= '0' && inputChar <= '9');
        }

        void ParseAndAdd (string number) {
            if(int.TryParse(number, out var parsed)){
                numbers.Add(parsed);
            }else{
                throw new System.Exception($"Couldn't parse number \"{number}\"!");
            }
        }
    }

    public override bool Equals (object obj) {
        if(obj is Version other){
            return this.CompareTo(other) == 0;
        }
        return false;
    }

    public override int GetHashCode () {
        return this.ToString().GetHashCode();
    }

    public override string ToString () {
        string output = string.Empty;
        for(int i=0; i<numbers.Count; i++){
            output += $"{numbers[i]}.";
        }
        return output.Substring(0, output.Length-1);
    }

    public int CompareTo (object obj) {
        if(obj is Version other){
            int max = Mathf.Max(this.numbers.Count, other.numbers.Count);
            for(int i=0; i<max; i++){
                int thisNum = (i < this.numbers.Count) ? this.numbers[i] : 0;
                int otherNum = (i < other.numbers.Count) ? other.numbers[i] : 0;
                if(thisNum > otherNum){
                    return 1;
                }else if(thisNum < otherNum){
                    return -1;
                }
            }
            return 0;
        }
        throw new ArgumentException($"Cannot compare \"{nameof(Version)}\" to \"{obj.GetType()}\"!");
    }

    public bool IsNewerThan (Version other) {
        return this > other;
    }

    public bool IsOlderThan (Version other) {
        return this < other;
    }

}
