JSON in Java, for C#
Partially written and assembled by Kevin Jenkins ( rakkar@jenkinssoftware.com )
Parsing code is by Procurios, used under the MIT license from http://techblog.procurios.nl/k/618/news/view/14605/14863/how-do-i-write-my-own-parser-(for-json).html
Interface and structure of "JSON in Java" by Douglas Crockford (douglas@crockford.com), see http://www.json.org/java/index.html used under the JSON License http://www.json.org/license.html

The "JSON in Java" ported to Unity C#. It is the same library used by Android Java, allowing copy/paste between native code and script. Reasonably fast, lightweight, and properly handles nested quotes.

---------- Example usage ----------

JSONObject jo1 = new JSONObject();
jo1.put("count", 1);
jo1.increment("count");
Debug.Log("Should print 2: " + jo1.getInt("count"));

JSONObject jo2 = new JSONObject("{\"quote\": \"He said, \\\"I like JSON\\\"\"}");
Debug.Log("Should print an escaped quote: " + jo2.toString());

JSONArray ja1 = new JSONArray();
ja1.put(jo1);
ja1.put(jo2);

Debug.Log("Should print count 2, and an escaped quote: " + ja1.toString());

JSONObject jo3 = ja1.getJSONObject(1);
Debug.Log("Should print the same escaped quote: " + jo3.toString());

string twitterTest = "{\"errors\": [{\"message\": \"The Twitter REST API v1 is no longer active. Please migrate to API v1.1. https://dev.twitter.com/docs/api/1.1/overview.\", \"code\": 68}]}";
JSONObject jo4 = new JSONObject(twitterTest);
JSONArray ja2 = jo4.getJSONArray("errors");
string message = ja2.getJSONObject(0).getString("message");
int code = ja2.getJSONObject(0).getInt("code");
Debug.Log(message + " code: " + code);

For comprehensive documentation, see http://www.json.org/java/index.html