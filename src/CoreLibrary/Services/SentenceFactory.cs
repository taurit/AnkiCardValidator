using CoreLibrary.Interfaces;

namespace CoreLibrary.Services;

public class SentenceFactory(IWordTokenizer wordTokenizer)
{
    public Sentence BuildSentence(string text, Sentence? previousSentence)
    {
        var words = wordTokenizer.GetWords(text);
        return new Sentence(text, words, previousSentence);
    }
}
