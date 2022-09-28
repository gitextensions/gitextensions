# Generating Gource Video

## Requirements

- [Chocolatey](https://docs.chocolatey.org/): To install  <https://chocolatey.org/install>
- Gource: Plugin that is downloaded from gitextensions plugins menu or chocolatey

```batch
choco upgrade gource -y
```

- [ffmpeg](https://ffmpeg.org/): Installed with chocolatey

```batch
choco upgrade ffmpeg -y
```

## Steps

1. Install needed requirments.  If using gitextensions plugins menu, no need to install gource.  It will be downloaded for you if needed.
1. Checkout whatever commit / branch you want to base the history off of or use ```--git-branch``` argument
1. Run gource

    ```batch
    gouurce --dir-name-depth 1 -logo "Logo\git-extensions-logo-24px.png" -s .06 -1280x720 --auto-skip-seconds .1 --multi-sampling --stop-at-end --key --hide mouse,progress,filenames,dirnames --file-idle-time 0 --max-files 0  --background-colour 000000 --font-size 22 --title "Your Title Here" -o gource.ppm --highlight-colour 00FF00 --multi-sampling --high-dpi --frameless --highlight-users
    ```

    - If you want to a see a video of only your contributions then add ```--user-show-filter``` like ```--user-show-filter "Jay Asbury"```
    - If you want to a see your contributions highlighted then add ```--highlight-user``` like ```--highlight-user "Jay Asbury"``` for example and remove ```--highlight-users```
    - use ```--git-branch branch name here``` to use a specific branch

    Best way to do this is to use plugins menu.  Commndline arguments can be found at <https://github.com/acaudwell/Gource>
    ![gource plugin](/assets/gource%20plugin.png)

1. Run ffmpeg

    ```batch
    ffmpeg.exe -y -r 60 -f image2pipe -vcodec ppm -i gource.ppm -vcodec libx264 -preset slow -pix_fmt yuv420p -crf 1 -threads 0 -bf 0 gource.x264.mp4
    ```

    - Play with preset argument and other args to adjust as needed.
    - :warning: both gource and ffmpeg generate large files.  I ran on release 3.5 and 4.0 branches and got these file sizes

    ```text
    Directory of C:\git\gitextensions

    09/26/2022  01:44 AM     2,448,248,037 gource3.5.x264.mp4
    09/26/2022  02:15 AM     2,001,686,002 gource4.0.x264.mp4

    Directory of C:\git\gitextensions

    09/26/2022  01:03 AM    49,830,278,768 gource3.5.ppm
    09/26/2022  01:10 AM    52,830,104,128 gource4.0.ppm

    Directory of C:\git\gitextensions

                4 File(s) 107,110,316,935 bytes
    ```
