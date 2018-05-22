namespace AlexaSoundboard.Utils
{
    public static class Statics
    {
        // Intents
        public static string SoundIntent = "SoundIntent";
        public const string AmazonStopIntent = "AMAZON.StopIntent";
        public const string AmazonCancelIntent = "AMAZON.CancelIntent";
        public const string AmazonHelpIntent = "AMAZON.HelpIntent";

        // Messages
        public const string StopMessage = "Ok. I will hear you later.";
        public const string CancelMessage = "Sure.";
        public const string HelpMessage = "You can ask me to play a short soundclip and I will try to do my very best to play it for you. For example: Play Bazinga."; 

        public static string WelcomeMessage ="Welcome to the Alexa Soundboard. You can ask me to play a short soundclip and I will try to do my very best to play it for you.";

    }
}
