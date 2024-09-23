export class Flashcard {
    public term: string | undefined;
    public termAudio: string | undefined;
    public termBaseForm: string | undefined;

    public termTranslation: string | undefined;
    public termTranslationAudio: string | undefined;
    public termEnglishTranslation: string | undefined;

    public termDefinition: string | undefined;

    public context: string | undefined;
    public contextAudio: string | undefined;
    public contextTranslation: string | undefined;
    public contextTranslationAudio: string | undefined;
    public contextEnglishTranslation: string | undefined;

    public type: string | undefined;

    public imageCandidates: string[] | undefined;
    public selectedImageIndex: number | undefined;
}
