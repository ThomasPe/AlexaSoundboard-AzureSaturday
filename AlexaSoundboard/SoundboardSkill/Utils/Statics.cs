namespace AlexaSoundboard.SoundboardSkill.Utils
{
    public static class Statics
    {
        // Intents
        public const string SoundIntent = "SoundIntent";
        public const string RandomSoundIntent = "RandomSoundIntent";
        public const string AmazonStopIntent = "AMAZON.StopIntent";
        public const string AmazonCancelIntent = "AMAZON.CancelIntent";
        public const string AmazonHelpIntent = "AMAZON.HelpIntent";

        // Magic Strings
        public static string SoundUrl = "https://alexasoundboard.blob.core.windows.net/sounds/{0}.mp3";

        // Messages
        public static string StopMessage = "Ok. I will hear you later.";
        public static string CancelMessage = "Sure.";
        public static string HelpMessage = "You can ask me to play a short soundclip and I will try to do my very best to play it for you. For example: Play Bazinga.";
        public static string WelcomeMessage = "Welcome to the Alexa Soundboard. You can ask me to play a short soundclip and I will try to do my very best to play it for you.";

        public static string NoSoundProvidedMessage = "I didn't catch the sound you want to hear. Please try again.";
        public static string SoundNotAvailableMessage = "Currently this sound isn't available. But I will try to get it. Please try it again later.";
        public static string SoundMessage = $"<speak><audio src='{SoundUrl}' /></speak>";
        public static string RandomSoundMessage = "<speak><audio src='{0}' /></speak>";        
    }
}
