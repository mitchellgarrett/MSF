# Mitchell's Serializable Format (MSF)

An object serializable format similar to JSON I designed for use in my own projects.
Built using C#.

To create an MSFObject call 'x = MSF.Serialize(source)' where 'source' is a string containing the MSF source code.

To access the nodes in an object call 'x[key]' where 'key' is a string key matching the node you wish to access.

An MSFNode contains a string 'Key' and an object 'Value' which can either be an integer, string, bool, MSFObject, or List.
 
 To serialize an MSFObject back to a string call 'MSF.Serialize(x)' or simply 'x.ToString()'.
