<p align="center">
  <img alt="App Logo" src="LectureCountdownApp/Assets/StoreLogo.scale-400.png" width="100" />

  <h1 align="center">Lecture Countdown</h1>

  <p align="center">
    A simple UWP countdown app for Windows 10/11.
  </p>

  <br />
</p>

![Issues](https://img.shields.io/github/issues/EmilJunker/LectureCountdown) 
![License](https://img.shields.io/github/license/EmilJunker/LectureCountdown) 
[![Crowdin](https://badges.crowdin.net/lecture-countdown/localized.svg)](https://crowdin.com/project/lecture-countdown)

## About The Project

Lecture Countdown is a must-have countdown app for students.

Does this sound familiar: You have been sitting in class for hours, are exhausted, and just want to head home already? Lecture Countdown is there to help you stay motivated! Keep it up and remain concentrated till the end! With the Lecture Countdown app, you can always see exactly how much longer the lecture will last - and how much time has already passed.

App Features:

- Always on Top mode
- See how much time has already passed
- Progress visualization in percent
- Notification when countdown is over
- Theme support (Dark/Light)

[<img src="https://getbadgecdn.azureedge.net/images/English_L.png"
      alt="Download app from the Microsoft Store"
      height="80">](https://www.microsoft.com/store/productId/9P4NPSWTX7LK)

## Getting Started

To build the app from source, follow these instructions.

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the UWP Development Kit

### Installation

To build and run the project, open the `.sln` file in Visual Studio, set LectureCountdownApp as the startup project (via right-click menu in in the solution explorer) and press F5 on the keyboard.

## Usage

You can launch the app from the command line to pass parameters. Note that the App execution alias for `LectureCountdown.exe` must be enabled in Windows Settings for this to work.

```powershell
> LectureCountdown --length 20
```

### Available Parameters:

```
--length                length of the countdown in minutes
--end-time              end time of the countdown in format hh:mm (with optional AM/PM)
--description           description of the countdown (wrap in quotation marks if necessary)
--notification          notification mode (accepts "none", "silent", "sound", or "alarm")
--notification-sound    notification sound to be played (= position of the sound in the dropdown menu)
--compact-mode          pass this parameter to launch the app in compact mode
--cancel                pass this parameter to cancel the current countdown
```

### Examples:

```powershell
> LectureCountdown --length 20 --description "My 20-minute countdown" --compact-mode
```

```powershell
> LectureCountdown --end-time 09:30 --notification alarm --notification-sound 6
```

```powershell
> LectureCountdown --cancel
```

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

### Creating A Pull Request

1. Fork the Project
2. Create your Feature Branch (`git checkout -b amazing-feature`)
3. Commit your Changes (`git commit -m 'Add some Amazing Feature'`)
4. Push to the Branch (`git push origin amazing-feature`)
5. Open a Pull Request

### Helping with Translation

This project is localized using [Crowdin](https://crowdin.com/project/lecture-countdown).

## Privacy Policy

See [PRIVACY.md](https://github.com/EmilJunker/LectureCountdown/blob/main/PRIVACY.md) for more information.

## License

Distributed under the MIT License. See [LICENSE.txt](https://github.com/EmilJunker/LectureCountdown/blob/main/LICENSE.txt) for more information.

## Donations

If you enjoy using this app and would like to support me so I can dedicate more time to open source projects like this, here is my [PayPal link](https://www.paypal.me/EmilJunker) - Thanks!

## Acknowledgements

* [Windows Community Toolkit](https://github.com/CommunityToolkit/WindowsCommunityToolkit)
* [Arabic Numbers Converter](https://github.com/EmilJunker/ArabicNumbersConverter)
* [Command Line Parser Library for CLR and NetStandard](https://github.com/commandlineparser/commandline)
