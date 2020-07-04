A DevLog is an important communication device. It ensures that team members are aware of the work
you have done, and have a record of how you did it. It can also be used for marketing purposes, 
keeping everyone informed of what you are working on and the progress made.

DevLogger is a Unity Plugin that helps you keep a DevLog while working on your project. The goal is to make it as easy as possible to create records of key milestones and to tell the world about them without interupting the development flow.

# Features

  * Capture in-game screenshots
  * Capture in-game animated GIFs
  * Capture in-editor windows image (scene view, hierarchy window etc.)
  * Post timed notes, with or without images, to a markdown DevLog
  * Post update, with or without images, to Twitter
  * Open Source contributions welcome - lets be more productive together

# Installation Of Latest Release

  1. `Window -> Package Manager`
  2. Click the '+" in the top left
  3. Select 'Add package from Git URL'
  4. Paste in `https://github.com/TheWizardsCode/DevLogger.git#release/stable`
  
# Installation Of Devlopment Code

  1. Clone the repo into your preferred location
  2. `Window -> Package Manager`
  3. Click the '+" in the top left
  4. Select 'Add package from disk ...'
  5. Point to the directory containing your development

## Twitter Setup

If you want to tweet from within DevLogger you will need to setup authentication
for Twitter but following these steps:

  1. Create a developer account on Twitter http://dev.twitter.com
  2. Register DevLogger at http://dev.twitter.com/apps/new
  3. Get the Consumer Token and Secret for the app
  4. Generate an Access Token and Access Token Secret
  5. In the DevLogger window expand the Twitter section
  6. Enter the consumer key and secret as well as the Access token and secret
  7. The twitter section will change to the twitter controls

# Release Process

We use [PackageTools](https://github.com/3dtbd/unity-package-tools) to create our releases. To build a release:

  0. Alongside your working repository checkout the `release/stable` branch of this repo
  1. Update (at least) the version number in the `PackageManifestConfig` in the root of the `Assets` folder
  2. Click `Generate VersionConstants.cs` in the inspector
  3. Commit the new constants file to Git
  4. Click `Export Package Source`
  5. Commit and push the changes in your release project to GitHub

# Using DevLogger

## Capture an in-game ScreenShot or Animated GIFs

When in play mode you can capture screenshots or animated GIFs right from within
the editor. GIFs will capture a portion of the most recent gameplay together with
a still from the middle of that segment. Stills will be captured at the point the
button is pressed.

  1. Setup the view that you want to capture in the game window
  2. Click the "Game View" or "Animated GIF" button

Captured images are displayed in the `Media Capture` section for use in DevLog and Twitter
entries.

Screenshots are stored in the `DevLog` folder in the root of your project.

## Capture In-Editor Screenshots

When not on play mode you can capture shots of editor windows and then use those images in
DevLog entries.

  1. Simply hit one of the buttons in the "Media Capture"

## Record a Log Entry

Log entries are stored in files in the `DevLog` folder in the root of your project.
In summary:

  1. Enter your log entry text into the field. 
  2. Click "Tweet with text only" or "Tweet with text and image"
  3. If tweeting with an image select the desired image in the file selection dialog

To add an entry to the DevLog type a short description into the Log Entry text box.
DevLogs are supposed to be short at this point. The goal is to record a quick status
update so you know what you did and when. Click either the `DevLog (no tweet) with 
text only` or the `DevLog (no Tweet) with selected image and text` button to record
the DevLog in the file.

Note that, if your log entry is short enough, you can also post a DevLog as a Tweet.
Doing this (see below) will automatically record the entry in your DevLog file as
well as send it to Twitter.

If you choose the `with image` option then the image selected in the `Media Capture`
section is linked to the entry. See below for details on how you capture screenshots
and animated GIFs.

## Send a Tweet

As noted above you can optionally send a DevLog entry to Twitter as well as record it
in your local DevLog file. When sending the entry to twitter appropriate hashtags will
also be added. As with DevLog entries you can optionally include a still image or an 
animated GIF.

  1. Enter your log entry text into the field. 
  2. Click "Tweet with text only" or "Tweet with text and image"
  3. If tweeting with an image select the desired image in the file selection dialog

The text entered must be shorter than the 140 chars minus the length of 
the selected hashtags. If it is too long the tweet buttons will not be 
available to you.


# Known Issues

Here are the known issues we are tracking and intend to fix (patches welcome to speed things up).
If you are facing an issue that is not in this list but it blocking work please open a GitHub issue
and report it - preferably with a patch to fix it, but don't worry, just telling us it is important
to you is helpful.

  * No known breaking bugs at this time (of course there are bugs)




