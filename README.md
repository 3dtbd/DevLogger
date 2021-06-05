A DevLog is an important communication device. It ensures that team members are aware of the work
you have done, and have a record of how you did it. It can also be used for marketing purposes, 
keeping everyone informed of what you are working on and the progress made.

DevLogger is a Unity Plugin that helps you keep a DevLog while working on your project. The goal is to make it as easy as possible to create records of key milestones and to tell the world about them without interupting the development flow.

# Features

See [below](#using-devlogger) for usage guide

  * Capture in-game screenshots
  * Capture in-game animated GIFs (HDRP and URP not currently supported)
  * Capture in-editor windows image (scene view, hierarchy window etc.)
  * Post timed notes, with or without images, to a markdown DevLog
  * Create and manage tasks in your project 
  * Manage hashtags
  * Create Dev Log entries from Git commits
  * Post updates, with or without images (with or without animated GIFs), to Twitter
  * Post updates, with or without images (including animated GIFs) to Discord
  * Create schedules for posting and get reminders of when you are due to post
  * Open Source contributions welcome - lets be more productive together

# Installation Of Latest Release

This is the easiest way of installing the code:

  1. `Window -> Package Manager`
  2. Click the '+" in the top left
  3. Select 'Add package from Git URL'
  4. Paste in `https://github.com/TheWizardsCode/DevLogger.git#release/stable`
  
# Installation Of Development Code

We are a big fan of enabling our users to improve Dev Logger, so we would encourage you to use the source code, it's not much harder than using the latest release.

  1. Fork and clone the repo and submodules into your preferred location with `git clone --recurse-submodules [YOUR_FORK_URL]`
  2. In the project view select `Assets/DevTest PackageManifestConfig`
  3. In the inspector click `Export Package Source`, this will export the package to a folder next to your checkout director called "DevLogger-Package"
  4. To use this package in your development environments go to `Window -> Package Manager`
  5. Click the '+" in the top left
  6. Select 'Add package from disk ...'
  7. Point to the `package.json` file in the `DevLogger-Package` directory 
  
If you find a bug or want to make an improvement do it inside the DevLogger project in Unity. To make it available to your work projects repeat step 2 and 3 above. This will re-publish your package locally and will be automatically picked up when you next give your development environment focus. 

Once you have tested the changes please issue a pull request against our repo so we can make the code better for everyone.

# Using DevLogger

Open the Dev Logger window using `Tools -> Wizards Code -> Dev Logger`.

When you first open the window it will open on the settings tab so that you can setup the storage databases:

  1. Location to store your captured images will default to a directory in your project root. You can change this to anywhere on your machine in "Captures Save Folder". Using the checkboxes in this section you can optionally have your captures saved by project and scene subfolders. This is useful if you want to keep all your images in the same location as it will keep them separated by project and/or scene within the project.
  2. Create a DevLog Scriptable Object to organize your DevLog files. The easiest way to do this is simply click the "Create" button on the setup screen. The Scriptable Object will be created in the root of your Assets folder. You can move it if you want to.
  3. Create a Screen Capture Scriptable Object to organize your Screen Captures. The easiest way to do this is simply click the "Create" button on the setup screen. The Scriptable Object will be created in the root of your Assets folder. You can move it if you want to.
  
Once these steps are completed the Entry UI will be displayed.

## Entry Tab

The entry tab is where you will spend most of your time. It consists of the following sections:

### Posting

Once you have a valid entry created your avialable posting options, for DevLog, Discord and Twitter will appear here.

### Log Entry

Here you can create a new blank entry and enter short and long text for your log entry. You can also use this section for editing entries already posted to your log. We'll come back to that later.

The `Status` value is used to indicate that current status of this develog entry. You can use the status' for whatever purpose you desire, but here's how we use them:

  * Idea - Usually quick notes capturing an idea that comes to you during development. These are not typically full rounded out, just a quick note and maybe a screenshot capturing the thought.
  * ToDo - When an idea grows into something that seems to be fully formed we make it a ToDo item. ToDo items are things we intend to implement but have not scheduled yet.
  * In Progress - This is what we are working on right now.
  * Testing - We think this work is done but it needs thorough play testing. Typically these items will be shipped in beta builds for our early adopters offering feedback.
  * Done - Done and released to the main game.
  * Social - These are typically informational items used to inform our players about progress we made. We used to use DevLog entries for this but we started to use it more for planning and design notes and this we needed a separate DevLog item that only had the essential information and the pretty screenshots. 

Bear in mind that the short entry will be used when posting to Twitter. The long entry will be used in the Dev Log and on Discord posts.

### Meta Data

The meta data section allows you to define Hashtags, URLs, Git commit hashes and more that can be used alongside your descriptive text. Hashtags and URLs will be used in social media postings as well, so be aware that they will consume your character limit. To include a particular item in the list of meta-data items check the checkbox next to it.

To add a new item simply type a freeform value into the textbox and click "Add".

There is also a checkbox to indicate if this entry should be used in social amplification. This relates to scheduled social posts, see below for more.

### Media

This section shows thumbnails of the media files you have collected. Each thumbnail can be selected for inclusion in a develog or social media post. You can also view the images full size or open the media storage folder from here.

### Capture

When in Edit mode this section has a number of buttons enabling you to capture any of the currently open editor windows. The captures will appear in the Media section (above) when complete.

When in Run mode this section allows you to capture stills and animated gifs. These captures will also appear in the Media section. 

Animated Gifs will capture a defined number of seconds before the capture button was pressed. That is, pressing the button does not mark the start of the capture, rather it marks the end of the capture. The quality and duration of the capture is configurable in the editor window.

Note it takes a short while for animated gifs to be processed, they will not appear in the media section until processing is complete. When capturing a Gif the first frame will be saved as a PNG in addition to the GIF itself.

## DevLog Tab

Click the "View Tasks Markdown" button to open a Markdown version of your Idea, Tasks and Inprogress items. Click the "View DevLog Markdown" to see your Testing, Done and Social items in markdown form.

The main part of this panel shows a list of all your items grouped by category. You can reorder and edit description and meta data here, as well as delete entries. You can also click the "Edit" button to open up the entry in the main Entry Panel where you can add/remove screenshots and more.

Selecting an entry tahat matches the social criteria (e.g. has screenshots) enables buttons that at the top of the panel with the option to tweet and/or post to discord from this tab. Note that these buttons will only appear if these services are correctly configured, see below for more details.

## Schedule Tab

This is a Work In Progress and not fully operational. Feedback welcome.

You can setup a schedule for publishing your tweets and discord posts here. This is not fully integrated into the editor yet. At the time of writing you need
to create a Schedule Entry using `Create -> Wizards Code -> Dev Log Scheduled Event`. These will automatically appear in the Schedule tab. From there you can 
set the time, day, channels to post to and Dev Log entry to post.

Note that for a Dev Log Entry to be available in the list for a schedules event it must have the "Social" flag set.

For Twitter the short text, meta data and images will be used. You can also set special hashtags for the scheduled event (e.g. #ScreenshotSaturday). For Discord the Short and Long text plus, URLs (but not hashtags) from meta data will be used.

Posts are not automatically sent at this time, but when it is past time to send a new tweet/discord post a "Post" button will display. Clicking this button will post to Twitter and Discord as configured.

You can have as many scheduled events as you desire.

## Git Tab

The Git tab enables you to view the git logs for this project and quickly create DevLog entries from them. This is only tested on Windows, we welcome reports and patches for other platforms.

You can click a button on the log entries to copy the data over to the DevLog entry fields where you can complete a DevLog entry.

## Settings Tab

Here you can setup the DevLogger tool. We covered the database items above in the installation section. The camera field defines which camera is used to capture GIFs. This will default to the main camera, but you can override it if you so desire.

There are three options for trimming items from screenshots. These are here to allow you to include or exclude certain elements and also to ensure that you can use tools like the excellent Full Screen asset without compromising your screenshots.

### Discord Settings

If you want to post to Discord using DevLogger you will need to [setup a Discord Webhook](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks) as follows;

  1. Navigatge to your server in Discord
  2. Open the Settings page
  3. Click the Webhooks tab
  4. Click the "Create Webhook" button
  5. Choose the channel the webhook posts to
  6. Name the webhook
  7. Copy the Webhook URL
  8. Paste the URL into the configuration box in the DevLogger window

### Twitter Settings

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

We use [PackageTools](https://github.com/jeffcampbellmakesgames/unity-package-tools) to create our releases. To build a release:

  1. Update the version number in the `Release Candidate PackageManifestConfig` to match that in `DevTest PackageManifestConfig` (both are in the root of the `Assets` folder)
  2. Click `Generate VersionConstants.cs` in the inspector
  3. Increase the version number in the `DevTest PackageManifestConfig` to represent the next release number (not this release)
  4. Commit the new constants file to Git
  5. Alongside your working repository checkout the `release/stable` branch of this repo into a directory called `DevLogger-Release` using `git clone --single-branch --branch release/stable git@github.com:TheWizardsCode/DevLogger.git DevLogger-Release`
  6. Click `Export Package Source` in the inspector for the `Release Candidate PackageManifestConfig`
  7. Commit and push the changes in `DevLogger-Release` to GitHub [But SEE BELOW]

NOTE there is currently a [bug](https://github.com/jeffcampbellmakesgames/unity-package-tools/issues/11) in the package manager tool that prevents the above from working, at least on my machine. You can work around the bug with the following steps:

1. Delete the existing package directory
2. Export the package source
3. cd into the package directory
4. `git init`
5. `git remote add origin git@...`
6. `git fetch`
7. `git add .`
8. `git commit -m "Release v0.2.5`
9. `git branch -m master release/stable`
10. `git push -f -u origin release/stable`
