# [Video-Processing](https://marketplace.uipath.com/listings/video-processing7305)
[![Build status](https://ci.appveyor.com/api/projects/status/3q6lf6pkt06t0uny/branch/main?svg=true)](https://ci.appveyor.com/project/k2zinger/video-processing/branch/main)


Extract frames and audio from mp4 files


## Installation
Studio -> Manage Packages -> Community -> (Search) Video Processing -> pick the package(s) to install and click Install -> Save

## Activities
`Extract Audio - MP3`: Extracts audio from an mp4 file and saves it as an mp3 file

`Extract Frames - JPEG`: Extracts image frames from an mp4 file and saves the jpeg images to a folder

## Prerequisites
`FFmpeg`: Download and install [FFmpeg](https://ffmpeg.org/), and then specify the installation location within the activities properties.  E.g. c:\FFmpeg (the activity looks for c:\FFmpeg\bin\ffmpeg.exe)
