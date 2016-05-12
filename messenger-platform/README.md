# Open Translators to Things Sample Bot - Messenger Platform

Sample bot built with for the Facebook Messenger Platform: https://developers.facebook.com/docs/messenger-platform/

Uses translators to interact with device categories.

# Setup

1. You need to follow the instructions on the Facebook Developers site to create an app enabled for the Messenger Platform.
2. Create a creds.js file in the same director as the app.js for this bot, and enter the page access token from the Facebook Developer Portal.

```
module.exports = {
    PAGE_ACCESS_TOKEN : '<page access token>'
}
```

Finally, deploy this bot to a web URL, since Facebook uses webhooks to communicate with this bot. Once your bot
is deployed, you need to set up the webhooks to point to the bot's URL in the Facebook Developer Portal.

# Developing

During development, you can use http://ngrok.com (or other similar solution) to get a public URL to your in-development code on
your dev box. This will save you from having to constantly deploy in-progress work and speed up your dev lifecycle.
