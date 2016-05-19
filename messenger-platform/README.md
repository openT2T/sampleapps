# Open Translators to Things Sample Bot - Messenger Platform

Sample bot built with for the Facebook Messenger Platform: https://developers.facebook.com/docs/messenger-platform/

Uses translators to interact with device categories.

# Setup

1. You need to follow the instructions on the Facebook Developers site to create an app enabled for the Messenger Platform.
2. Update the config.json file in the config directory with the required information specific to your application.
   This is private information, and should not be committed to git (hence this file is in .gitignore)

```
{
    "deviceRegistryUrl": "https://github.com/openT2T/translators.git",
    "repoDir": "repo",
    "facebook": {
        "pageId": "<from Facebook Developer Portal>",
        "verifyToken": "<from Facebook Developer Portal>",
        "pageAccessToken": "<from Facebook Developer Portal>",
        "witToken": "<from Wit.ai console>"
    },
    "wink": {
        "clientId": "<from Wink>",
        "clientSecret": "<from Wink>"
    }
}
```

Finally, deploy this bot to a web URL, since Facebook uses webhooks to communicate with this bot. Once your bot
is deployed, you need to set up the webhooks to point to the bot's URL in the Facebook Developer Portal.

# Developing

During development, you can use http://ngrok.com (or other similar solution) to get a public URL to your in-development code on
your dev box. This will save you from having to constantly deploy in-progress work and speed up your dev lifecycle.
