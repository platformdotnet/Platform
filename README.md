Platform.NET
===


A cross-platform library of useful classes and extensions for C# and .NET.

This library was initially developed between 2003-2008. The library updated to use modern C# syntax and released as open source under the BSD license. It is the foundation of my other C# projects including the projects located at the GitHub [Platform.NET](https://github.com/platformdotnet) organisation.

Notable classes:

 * `Platform.Collections.TimedReferenceDictionary`
 
 	A dictionary where the values are weakly references but guaranteed to not become weak for a set period of time after they were last touched.
 	
 * `Platform.Linq.ExpressionVisitor`
 
 	A class for building Linq providers.
 	
 * `Platform.Linq.ExtendedLambdaExpressionCompiler`
 
 	Compiles C# expression trees with support for the `Platform.Linq.MemberPopulateExpression` expression.
 	
 * `Platform.Network.Time.NetworkTimeClient` and `Platform.Network.Time.NtpNetworkTimeClient`
 
	An NTP client for .NET
 	
 * `Platform.References.NotifyingWeakReference`
 
 	A weak reference that raises an event when it has been freed
 
 * `Platform.References.TimedReference`
 
 	A reference that becomes weak when it has not been used for a specified period of time.
 	
 * `Platform.References.ReferenceQueue`
 
 	Java style reference queues
 	
 * `Platform.IModel`, `Platform.IValued`, `Platform.IMetered`, `Platform.IOwned`
 
 	Based generic model interfaces
 	
 * `Platform.ITask` and `Platform.AbstractTask`
 
 	The foundation of a classes that can be executed and monitored in the background. Tasks can be started, paused, stopped etc. These foundation classes make it very easy to create monitorable and controllable background tasks such as file downloaders, data sorters etc.
 	
 * `Platform.IO.InteractiveCryptoStream`
 
 	A cryptographic stream that supports interactivity (flushing mid-buffer).
 	
 * `Platform.IO.MeteringStream`
 
 	A wrapper around any stream that monitors read/write throughput and exposes these stats via the `IMeter` interface.
 	
 * `Platform.Text.TextConversion`
 
 	Provides useful string conversion methods supporting base32, base64, hex encoding, url encoding, soundex etc.
 	
 * `Platform.Utilities.InvocationQueue`
 
 	Manages a queue of `Action` objects that are executed one-after-another either on the main thread or in a background thread. The `InvocationQueue` is based on `ITask`. Think of it as an in-memory message processing queue.
 	
 * `Platform.Xml.Serialization.XmlSerializer<T>`
 
 	An advanced XmlSerializer designed to work around the limitations of the Microsoft XmlSerializer. The Microsoft XmlSerializer is designed to serialize objects to XML in a format that is intended to be deserialized only by the Microsoft XmlSerializer. This means it outputs extra decoration attributes etc. The Platform XmlSerializer is highly cusotmisable and is designed to serialize and deserialize clean cross-platform XML documents.
 	



---
Copyright © 2003-2013 Thong Nguyen (tumtumtum@gmail.com)

