using System.Text;

using Random = UnityEngine.Random;
using Mathf = UnityEngine.Mathf;

public static class RandomTextGenerator {

	public struct Distribution {

		public static Distribution WordLengthDefault = new Distribution(1, 6, 13);
		public static Distribution WordSequenceLengthDefault = new Distribution(3, 7, 13);
		public static Distribution ParagraphLengthDefault = new Distribution(3, 5, 10);

		public readonly int min;
		public readonly int mid;
		public readonly int max;

		public Distribution (int min, int mid, int max) {
			this.min = min;
			this.mid = mid;
			this.max = max;
		}

		public int EvaluateRandom () {
			float randomValue = Random.value;
			if(randomValue < 0.5f){
				float t = 2f * randomValue;
				return Mathf.Clamp(Mathf.FloorToInt(Mathf.Lerp(min, mid + 1, t)), min, mid);
			}else{
				float t = 2f * (randomValue - 0.5f);
				return Mathf.Clamp(Mathf.FloorToInt(Mathf.Lerp(mid, max + 1, t)), mid, max);
			}
		}

	}

	private static int upperToLower = 32;
	private static char[] uppercaseVowels = new char[]{'A', 'E', 'I', 'O', 'U'};
	private static char[] uppercaseConsonants = new char[]{'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z'};

	public static char GetRandomCharacter (float vowelChance, bool capitalized = false) {
		char[] charArray;
		if(Random.value < vowelChance){
			charArray = uppercaseVowels;
		}else{
			charArray = uppercaseConsonants;
		}
		return (char)(charArray[Random.Range(0, charArray.Length)] + (capitalized ? 0 : upperToLower));
	}

	public static char GetRandomCharacter (bool capitalized = false) {
		int output = Random.Range('A', 'Z'+1);
		if(!capitalized){
			output += upperToLower;
		}
		return (char)output;
	}

	public static string GetRandomWord (bool capitalized = false) {
		return GetRandomWord(Distribution.WordLengthDefault, capitalized);
	}

	public static string GetRandomWord (Distribution wordSettings, bool capitalized = false) {
		int numberOfLetters = wordSettings.EvaluateRandom();
		StringBuilder sb = new StringBuilder();
		if(numberOfLetters > 0){
			sb.Append(GetRandomCharacter(0.2f, capitalized));
			for(int i=1; i<numberOfLetters; i++){
				sb.Append(GetRandomCharacter(0.8f, false));
			}
		}
		return sb.ToString();
	}

	public static string GetRandomWordSequence (bool capitalized = false) {
		return GetRandomWordSequence(Distribution.WordSequenceLengthDefault, capitalized);
	}

	public static string GetRandomWordSequence (Distribution sequenceSettings, bool capitalized = false) {
		int numberOfWords = sequenceSettings.EvaluateRandom();
		StringBuilder sb = new StringBuilder();
		if(numberOfWords > 0){
			sb.Append(GetRandomWord(capitalized));
			for(int i=1; i<numberOfWords; i++){
				sb.Append(" ");
				sb.Append(GetRandomWord(false));
			}
		}
		return sb.ToString();
	}

	public static string GetRandomParagraph () {
		return GetRandomParagraph(Distribution.ParagraphLengthDefault);
	}

	public static string GetRandomParagraph (Distribution paragraphSettings) {
		int numberOfSentences = paragraphSettings.EvaluateRandom();
		StringBuilder sb = new StringBuilder();
		for(int i=0; i<numberOfSentences; i++){
			sb.Append(GetRandomWordSequence(true));
			sb.Append(". ");
		}
		return sb.ToString();
	}

}
