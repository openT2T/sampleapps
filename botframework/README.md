# Open Translators to Things Sample Bot - Microsoft Bot Framework

Sample bot built with for the Microsoft Bot Framework: https://dev.botframework.com/

Uses translators to interact with device categories (schemas).

# Setup

1. You need to follow the instructions on the Microsoft Bot Framework site to create a bot registered with the Micrsoft Bot Framework
2. TODO: Luis.ai setup
3. Create the config/config.json file, and include the required information specific to your application.
   This is private information, and should not be committed to git (hence this file is in .gitignore)

```
{
    "deviceRegistryUrl": "https://github.com/openT2T/translators.git",
    "repoDir": "repo",
    "botframework": {
        "luis": "<from luis.ai console>"
    },
    "wink": {
        "clientId": "<from Wink>",
        "clientSecret": "<from Wink>"
    }
}
```

Finally, deploy this bot to a web URL, since the Bot Framework uses webhooks to communicate with this bot. Once your bot
is deployed, you need to set up the webhooks to point to the bot's URL in the Microsoft Bot Framework portal.

# Developing

During development, you can use http://ngrok.com (or other similar solution) to get a public URL to your in-development code on
your dev box. This will save you from having to constantly deploy in-progress work and speed up your dev lifecycle.

You can also use the Bot Framework Emulator as documented here: http://docs.botframework.com/connector/tools/bot-framework-emulator/
