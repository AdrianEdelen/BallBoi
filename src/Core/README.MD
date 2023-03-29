build instructions:

Make sure you have docker installed

In order to run the bot locally on a test server you need to follow the instruction on the discord developer portal for setting up an application and bot

Create a test guild/server in discord and invite the bot to it.

In the launchsetting.json, put your api key and guild ID.

when running in in visual studio with docker, the software uses the launch settings instead of the dockerfile env vars. 

run the container, the bot should show up online on your test server.


Submit PRs to dev Branch. And I will handle deploying and pipelining for now.
