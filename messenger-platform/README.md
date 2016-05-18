# Open Translators to Things Sample Bot - Messenger Platform

Sample bot built with for the Facebook Messenger Platform: https://developers.facebook.com/docs/messenger-platform/

Uses translators to interact with device categories.

# Setup

1. You need to follow the instructions on the Facebook Developers site to create an app enabled for the Messenger Platform.
2. Create a creds.js file in the same director as the app.js for this bot, and enter the required information specific to your application.
   This is private information, and should not be committed to git.

```
module.exports = {

   // Address where the server is deployed
    SERVER_URL : 'http://...',
    
    // Page ID (from Facebook Developer Portal)
    FB_PAGE_ID: '...',
    
    // Webhook verify token (from Facebook Developer Portal)
    FB_VERIFY_TOKEN: '...',
    
    // Page Access Token (from Facebook Developer Portal)
    PAGE_ACCESS_TOKEN : '...',
    
    // Wink API Client ID (from Wink)
    WINK_CLIENT_ID: '...',
    
    // Wink API Client Secret (from Wink)
    WINK_CLIENT_SECRET: '...',
    
    // Wit.ai access token for natural language conversations
    WIT_TOKEN: '...'
}
```

Finally, deploy this bot to a web URL, since Facebook uses webhooks to communicate with this bot. Once your bot
is deployed, you need to set up the webhooks to point to the bot's URL in the Facebook Developer Portal.

# Developing

During development, you can use http://ngrok.com (or other similar solution) to get a public URL to your in-development code on
your dev box. This will save you from having to constantly deploy in-progress work and speed up your dev lifecycle.
