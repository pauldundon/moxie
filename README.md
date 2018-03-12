Moxie is a very lightweight web server designed to mock-up Ajax APIs in a development environment. It is COMPLETELY UNSUITED FOR PRODUCTION ENVIRONMENTS. If your url doesn't start with localhost, you're in trouble.

### Getting Started

1. You'll need SQL Server and Visual Studio
2. Open the moxie Solution
3. Find the file 00.init.sql. Run in SQL Server Management Studio. It will create a database called Moxie.
4. Add a file connections.local.config to your project
5. Set the "Copy To Output Directory" property to "Copy Always"
6. Add a connection string something like the following

```
<?xml version="1.0" encoding="utf-8" ?>
<connectionStrings>
  <add name="moxie" connectionString="Data Source=myserver;Initial Catalog=Moxie;Integrated Security=True"
    providerName="System.Data.SqlClient" />
</connectionStrings>
```

You should now be able to run Moxie, and it will listen on port 904. You can change this by modifying the parameter passed to RunListener in Entrypoint.Main.

### Working with Documents

Moxie stores JSON documents against paths like /Customers/12345. You can create a document using POST or PUT. POST expects the path you use to be the name of a collection; if it can't find a collection, it will create one. PUT expects the path to be the full path of the document; if you PUT to /Customers/12345, Moxie will create a collection at /Customers if one doesn't already exist.

Both POST and PUT return the url of the created document.

If you GET a collection, Moxie returns a JSON document with an array called links. Each element in the array has an href property which is the full url of a document.

DELETE deletes a document, but does not remove collections.

### Static Documents

If you want to create documents without writing POST or PUT code, you can add them to the "response" folder. When Moxie receives a GET request, it looks in the response/get folder for a JSON file. Given a request for /Customers/HighRollers/12345?name=Benjamin, Moxie will look for
```
response/get/Customers/HighRollers/12345?name=Benjamin.json
response/get/Customers/HighRollers/12345.json
response/get/Customers/HighRollers.json
response/get/Customers.json
```
The same goes for POST requests, except that Moxie will look for these in response/post.

###Simulating CGI-like Processors

If Moxie finds a file in the response folder, it will open it and look for a "processor" property. If it finds this, it will take the value of this as a type name, and instantiate that type. Moxie assumes the type inherits from RequestHandler, and it will use it to process the request. There are two examples of this in the code.

