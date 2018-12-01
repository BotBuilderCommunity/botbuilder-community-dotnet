# BestMatch Middleware Sample

This sample demonstrates using the BestMatchMiddleware class to define your own piece of middleware to intercept incoming messages and respond to them if needed.

In this example we create a class called CommonResponsesMiddleware, which will look 
for incoming messages with common phrases like "hi", "bye", "thank you", and 
respond to them if a match is found.  If not then the middleware will let the 
message through and the main bot will handle the message by simply 
repeating what the user said back to them.

