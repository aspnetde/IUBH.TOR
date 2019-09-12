/// Provides some constant values that are being
/// (re-)used across different parts of the
/// application.
module IUBH.TOR.Constants

open Xamarin.Forms

module Backgrounding =
    /// Note: Android requires us to schedule not below 15 minutes!
    /// See https://stackoverflow.com/a/46240470 for more information.
    [<Literal>]
    let FetchIntervalInMinutes = 15.0

    [<Literal>]
    let NotificationTitle = "IUBH TOR"
    
    [<Literal>]
    let NotificationText = "Your transcript of records got updated! 🥳"

module CARE =
    // Replace by https://care-fs.iubh.de/de/ in production
    [<Literal>]
    let LoginUrl = "http://localhost:6969/login"
    
    // Replace by https://care-fs.iubh.de/en/study/transcript-of-records.php?p_id=4280
    // in production
    [<Literal>]
    let TranscriptOfRecordsUrl = "http://localhost:6969/tor"

module UI =
    module Color =
        let NavigationBarBackground = Color.FromHex("#1B3A47")
        let NavigationBarText = Color.White
        
        let ActivityIndicator = Color.FromHex("#3B8E90")
        let EntryBackground = Color.White
        let ButtonBackground = Color.FromHex("#3B8E90")
        let PageBackground = Color.FromHex("#F5F5F5")
        let PlaceholderText = Color.FromHex("#757575")

    module FontSize =
        /// Hack to make the unit tests work. Otherwise we would have
        /// to initialize Xamarin.Forms first, which isn't possible
        /// without great ceremony. And as we don't care for the font
        /// sizes during tests, we simply cheat that way.
        let private namedSize name =
            try
                Device.GetNamedSize(name, typeof<Label>)
            with _ -> 10.0
        
        let Micro = namedSize NamedSize.Micro
        let Small = namedSize NamedSize.Small
