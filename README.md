# Bachelor's Thesis

This repository contains [the source code](https://github.com/aspnetde/IUBH.TOR/tree/master/src) of IUBH TOR, the application built for my Bachelor's thesis, along with [the LaTeX source](https://github.com/aspnetde/IUBH.TOR/tree/master/thesis) of [the thesis](https://github.com/aspnetde/IUBH.TOR/raw/master/compiled/thesis.pdf) itself.

<p align="center">
    <a href="https://github.com/aspnetde/IUBH.TOR/raw/master/compiled/thesis.pdf" title="Suitability of functional programming for the development of mobile applications: A comparison of F# and C# involving Xamarin">
        <img src="https://github.com/aspnetde/IUBH.TOR/raw/master/compiled/thesis.png" width="400" alt="Suitability of functional programming for the development of mobile applications: A comparison of F# and C# involving Xamarin" title="Suitability of functional programming for the development of mobile applications: A comparison of F# and C# involving Xamarin" />
    </a>
</p>

## Development notes

### Hot Reload for the C# App

For the C# app [HotReload](https://github.com/AndreiMisiukevich/HotReload) can be used by running

```
mono Xamarin.Forms.HotReload.Observer.exe u=http://127.0.0.1:8000
```

When testing on Android:

```
cd /Users/$USER/Library/Developer/Xamarin/android-sdk-macosx/
adb forward tcp:8000 tcp:8000
```

### Hot Reload for the F# App

It's called Live Update in the speak of Fabulous, see detailed information [here](https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html).

```
fabulous --watch --send
```

When testing on Android:

```
USB:
    adb -d forward  tcp:9867 tcp:9867
EMULATOR:
    adb -e forward  tcp:9867 tcp:9867
```

### Simulating Background Fetches

On Android:

```
adb shell cmd jobscheduler run -f de.iubh.tor 69
```

### Setting up iubh.tor.server

Both apps are intended to work with the real CARE system. However, iubh.tor.server provides a fast and reliable test environment that behaves exactly as the original CARE system. This means it expects the credentials in the same shape as the original login webpage, and it returns an HTML file that contains the transcript of records in the same style as CARE does.

To set it up, run

```
npm init
```

To run it, use

```
node index.js
```

There's also a Postman config located in /testdata.
