Onewheel Community Edition (OWCE) App
===========

A cross-platform app for use with the [Onewheel](https://onewheel.com/) V1, Plus, XR, Pint, Pint X and GT.

NOTE: GT support requires patching with [Rewheel](https://github.com/rewheel-app/rewheel).
The newer board firmware no longer sends through voltage, as Future Motion removed it. Those versions are:
- XR with firmware 4155 and higher
- Pint with firmware 5059 and higher
_ Pint X, all firmware
- GT, all firmware

NOTE: Onewheel Community Edition app is not endorsed by or affiliated with Future Motion in any way.

Written in C# with [.NET MAUI](https://dotnet.microsoft.com/apps/maui) (.NET 10)

Open Source Project by [@beeradmoore](http://www.twitter.com/beeradmoore) 

## Available (soon) for free on:
* Android and WearOS: Available on Google Play
* iPhone and Apple Watch: Available on App Store

## How to build and run this project.

The app is a single [.NET MAUI](https://learn.microsoft.com/dotnet/maui/) project (`OWCE/OWCE/OWCE.csproj`) targeting **.NET 10**. It replaces the old Xamarin.Forms solution (Xamarin reached end of support in May 2024).

### Prerequisites

1. The [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`global.json` pins the major version).
2. The MAUI workload for the platform(s) you want to build:
   ```bash
   dotnet workload install maui-android   # Android - Windows, macOS or Linux
   dotnet workload install maui-ios       # iOS - macOS only
   ```
3. A JDK (17 or newer) and the Android SDK for Android builds. If you do not have the Android SDK yet, the .NET Android tooling can install it for you:
   ```bash
   dotnet build OWCE/OWCE/OWCE.csproj -f net10.0-android -t:InstallAndroidDependencies -p:AndroidSdkDirectory=$HOME/android-sdk -p:AcceptAndroidSDKLicenses=True
   ```
   (Then pass the same `-p:AndroidSdkDirectory=...` to the builds below, or set `ANDROID_HOME`.)
4. For iOS you also need Xcode (the version required by your .NET SDK) on a Mac.

### Build

```bash
# Android (builds an APK/AAB, works on any OS)
dotnet build OWCE/OWCE/OWCE.csproj -f net10.0-android

# iOS (macOS only)
dotnet build OWCE/OWCE/OWCE.csproj -f net10.0-ios
```

To deploy to a connected Android device/emulator use `dotnet build OWCE/OWCE/OWCE.csproj -f net10.0-android -t:Run`. In Visual Studio or Rider open `OWCE/OWCE.sln` and pick the target framework from the run dropdown. Only the Android target is built by default on Windows and Linux; iOS is added automatically on macOS (or on Windows with a paired Mac via `-p:BuildIos=true`).

NOTE: Because the app depends on the Onewheels low-energy Bluetooth, it will not function correctly in a simulator/emulator. For best results, deploy to a physical device. Debug builds running in a simulator/emulator use `MockOWBLE` to replay sample ride data.

### Project layout

* `OWCE/OWCE/` - the app. Shared code (pages, views, board logic) lives here, platform code lives under `Platforms/Android` and `Platforms/iOS`.
* `OWCE/OWCE/Platforms/*` - Bluetooth (`OWBLE`), permission prompts and Apple Watch bridge per platform. They are registered with dependency injection in `MauiProgram.cs`.
* `OWCE/OWCE.WatchOS/` - the Apple Watch app. This is still a legacy Xamarin.watchOS project: .NET MAUI does not support watchOS apps, so it is not part of `OWCE.sln` and can not be built with the .NET 10 SDK. See [Documentation/WatchApplication.md](Documentation/WatchApplication.md).

## Frequently (or not so frequently) Asked Questions

### Why did you create this?

There are quite a number of members on the [Onewheel Owners Facebook group](https://www.facebook.com/groups/onewheelownersgroup/) that, for one reason or another, don't like the stock app by Future Motion. I figured, why not create an app with its development shaped by features that the community wants?

### Do other third-party apps already exist?

Yes, but Future Motion's firmware lockdowns prohibit them from being used. Such as pOnewheel[https://github.com/ponewheel/android-ponewheel] (deprecated) for Android and Float Deck (obsolete) for iOS. However, the problem is that one is for Android, and the other is for iOS. Wouldn't it be better if there was just 1 app with the exact same feature sets shared across both platforms?

### Does this change how my Onewheel performs?

No. This app uses the same Bluetooth low energy (BLE) interface that the official Onewheel app uses to read and display various stats.

### What Onewheels are supported?

Currently, v1, Plus, XR, and Pint. Pint X and GT have not yet been thoroughly tested.

### Will using this app void my warranty?

Although things such as riding your board without a helmet can void your warranty, we don't believe that using third-party apps will void your warranty.
