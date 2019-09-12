namespace IUBH.TOR
{
    public static class Constants
    {
        // Replace by https://care-fs.iubh.de/de/ in production
        public const string CareLoginUrl = "http://localhost:6969/login";

        // Replace by https://care-fs.iubh.de/en/study/transcript-of-records.php?p_id=4280
        // in production
        public const string CareTranscriptOfRecordsUrl =
            "http://localhost:6969/tor";

        public const string InvalidCredentialsMessage =
            "Wrong user name or password. Please try again.";

        // Note: Android requires us to schedule not below 15 minutes!
        // See https://stackoverflow.com/a/46240470 for more information.
        public const int FetchIntervalInMinutes = 15;

        public const string NotificationTitle = "IUBH TOR";
        public const string NotificationText = "Your transcript of records got updated! 🥳";
    }
}
