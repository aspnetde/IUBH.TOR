namespace IUBH.TOR

module AsyncResult =
    /// Binds two asynchronous functions, which are
    /// using Result, together.
    let bind a b =
        async {
            let! bR = b
            match bR with
            | Ok ok -> return! a ok
            | Error error -> return Error error
        }
