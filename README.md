#Handling concurrency in an ASP.NET Core Web API with Dapper

**So, an app consuming this API would:**

- get the product from the API
- display it on a page
- allow the user to make changes to the product
- submit the updated product to the API

**A solution**

A solution is to check that the product hasn’t changed between an app getting it and submitting changes to it. If the product isn’t up-to-date, then another user has been making changes at the same time, and the changes can be rejected.

How can we check whether the product has changed though? Well, if the product is persisted in SQL Server, we can use rowversion. rowversion is a mechanism for automatically version-stamping table rows. If we add a field of type rowversion, SQL Server will automatically change the value of this field every time a change occurs in that row.

If we include the value from this rowversion field in the GET request for a product and require it in the PUT request, we can check if the product has changed before making the database update.

**Adding a product version**
We are going to add a Version field to the Product table so that we can enforce a request must have the latest version of the product to change it:

_ALTER TABLE Product
ADD Version rowversion_

Before we implement the additional code in the web API, let’s experiment with this new field:
If we select a product, we see that SQL Server has given Version an initial value:

Product version 1

If we update and select the product again, we see that SQL Server has updated the Version:

Product version 2

Nice!

So, let’s add Version to our Product model in our web API which needs to be a byte array:

_public class Product
{
  ...
  public byte[] Version { get; set; }
}_
It is worthing noting that ASP.NET core automatically converts the byte array to a base64 encoded string during model binding. We can see this if we make a GET request for a product:

Get product

**Cool!**
